using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using SemanticKernel.Models;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SemanticKernel
{
    public record QueryRequest(string Query, List<ChatMessageContent> History);

    public class Program
    {
        private static async Task WriteSSEMessage(HttpResponse response, object data)
        {
            var json = data is string str ? str : JsonSerializer.Serialize(data);
            var bytes = Encoding.UTF8.GetBytes($"data: {json}\n\n");
            await response.Body.WriteAsync(bytes);
            await response.Body.FlushAsync();
        }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Add essential services
            builder.Services.AddHttpClient();

            // 2. Configure AI services
            builder.Services.AddKeyedSingleton<IChatCompletionService>(
                "OpenAI_Translation",
                new OpenAIChatCompletionService(
                    modelId: "gpt-4",
                    apiKey: Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? "sk-your-openai-api-key"
                )
            );

            builder.Services.AddKeyedSingleton<IChatCompletionService>(
                "QWen3_Intent",
                new OpenAIChatCompletionService(
                    modelId: "qwen3-72b-chat",
                    apiKey: Environment.GetEnvironmentVariable("QWEN_API_KEY") ?? "your-qwen3-api-key",
                    endpoint: new Uri("https://api.qwen.com/v1")
                )
            );

            builder.Services.AddKeyedSingleton<IChatCompletionService>(
                "Ollama_Extraction",
                new OpenAIChatCompletionService(
                    modelId: "llama3",
                    apiKey: "ollama",
                    endpoint: new Uri("http://localhost:11434/v1")
                )
            );

            // 3. Register application services
            builder.Services.AddSingleton<ChatHistoryService>();
            builder.Services.AddSingleton<MultiAgentService>();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            // 1. 原始SSE流式响应接口
            app.MapPost("/api/QueryByStream", async (
                HttpContext context,
                QueryRequest request,
                MultiAgentService agentService,
                CancellationToken cancellationToken) =>
            {
                context.Response.Headers.Append("Content-Type", "text/event-stream");
                context.Response.Headers.Append("Cache-Control", "no-cache");
                context.Response.Headers.Append("Connection", "keep-alive");

                try 
                {
                    // 1. 获取第三方API的原始响应流
                    using var apiStream = await agentService.GetDirectStreamResponseAsync(request.Query);
                    
                    // 2. 按行读取并直接转发原始SSE数据
                    using var reader = new StreamReader(apiStream);
                    string? line;
                    
                    while ((line = await reader.ReadLineAsync()) != null)
                    {
                        // 如果是 [DONE] 消息，跳过
                        if (line.Contains("\"content\": \"[DONE]\""))
                        {
                            continue;
                        }
                        
                        // 直接写入行数据和换行符
                        byte[] lineBytes = Encoding.UTF8.GetBytes(line + "\n");
                        await context.Response.Body.WriteAsync(lineBytes, 0, lineBytes.Length, cancellationToken);
                        await context.Response.Body.FlushAsync(cancellationToken);
                    }

                    // 3. 继续处理suggestion数据流
                    await foreach (var message in agentService.ProcessQueryStreamAsync(request.Query, request.History)
                        .WithCancellation(cancellationToken))
                    {
                        if (message.Content != "[DONE]")
                        {
                            await WriteSSEMessage(context.Response, new OpenAIStreamResponse
                            {
                                choices = new[]
                                {
                                    new OpenAIStreamResponse.Choice
                                    {
                                        delta = new OpenAIStreamResponse.Delta
                                        {
                                            content = message.Content
                                        }
                                    }
                                }
                            });
                        }
                    }

                    // 4. 最后发送完成标记
                    await WriteSSEMessage(context.Response, "[DONE]");
                }
                catch (Exception ex)
                {
                    // 发送错误消息并关闭流
                    await WriteSSEMessage(context.Response, new OpenAIStreamResponse
                    {
                        choices = new[]
                        {
                            new OpenAIStreamResponse.Choice
                            {
                                delta = new OpenAIStreamResponse.Delta
                                {
                                    content = $"Error: {ex.Message}"
                                }
                            }
                        }
                    });
                    await WriteSSEMessage(context.Response, "[DONE]");
                }
            });

            // 2. 使用SSE工具类的流式响应接口
            app.MapPost("/api/QueryByStreamWithIAsyncEnumerableObject", async (
                HttpContext context,
                QueryRequest request,
                MultiAgentService agentService,
                CancellationToken cancellationToken) =>
            {
                static async IAsyncEnumerable<OpenAIStreamResponse> StreamResponses(
                    MultiAgentService service,
                    string query,
                    List<ChatMessageContent> history,
                    [EnumeratorCancellation] CancellationToken cancellationToken)
                {
                    // 发送初始响应
                    yield return new OpenAIStreamResponse
                    {
                        choices = new[]
                        {
                            new OpenAIStreamResponse.Choice
                            {
                                delta = new OpenAIStreamResponse.Delta()
                            }
                        }
                    };

                    // 处理消息流
                    await foreach (var message in service.ProcessQueryStreamAsync(query, history)
                        .WithCancellation(cancellationToken))
                    {
                        if (message.Content == "[DONE]")
                        {
                            yield break;
                        }

                        yield return new OpenAIStreamResponse
                        {
                            choices = new[]
                            {
                                new OpenAIStreamResponse.Choice
                                {
                                    delta = new OpenAIStreamResponse.Delta
                                    {
                                        content = message.Content
                                    }
                                }
                            }
                        };
                    }
                }

                context.Response.Headers.Append("Content-Type", "text/event-stream");
                context.Response.Headers.Append("Cache-Control", "no-cache");
                context.Response.Headers.Append("Connection", "keep-alive");

                try
                {
                    var responseStream = StreamResponses(agentService, request.Query, request.History, cancellationToken);
                    
                    await foreach (var response in responseStream.WithCancellation(cancellationToken))
                    {
                        // 使用SSE工具类格式化消息
                        var json = JsonSerializer.Serialize(response);
                        var message = $"{SSE.DataPrefix}{json}";
                        var bytes = Encoding.UTF8.GetBytes(message + "\n\n");
                        await context.Response.Body.WriteAsync(bytes, cancellationToken);
                        await context.Response.Body.FlushAsync(cancellationToken);
                    }

                    // 发送完成标记
                    var doneBytes = Encoding.UTF8.GetBytes(SSE.DoneMessage + "\n\n");
                    await context.Response.Body.WriteAsync(doneBytes, cancellationToken);
                    await context.Response.Body.FlushAsync(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    // 客户端断开连接，正常退出
                }
            });

            await app.RunAsync();
        } 
    }
   
}
        
 
