# Semantic Kernel 入门手册 (增强版)

## 1. 简介

Semantic Kernel (SK) 是一个由微软开源的软件开发工具包（SDK），它旨在将大语言模型（LLM）的强大能力与传统编程语言（如 C#）无缝集成。通过 SK，开发者可以轻松地创建、管理和编排 AI 功能，构建能够调用现有代码的复杂 AI 应用。

本手册旨在帮助企业开发人员快速理解 SK 的核心概念，并提供一个可直接用于 POC 项目的复杂示例，展示如何落地一个可用的 AI Agent 服务。

## 2. 核心概念

### 2.1 Kernel (内核)

Kernel 是 SK 的核心与灵魂。它如同一个“AI 编排器”，负责加载配置、管理插件、调用函数、串联服务，是驱动所有 AI 功能的中枢。

### 2.2 插件 (Plugins)

插件是封装了一组相关功能的模块，是 Kernel 执行具体任务的“工具箱”。插件中可以包含两种类型的函数：

*   **语义函数 (Semantic Functions)**: 基于自然语言提示（Prompt）的函数，用于执行需要 LLM 理解和生成的任务。
*   **原生函数 (Native Functions)**: 用 C# 编写的传统代码，用于执行精确的计算、数据操作或与外部系统（如 API、数据库）的交互。

### 2.3 记忆 (Memory)

记忆赋予了 Kernel “长期知识”和“短期上下文”的能力。它通过将文本信息转换为向量（Embeddings）并存储在向量数据库中，来实现语义层面的信息检索。

### 2.4 连接器 (Connectors)

连接器是 Kernel 与外部世界沟通的桥梁。通过它，Kernel 可以接入并使用各种服务，如 AI 模型、向量数据库等。

### 2.5 规划器 (Planner)

规划器能根据用户的最终目标，自动地从已加载的插件中选择合适的函数，并编排成一个可执行的计划（Plan）。

## 3. 框架对比：AI Agent 框架概览

| 特性 | Semantic Kernel | LangChain | LlamaIndex | AutoGPT | OpenManus |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **核心理念** | 将AI能力封装成“技能”，通过编排器与原生代码结合 | 提供一套完整的“链”式工具，快速构建端到端应用 | 专注于“数据”，优化RAG（检索增强生成）流程 | 实现完全自主的AI，自动分解和执行任务 | 复刻和开源高级AI Agent能力，社区驱动 |
| **主要语言** | **C#, Python** | Python, JavaScript | Python | Python | Python, JavaScript |
| **生态系统** | **.NET, Microsoft Azure** | Python 社区 | Python 数据科学社区 | 开源社区 | 开源社区, MetaGPT |
| **优势** | **与C#/.NET生态无缝集成，企业级支持，规划器强大** | 社区庞大，组件丰富，上手快速，原型验证快 | 在数据索引、检索和RAG方面非常深入和强大 | 强大的自主任务规划和执行能力 | 模块化、可扩展，支持本地模型（Ollama） |
| **适用场景** | **需要将AI能力深度集成到现有.NET应用的企业级项目** | 快速构建和迭代各种基于LLM的Python应用 | 构建复杂的、以数据为核心的问答系统 | 探索通用人工智能（AGI）的自主代理实验 | 构建和研究可定制的、通用的AI代理 |

## 4. 终极代码示例：多Agent流式API (ASP.NET Core)

此示例旨在模拟一个真实的企业级场景：构建一个后端 API，该 API 接收用户查询，通过一个多 Agent 系统进行处理，并将结果以流式（SSE）方式返回给前端。

**目标**：实现一个 `QueryByStream` 的 API 接口，当用户输入 “厦门维保报告，详细介绍” 时，系统将：
1.  **翻译 Agent (OpenAI)**: 将查询翻译成目标语言（例如，英文）。
2.  **意图识别 Agent (QWen3)**: 识别出用户的核心意图是 `DocumentSearch`。
3.  **实体提取 Agent (Ollama)**: 从查询中提取出关键实体 `厦门维保报告`。
4.  **文档搜索 (HttpClient)**: 使用提取的实体调用外部文档API，并将结果流式返回。
5.  **聊天历史管理**: 在与模型交互前，对聊天历史进行压缩，并进行持久化。

---

### 4.1 项目设置 (Program.cs)

首先，我们需要在 `Program.cs` (或 Startup.cs) 中配置多个AI模型和HttpClient。

```csharp
// Program.cs in an ASP.NET Core Minimal API project
using Microsoft.SemanticKernel;
using System.Net.Http;

var builder = WebApplication.CreateBuilder(args);

// 1. 注册 HttpClientFactory 用于外部 API 调用
builder.Services.AddHttpClient();

// 2. 注册和配置多个 AI 服务
// 每个服务都有一个唯一的 key，用于在 Agent 中选择性调用
builder.Services.AddKeyedSingleton<IChatCompletionService>("OpenAI_Translation", new OpenAIChatCompletionService(
    modelId: "gpt-4o",
    apiKey: "sk-your-openai-api-key"
));
builder.Services.AddKeyedSingleton<IChatCompletionService>("QWen3_Intent", new OpenAIChatCompletionService(
    modelId: "qwen3-72b-chat", // 假设 QWen3 兼容 OpenAI API
    apiKey: "your-qwen3-api-key",
    endpoint: new Uri("https://api.qwen.com/v1")
));
builder.Services.AddKeyedSingleton<IChatCompletionService>("Ollama_Extraction", new OpenAIChatCompletionService(
    modelId: "llama3", // 假设 Ollama 中运行的是 Llama3
    apiKey: "ollama", // Ollama API key is often not needed
    endpoint: new Uri("http://localhost:11434/v1")
));

// 3. 注册我们的核心服务
builder.Services.AddSingleton<ChatHistoryService>();
builder.Services.AddSingleton<MultiAgentService>();

var app = builder.Build();

// 4. 定义 API 终结点
app.MapPost("/api/QueryByStream", async (
    QueryRequest request,
    MultiAgentService agentService,
    HttpContext context) =>
{
    context.Response.Headers.Append("Content-Type", "text/event-stream");
    context.Response.Headers.Append("Cache-Control", "no-cache");
    context.Response.Headers.Append("Connection", "keep-alive");

    await agentService.ProcessQueryStreamAsync(request.Query, request.History, context.Response.Body);
});

app.Run();

// 定义请求模型
public record QueryRequest(string Query, List<ChatMessage> History);
public record ChatMessage(string Role, string Content);
```

### 4.2 多Agent核心服务 (MultiAgentService.cs)

这个服务编排了所有的 Agent 和工具，执行核心逻辑。

```csharp
using Microsoft.SemanticKernel;
using System.Text;
using System.Text.Json;

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
        // 为每个Agent创建独立的Kernel实例
        _translationKernel = Kernel.CreateBuilder().AddChatCompletionService(translationService).Build();
        _intentKernel = Kernel.CreateBuilder().AddChatCompletionService(intentService).Build();
        _extractionKernel = Kernel.CreateBuilder().AddChatCompletionService(extractionService).Build();
        _httpClientFactory = httpClientFactory;
        _chatHistoryService = chatHistoryService;
    }

    public async Task ProcessQueryStreamAsync(string query, List<ChatMessage> history, Stream responseStream)
    {
        // 0. 聊天历史管理
        var compressedHistory = await _chatHistoryService.ReduceHistoryAsync(history);
        var fullQueryContext = $"History: {compressedHistory}\nUser Query: {query}";

        // 1. 翻译 Agent (OpenAI)
        var translationResult = await _translationKernel.InvokePromptAsync(
            $"Translate the user query into English: {query}");
        await WriteSseMessage(responseStream, "agent-log", $"Translation (OpenAI): {translationResult.GetValue<string>()}");

        // 2. 意图识别 Agent (QWen3)
        var intentFunction = _intentKernel.CreateFunctionFromPrompt(
            "Identify the user's intent from this list: [DocumentSearch, GeneralChat]. Query: {{$input}}",
            new PromptTemplateConfig { ResponseFormat = "json_object" });
        var intentResult = await _intentKernel.InvokeAsync(intentFunction, new() { { "input", fullQueryContext } });
        await WriteSseMessage(responseStream, "agent-log", $"Intent (QWen3): {intentResult.GetValue<string>()}");

        var intent = JsonDocument.Parse(intentResult.GetValue<string>()!).RootElement.GetProperty("intent").GetString();

        if (intent != "DocumentSearch")
        {
            await WriteSseMessage(responseStream, "final-result", "I can only help with document searches right now.");
            return;
        }

        // 3. 实体提取 Agent (Ollama)
        var extractionResult = await _extractionKernel.InvokePromptAsync(
            $"Extract the key document title from the query: {query}");
        var documentTitle = extractionResult.GetValue<string>()?.Trim();
        await WriteSseMessage(responseStream, "agent-log", $"Extraction (Ollama): {documentTitle}");

        // 4. 调用外部文档搜索API (HttpClient)
        await WriteSseMessage(responseStream, "agent-log", $"Starting document search for: {documentTitle}");
        var client = _httpClientFactory.CreateClient();
        var apiResponse = await client.GetAsync($"https://api.example.com/search?q={documentTitle}", HttpCompletionOption.ResponseHeadersRead);

        if (apiResponse.IsSuccessStatusCode)
        {
            using var apiStream = await apiResponse.Content.ReadAsStreamAsync();
            var buffer = new byte[1024];
            int bytesRead;
            while ((bytesRead = await apiStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                var chunk = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                await WriteSseMessage(responseStream, "document-chunk", chunk);
            }
        }
        else
        {
            await WriteSseMessage(responseStream, "error", "Failed to retrieve document from external API.");
        }
        
        // 5. 持久化聊天记录
        _chatHistoryService.SaveHistory(query, "Final document stream sent.");
        await WriteSseMessage(responseStream, "agent-log", "Process complete. History saved.");
    }

    private async Task WriteSseMessage(Stream stream, string eventName, string data)
    {
        var message = $"event: {eventName}\ndata: {data.Replace("\n", "\ndata: ")}\n\n";
        var bytes = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(bytes, 0, bytes.Length);
        await stream.FlushAsync();
        await Task.Delay(50); // 短暂延迟以确保消息被发送
    }
}
```

### 4.3 聊天记录管理 (ChatHistoryService.cs)

这个服务负责压缩和持久化聊天记录。

```csharp
using Microsoft.SemanticKernel;

public class ChatHistoryService
{
    private readonly Kernel _summarizationKernel;
    private readonly List<string> _database = new(); // 模拟数据库

    public ChatHistoryService([FromKeyedServices("OpenAI_Translation")] IChatCompletionService service)
    {
        // 可以复用一个模型来进行摘要
        _summarizationKernel = Kernel.CreateBuilder().AddChatCompletionService(service).Build();
    }

    // 策略：如果历史记录太长，就进行摘要
    public async Task<string> ReduceHistoryAsync(List<ChatMessage> history)
    {
        var historyText = string.Join("\n", history.Select(h => $"{h.Role}: {h.Content}"));
        if (historyText.Length < 500) // 阈值
        {
            return historyText;
        }

        // 调用LLM进行摘要
        var result = await _summarizationKernel.InvokePromptAsync(
            $"Summarize this conversation concisely: {historyText}");
        return result.GetValue<string>() ?? "";
    }

    // 持久化
    public void SaveHistory(string userQuery, string assistantResponse)
    {
        // 在真实应用中，这里会写入数据库
        _database.Add($"User: {userQuery}");
        _database.Add($"Assistant: {assistantResponse}");
        Console.WriteLine("--- History Saved ---");
        Console.WriteLine(string.Join("\n", _database));
        Console.WriteLine("---------------------");
    }
}
```

## 5. 如何运行和测试

1.  **启动项目**: 使用 `dotnet run` 启动 ASP.NET Core 应用。
2.  **使用 `curl` 测试 SSE**:
    ```bash
    curl -X POST -N \
      -H "Content-Type: application/json" \
      -d '{"query": "帮我找一下厦门的维保报告，要详细介绍", "history": []}' \
      http://localhost:5000/api/QueryByStream
    ```
3.  **观察输出**: 你将在终端看到一系列的 SSE 事件，实时展示 Agent 的工作流程、从外部 API 获取的文档流，以及最终的结果。

## 6. 学习资源

*   **官方文档**: [Semantic Kernel 文档 - Microsoft Learn](https://learn.microsoft.com/ja-jp/semantic-kernel/)
*   **核心仓库**: [microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel)
*   **入门项目**: [microsoft/semantic-kernel-starters](https://github.com/microsoft/semantic-kernel-starters)
*   **详细示例**: [Semantic Kernel Detailed Samples](https://learn.microsoft.com/ja-jp/semantic-kernel/get-started/detailed-samples)
*   **社区精选**: [awesome-semantickernel](https://github.com/geffzhang/awesome-semantickernel)
