# Semantic Kernel 入门手册

## 1. 简介

Semantic Kernel (SK) 是一个由微软开源的软件开发工具包（SDK），它旨在将大语言模型（LLM）的强大能力与传统编程语言（如 C#）无缝集成。通过 SK，开发者可以轻松地创建、管理和编排 AI 功能，构建能够调用现有代码的复杂 AI 应用。

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

## 3. 框架对比：Semantic Kernel vs. LangChain vs. LlamaIndex

| 特性 | Semantic Kernel | LangChain | LlamaIndex |
| :--- | :--- | :--- | :--- |
| **核心理念** | 将AI能力封装成“技能”，通过编排器与原生代码结合 | 提供一套完整的“链”式工具，快速构建端到端应用 | 专注于“数据”，优化RAG（检索增强生成）流程 |
| **主要语言** | C#, Python | Python, JavaScript | Python |
| **生态系统** | .NET, Microsoft Azure | Python 社区 | Python 数据科学社区 |
| **优势** | 与C#/.NET生态无缝集成，企业级支持，规划器能力强大 | 社区庞大，组件丰富，上手快速，原型验证快 | 在数据索引、检索和RAG方面非常深入和强大 |
| **适用场景** | 需要将AI能力深度集成到现有.NET应用的企业级项目 | 快速构建和迭代各种基于LLM的Python应用 | 构建复杂的、以数据为核心的问答系统和知识库 |

## 4. 主要代码示例

### 4.1 意图识别与函数调用

这个例子演示了如何识别用户的意图，并根据意图调用不同的函数。我们将设定，当用户意图为`DocumentSearch`时，调用一个特定的模型（模拟为QWen3）。

```csharp
using Microsoft.SemanticKernel;
using System.ComponentModel;

// 模拟的外部服务调用
public class ExternalServices
{
    [KernelFunction, Description("当用户需要搜索文档时调用此函数")]
    public static string DocumentSearch(string query)
    {
        Console.WriteLine($"[调用 QWen3 模型] 正在搜索文档: {query}");
        return $"找到了关于 '{query}' 的3份维保报告。";
    }

    [KernelFunction, Description("当用户需要进行普通聊天时调用此函数")]
    public static string GeneralChat(string query)
    {
        Console.WriteLine("[调用通用模型] 正在进行通用聊天...");
        return "您好！有什么可以帮助您的吗？";
    }
}

// ... Kernel 初始化 ...

// 加载包含原生函数的插件
kernel.Plugins.AddFromType<ExternalServices>();

// 创建一个自动调用函数的规划器
var planner = new HandlebarsPlanner(new HandlebarsPlannerOptions { AllowLoops = true });

// 用户输入
var userInput = "帮我找一下厦门的维保报告";

// 让规划器根据用户输入自动选择并执行函数
var plan = await planner.CreatePlanAsync(kernel, userInput);
var result = await plan.InvokeAsync(kernel);

Console.WriteLine($"\n最终结果: {result}");
```

## 5. 完整示例：多Agent协作系统

请参考上一版本文档中关于多Agent系统的详细代码，该示例展示了如何为不同Agent配置不同模型，并实现任务路由。

## 6. 学习资源

*   **官方文档**: [Semantic Kernel 文档 - Microsoft Learn](https://learn.microsoft.com/ja-jp/semantic-kernel/)
*   **核心仓库**: [microsoft/semantic-kernel](https://github.com/microsoft/semantic-kernel)
*   **入门项目**: [microsoft/semantic-kernel-starters](https://github.com/microsoft/semantic-kernel-starters)
*   **详细示例**: [Semantic Kernel Detailed Samples](https://learn.microsoft.com/ja-jp/semantic-kernel/get-started/detailed-samples)
*   **社区精选**: [awesome-semantickernel](https://github.com/geffzhang/awesome-semantickernel)

```