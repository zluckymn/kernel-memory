using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using SemanticKernel.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading.Channels;

public class MultiAgentService
{
    private readonly Kernel _translationKernel;
    private readonly Kernel _intentKernel;
    private readonly Kernel _extractionKernel;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ChatHistoryService _chatHistoryService;

    public MultiAgentService(
        [FromKeyedServices("OpenAI_Translation")] IChatCompletionService translationService,
        [FromKeyedServices("QWen3_Intent")] IChatCompletionService intentService,
        [FromKeyedServices("Ollama_Extraction")] IChatCompletionService extractionService,
        IHttpClientFactory httpClientFactory,
        ChatHistoryService chatHistoryService)
    {
        // Correctly build each Kernel instance by adding the specific service to a new builder.
        var translationBuilder = Kernel.CreateBuilder();
        translationBuilder.Services.AddSingleton(translationService);
        _translationKernel = translationBuilder.Build();

        var intentBuilder = Kernel.CreateBuilder();
        intentBuilder.Services.AddSingleton(intentService);
        _intentKernel = intentBuilder.Build();

        var extractionBuilder = Kernel.CreateBuilder();
        extractionBuilder.Services.AddSingleton(extractionService);
        _extractionKernel = extractionBuilder.Build();

        _httpClientFactory = httpClientFactory;
        _chatHistoryService = chatHistoryService;
    }

    public IAsyncEnumerable<ChatMessageContent> ProcessQueryStreamAsync(string query, List<ChatMessageContent> history)
    {
        return ProcessQueryStreamInternalAsync(query, history);
    }

    private async IAsyncEnumerable<ChatMessageContent> ProcessQueryStreamInternalAsync(string query, List<ChatMessageContent> history)
    {
        var compressedHistory = await _chatHistoryService.ReduceHistoryAsync(history);
        var fullQueryContext = $"History: {compressedHistory}\nUser Query: {query}";

        var translationResult = await _translationKernel.InvokePromptAsync(
            $"Translate the user query into English: {query}");
        yield return new ChatMessageContent(AuthorRole.Assistant, $"Translation (OpenAI): {translationResult.GetValue<string>()}");

        var intentSettings = new OpenAIPromptExecutionSettings { ResponseFormat = "json_object" };
        var intentResult = await _intentKernel.InvokePromptAsync(
            "Respond with a JSON object containing the user's intent from this list: [DocumentSearch, GeneralChat]. Query: {{$input}}",
            new(intentSettings) { { "input", fullQueryContext } });
        yield return new ChatMessageContent(AuthorRole.Assistant, $"Intent (QWen3): {intentResult.GetValue<string>()}");

        string intent = "GeneralChat";
        string? parseError = null;
        
        try
        {
            var intentJson = JsonDocument.Parse(intentResult.GetValue<string>()!);
            if (intentJson.RootElement.TryGetProperty("intent", out var intentProp))
            {
                intent = intentProp.GetString() ?? "GeneralChat";
            }
        }
        catch (JsonException)
        {
            parseError = "Warning: Could not parse intent JSON.";
        }

        if (parseError != null)
        {
            yield return new ChatMessageContent(AuthorRole.Assistant, parseError);
            yield break;
        }

        if (intent != "DocumentSearch")
        {
            yield return new ChatMessageContent(AuthorRole.Assistant, "I can only help with document searches right now.");
            yield break;
        }

        var extractionResult = await _extractionKernel.InvokePromptAsync(
            $"Extract the key document title from the query: {query}");
        var documentTitle = extractionResult.GetValue<string>()?.Trim();
        yield return new ChatMessageContent(AuthorRole.Assistant, $"Extraction (Ollama): {documentTitle}");

        yield return new ChatMessageContent(AuthorRole.Assistant, $"Starting document search for: {documentTitle}");
        
        await foreach (var message in StreamResponseAsync(documentTitle ?? ""))
        {
            yield return message;
        }

        _chatHistoryService.SaveHistory(query, "Final document stream sent.");
        yield return new ChatMessageContent(AuthorRole.Assistant, "Process complete. History saved.");
    }

    private async IAsyncEnumerable<ChatMessageContent> StreamResponseAsync(string documentTitle)
    {
        var channel = Channel.CreateUnbounded<ChatMessageContent>();
        var client = _httpClientFactory.CreateClient();

        // 启动异步任务来处理响应流
        _ = ProcessResponseStream(documentTitle, channel.Writer, client);

        // 从channel读取消息并yield return
        await foreach (var message in channel.Reader.ReadAllAsync())
        {
            yield return message;
        }
    }

    public async Task<Stream> GetDirectStreamResponseAsync(string query)
    {
        var client = _httpClientFactory.CreateClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/v1/complete")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = "gpt-3.5-turbo",
                messages = new[] { new { role = "user", content = query } },
                stream = true
            }), Encoding.UTF8, "application/json")
        };

        var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStreamAsync();
    }

    private async Task ProcessResponseStream(string documentTitle, ChannelWriter<ChatMessageContent> writer, HttpClient client)
    {
        HttpResponseMessage? response = null;
        
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/v1/complete")
            {
                Content = new StringContent(JsonSerializer.Serialize(new 
                {
                    model = "gpt-3.5-turbo",
                    messages = new[] { new { role = "user", content = documentTitle } },
                    stream = true
                }), Encoding.UTF8, "application/json")
            };

            response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync();
            
            // 使用SSE工具类解析流
            await foreach (var streamResponse in SSE.ParseStreamAsync<OpenAIStreamResponse>(stream))
            {
                if (streamResponse?.choices is { Length: > 0 })
                {
                    var choice = streamResponse.choices[0];
                    if (choice.delta?.content is string content && !string.IsNullOrEmpty(content))
                    {
                        await writer.WriteAsync(new ChatMessageContent(AuthorRole.Assistant, content));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await writer.WriteAsync(new ChatMessageContent(AuthorRole.Assistant, $"API Error: {ex.Message}"));
        }
        finally
        {
            response?.Dispose();
            writer.Complete();
        }
    }
    } 
 