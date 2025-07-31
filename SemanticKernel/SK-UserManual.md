# Semantic Kernel 用户手册

## 简介

Semantic Kernel (SK) 是一个开源的软件开发工具包（SDK），旨在将大语言模型（LLM）与传统的编程语言（如 C#）相结合。它允许开发人员轻松地创建、管理和编排 AI 功能，构建复杂的 AI 应用。

## 核心概念

### Kernel

Kernel 是 Semantic Kernel 的核心，它负责管理和执行各种功能。Kernel 包含了插件、记忆和连接器等组件。

### 插件（Plugins）

插件是包含一组相关功能的模块。一个插件可以包含多个函数，这些函数可以是：

*   **语义函数（Semantic Functions）**：用自然语言编写的提示（Prompts），用于与大语言模型进行交互。
*   **原生函数（Native Functions）**：用 C# 编写的传统代码，用于执行各种计算或与外部系统进行交互。

### 记忆

记忆是 Semantic Kernel 的核心功能之一，它允许 Kernel “记住”信息，并在后续的交互中利用这些信息。这对于构建能够进行连贯对话、拥有上下文感知能力的 AI 应用至关重要。

记忆的实现主要依赖于**嵌入（Embeddings）**和**向量数据库（Vector Databases）**。当信息被存入记忆时，Semantic Kernel 会首先通过嵌入模型（如 OpenAI 的 `text-embedding-ada-002`）将文本信息转换为向量形式，然后将这些向量存储在向量数据库中。当需要检索信息时，Kernel 会将查询文本同样转换为向量，并在数据库中进行相似度搜索，从而找到最相关的信息。

### 连接器

连接器是 Semantic Kernel 与外部世界沟通的桥梁。通过连接器，Kernel 可以访问各种大语言模型、服务和数据源。这极大地扩展了 Semantic Kernel 的能力，使其能够集成到复杂的应用场景中。

Semantic Kernel 提供了丰富的连接器，包括：

*   **模型连接器**：用于连接到各种大语言模型，如 OpenAI, Azure OpenAI, Hugging Face 等。
*   **记忆连接器**：用于连接到各种向量数据库，如 Azure AI Search, Qdrant, Chroma, SQLite 等，以实现记忆的持久化存储。
*   **外部服务连接器**：用于连接到各种外部服务，如 Microsoft Graph, Bing 搜索等。

### 函数链（Function Chaining）

函数链是 Semantic Kernel 的一个强大功能，它允许将多个函数（无论是语义函数还是原生函数）链接在一起，形成一个复杂的工作流。通过函数链，可以实现更复杂的逻辑和任务。输出的结果会作为下一个函数的输入。

#### C# 代码示例：链接两个语义函数

这个例子将创建两个语义函数：一个函数扮演“蝙蝠侠”，另一个函数扮演管家“阿尔弗雷德”，阿尔弗雷德会礼貌地总结蝙蝠侠的回答。

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Threading.Tasks;

public class FunctionChainingExample
{
    public static async Task RunAsync()
    {
        // 配置您的 AI 服务
        var builder = Kernel.CreateBuilder();
        builder.AddAzureOpenAIChatCompletion(
            "your-deployment-name",
            "your-endpoint",
            "your-api-key"
        );
        var kernel = builder.Build();

        // 定义“蝙蝠侠”语义函数
        string batTemplate = @"
        像蝙蝠侠一样回应用户的请求。
        要有创意和幽默感，但要保持内容干净。
        用户: {{$input}}
        AI: ";

        var batFunction = kernel.CreateFunctionFromPrompt(batTemplate,
            description: "以蝙蝠侠的身份回应请求。"
        );

        // 定义“阿尔弗雷德”语义函数
        string alfredTemplate = @"
        像布鲁斯·韦恩的管家阿尔弗雷德一样回应用户的请求。
        你的工作是总结蝙蝠侠的文字并转达给用户。
        要礼貌和乐于助人。
        蝙蝠侠的回应: {{$input}}
        总结: ";

        var alfredFunction = kernel.CreateFunctionFromPrompt(alfredTemplate,
            description: "像阿尔弗雷德一样礼貌地总结回应。"
        );

        // 获取用户输入
        Console.WriteLine("请输入您想对蝙蝠侠说的话:");
        string? message = Console.ReadLine();

        // 使用 InvokeAsync 将函数链接在一起
        if (message != null)
        {
            var result = await kernel.InvokeAsync(batFunction, new() { { "input", message } });
            var finalResult = await kernel.InvokeAsync(alfredFunction, new() { { "input", result.ToString() } });


            Console.WriteLine("\n--- 阿尔弗雷德的总结 ---");
            Console.WriteLine(finalResult.GetValue<string>());
        }
    }
}
```

### 完整示例：构建一个简单的问答机器人

现在，我们将结合前面介绍的概念，构建一个完整的问答机器人。这个机器人将能够：
1.  从提供的文本中学习知识（存入记忆）。
2.  理解用户的问题。
3.  在记忆中搜索答案。
4.  生成一个友好的回答。

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

public class QABot
{
    public static async Task Main(string[] args)
    {
        // 1. 配置 Kernel
        var builder = Kernel.CreateBuilder();
        builder.Services.AddAzureOpenAIChatCompletion("your-deployment-name", "https://your-endpoint.openai.azure.com/", "your-api-key");
        builder.Services.AddAzureOpenAITextEmbeddingGeneration("your-embedding-deployment-name", "https://your-endpoint.openai.azure.com/", "your-api-key");
        builder.Services.AddSingleton<IVolatileMemoryStore, VolatileMemoryStore>();
        var kernel = builder.Build();

        // 2. 将知识存入记忆
        var memory = kernel.Services.GetRequiredService<ISemanticTextMemory>();
        await memory.SaveInformationAsync("facts", id: "fact1", text: "Semantic Kernel 是一个能将大语言模型与传统编程语言结合的开源SDK。它由微软开发。主要编程语言是C#.\n总结：SK是一个微软开发的C#开源SDK，用于结合LLM和传统编程。", description: "关于Semantic Kernel的信息");
        await memory.SaveInformationAsync("facts", id: "fact2", text: "SK的核心组件包括Kernel、插件（Plugins）、记忆（Memory）和连接器（Connectors）。\n总结：SK的核心组件是Kernel、插件、记忆和连接器。", description: "关于Semantic Kernel核心组件的信息");

        // 3. 创建一个语义函数来回答问题
        string qaPrompt = @"
        根据以下事实回答问题：
        --- 开始 ---
        {{$facts}}
        --- 结束 ---
        问题: {{$input}}
        答案: ";

        var qaFunction = kernel.CreateFunctionFromPrompt(qaPrompt, description: "根据事实回答问题。");

        // 4. 提问并获取答案
        var question = "什么是Semantic Kernel？它有哪些核心组件？";
        Console.WriteLine($"问题: {question}");

        // 5. 从记忆中检索相关事实
        var searchResults = memory.SearchAsync("facts", question, limit: 2);
        var factStrings = new System.Text.StringBuilder();
        await foreach (var result in searchResults)
        {
            factStrings.AppendLine(result.Metadata.Text);
        }

        // 6. 调用函数生成最终答案
        var answerResult = await kernel.InvokeAsync(qaFunction, new() { { "input", question }, { "facts", factStrings.ToString() } });

        Console.WriteLine("\n答案:");
        Console.WriteLine(answerResult.GetValue<string>());
    }
}
```

这个示例展示了如何将 Semantic Kernel 的各个部分组合在一起，构建一个有用的 AI 应用。通过修改和扩展这个示例，您可以构建更复杂的、基于您自己数据的问答系统、内容生成器等。
