# Exploring the Semantic Kernel `AzureAIAgent` 探索Semantic Kernel `AzureAIAgent`

- 05/29/2025



This feature is in the experimental stage. Features at this stage are under development and subject to change before advancing to the preview or release candidate stage.
此功能处于实验阶段。此阶段的功能正在开发中，在进入预览版或候选版本阶段之前可能会发生变化。

 Tip  提示

Detailed API documentation related to this discussion is available at:
与此讨论相关的详细 API 文档可在以下位置获得：

- [`AzureAIAgent`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.agents.azureai)



## What is an `AzureAIAgent`? 什么是 `AzureAIAgent`？

An `AzureAIAgent` is a specialized agent within the Semantic Kernel framework, designed to provide advanced conversational capabilities with seamless tool integration. It automates tool calling, eliminating the need for manual parsing and invocation. The agent also securely manages conversation history using threads, reducing the overhead of maintaining state. Additionally, the `AzureAIAgent` supports a variety of built-in tools, including file retrieval, code execution, and data interaction via Bing, Azure AI Search, Azure Functions, and OpenAPI.
`AzureAIAgent` 是Semantic Kernel框架中的专用代理，旨在通过无缝工具集成提供高级对话功能。它自动执行工具调用，无需手动解析和调用。代理还使用线程安全地管理对话历史记录，从而减少维护状态的开销。此外，`AzureAIAgent` 支持各种内置工具，包括文件检索、代码执行以及通过 Bing、Azure AI 搜索、Azure Functions 和 OpenAPI 进行数据交互。

To use an `AzureAIAgent`, an Azure AI Foundry Project must be utilized. The following articles provide an overview of the Azure AI Foundry, how to create and configure a project, and the agent service:
若要使用 `AzureAIAgent`，必须使用 Azure AI Foundry 项目。以下文章概述了 Azure AI Foundry、如何创建和配置项目以及代理服务：

- [What is Azure AI Foundry?
  什么是 Azure AI Foundry？](https://learn.microsoft.com/en-us/azure/ai-foundry/what-is-ai-foundry)
- [The Azure AI Foundry SDK](https://learn.microsoft.com/en-us/azure/ai-foundry/how-to/develop/sdk-overview)
- [What is Azure AI Agent Service
  什么是 Azure AI 代理服务](https://learn.microsoft.com/en-us/azure/ai-services/agents/overview)
- [Quickstart: Create a new agent
  快速入门：创建新代理](https://learn.microsoft.com/en-us/azure/ai-services/agents/quickstart)



## Preparing Your Development Environment 准备您的开发环境

To proceed with developing an `AzureAIAgent`, configure your development environment with the appropriate packages.
若要继续开发 `AzureAIAgent`，请使用适当的包配置开发环境。

Add the `Microsoft.SemanticKernel.Agents.AzureAI` package to your project:
将 `Microsoft.SemanticKernel.Agents.AzureAI` 包添加到项目中：

pwsh  普什Copy  复制

```pwsh
dotnet add package Microsoft.SemanticKernel.Agents.AzureAI --prerelease
```

You may also want to include the `Azure.Identity` package:
可能还需要包含 `Azure.Identity` 包：

pwsh  普什Copy  复制

```pwsh
dotnet add package Azure.Identity
```



## Configuring the AI Project Client 配置 AI 项目客户端

Accessing an `AzureAIAgent` first requires the creation of a client that is configured for a specific Foundry Project, most commonly by providing your project endpoint ([The Azure AI Foundry SDK: Getting Started with Projects](https://learn.microsoft.com/en-us/azure/ai-foundry/how-to/develop/sdk-overview#get-started-with-projects)).
访问 `AzureAIAgent` 首先需要创建为特定 Foundry 项目配置的客户端，最常见的方法是提供项目终结点（[Azure AI Foundry SDK：项目入门 ](https://learn.microsoft.com/en-us/azure/ai-foundry/how-to/develop/sdk-overview#get-started-with-projects)）。

C#Copy  复制

```csharp
PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("<your endpoint>", new AzureCliCredential());
```



## Creating an `AzureAIAgent` 创建 `AzureAIAgent`

To create an `AzureAIAgent`, you start by configuring and initializing the Foundry project through the Azure Agent service and then integrate it with Semantic Kernel:
若要创建 `AzureAIAgent`，首先通过 Azure 代理服务配置和初始化 Foundry 项目，然后将其与Semantic Kernel集成：

C#Copy  复制

```csharp
PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("<your endpoint>", new AzureCliCredential());

// 1. Define an agent on the Azure AI agent service
PersistentAgent definition = await agentsClient.Administration.CreateAgentAsync(
    "<name of the the model used by the agent>",
    name: "<agent name>",
    description: "<agent description>",
    instructions: "<agent instructions>");

// 2. Create a Semantic Kernel agent based on the agent definition
AzureAIAgent agent = new(definition, agentsClient);
```



## Interacting with an `AzureAIAgent` 与 `AzureAIAgent` 交互

Interaction with the `AzureAIAgent` is straightforward. The agent maintains the conversation history automatically using a thread.
与 `AzureAIAgent` 的交互非常简单。代理使用线程自动维护对话历史记录。

The specifics of the *Azure AI Agent thread* is abstracted away via the `Microsoft.SemanticKernel.Agents.AzureAI.AzureAIAgentThread` class, which is an implementation of `Microsoft.SemanticKernel.Agents.AgentThread`.
*Azure AI 代理线程*的细节通过类 `Microsoft.SemanticKernel.Agents.AzureAI.AzureAIAgentThread` 抽象出来，该类是 `Microsoft.SemanticKernel.Agents.AgentThread` 的实现。

 Important  重要

Note that the Azure AI Agents SDK has the `PersistentAgentThread` class. It should not be confused with `Microsoft.SemanticKernel.Agents.AgentThread`, which is the common Semantic Kernel Agents abstraction for all thread types.
请注意，Azure AI 代理 SDK 具有 `PersistentAgentThread` 类。它不应与 `Microsoft.SemanticKernel.Agents.AgentThread` 混淆，后者是所有线程类型的常见Semantic Kernel代理抽象。

The `AzureAIAgent` currently only supports threads of type `AzureAIAgentThread`.
`AzureAIAgent` 目前仅支持 `AzureAIAgentThread` 类型的线程。

C#Copy  复制

```csharp
AzureAIAgentThread agentThread = new(agent.Client);
try
{
    ChatMessageContent message = new(AuthorRole.User, "<your user input>");
    await foreach (ChatMessageContent response in agent.InvokeAsync(message, agentThread))
    {
        Console.WriteLine(response.Content);
    }
}
finally
{
    await agentThread.DeleteAsync();
    await agent.Client.DeleteAgentAsync(agent.Id);
}
```

An agent may also produce a streamed response:
代理还可以生成流式响应：

C#Copy  复制

```csharp
ChatMessageContent message = new(AuthorRole.User, "<your user input>");
await foreach (StreamingChatMessageContent response in agent.InvokeStreamingAsync(message, agentThread))
{
    Console.Write(response.Content);
}
```



## Using Plugins with an `AzureAIAgent` 将插件与 `AzureAIAgent` 配合使用

Semantic Kernel supports extending an `AzureAIAgent` with custom plugins for enhanced functionality:
Semantic Kernel支持使用自定义插件扩展 `AzureAIAgent` 以增强功能：

C#Copy  复制

```csharp
KernelPlugin plugin = KernelPluginFactory.CreateFromType<YourPlugin>();
PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("<your endpoint>", new AzureCliCredential());

PersistentAgent definition = await agentsClient.Administration.CreateAgentAsync(
    "<name of the the model used by the agent>",
    name: "<agent name>",
    description: "<agent description>",
    instructions: "<agent instructions>");

AzureAIAgent agent = new(definition, agentsClient, plugins: [plugin]);
```



## Advanced Features  高级功能

An `AzureAIAgent` can leverage advanced tools such as:
`AzureAIAgent` 可以利用高级工具，例如：

- [Code Interpreter  代码解释器](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/azure-ai-agent?pivots=programming-language-csharp#code-interpreter)
- [File Search  文件搜索](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/azure-ai-agent?pivots=programming-language-csharp#file-search)
- [OpenAPI integration  OpenAPI 集成](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/azure-ai-agent?pivots=programming-language-csharp#openapi-integration)
- [Azure AI Search integration
  Azure AI 搜索集成](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/azure-ai-agent?pivots=programming-language-csharp#azureai-search-integration)
- [Bing Grounding  必应接地](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/azure-ai-agent?pivots=programming-language-csharp#bing-grounding)



### Code Interpreter  代码解释器

Code Interpreter allows the agents to write and run Python code in a sandboxed execution environment ([Azure AI Agent Service Code Interpreter](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/code-interpreter)).
代码解释器允许代理在沙盒执行环境（[Azure AI 代理服务代码解释器 ](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/code-interpreter)）中编写和运行 Python 代码。

C#Copy  复制

```csharp
PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("<your endpoint>", new AzureCliCredential());

PersistentAgent definition = await agentsClient.CreateAgentAsync(
    "<name of the the model used by the agent>",
    name: "<agent name>",
    description: "<agent description>",
    instructions: "<agent instructions>",
    tools: [new CodeInterpreterToolDefinition()],
    toolResources:
        new()
        {
            CodeInterpreter = new()
            {
                FileIds = { ... },
            }
        }));

AzureAIAgent agent = new(definition, agentsClient);
```



### File Search  文件搜索

File search augments agents with knowledge from outside its model ([Azure AI Agent Service File Search Tool](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/file-search)).
文件搜索使用其模型外部的知识（[Azure AI 代理服务文件搜索工具 ](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/file-search)）增强代理。

C#Copy  复制

```csharp
PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("<your endpoint>", new AzureCliCredential());

PersistentAgent definition = await agentsClient.CreateAgentAsync(
    "<name of the the model used by the agent>",
    name: "<agent name>",
    description: "<agent description>",
    instructions: "<agent instructions>",
    tools: [new FileSearchToolDefinition()],
    toolResources:
        new()
        {
            FileSearch = new()
            {
                VectorStoreIds = { ... },
            }
        });

AzureAIAgent agent = new(definition, agentsClient);
```



### OpenAPI Integration  OpenAPI 集成

Connects your agent to an external API ([How to use Azure AI Agent Service with OpenAPI Specified Tools](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/openapi-spec)).
将代理连接到外部 API（[ 如何将 Azure AI 代理服务与 OpenAPI 指定工具配合使用 ](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/openapi-spec)）。

C#Copy  复制

```csharp
PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("<your endpoint>", new AzureCliCredential());

string apiJsonSpecification = ...; // An Open API JSON specification

PersistentAgent definition = await agentsClient.CreateAgentAsync(
    "<name of the the model used by the agent>",
    name: "<agent name>",
    description: "<agent description>",
    instructions: "<agent instructions>",
    tools: [
        new OpenApiToolDefinition(
            "<api name>", 
            "<api description>", 
            BinaryData.FromString(apiJsonSpecification), 
            new OpenApiAnonymousAuthDetails())
    ]
);

AzureAIAgent agent = new(definition, agentsClient);
```



### AzureAI Search Integration AzureAI 搜索集成

Use an existing Azure AI Search index with with your agent ([Use an existing AI Search index](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/azure-ai-search)).
将现有的 Azure AI 搜索索引与代理一起使用（[ 使用现有的 AI 搜索索引 ](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/azure-ai-search)）。

C#Copy  复制

```csharp
PersistentAgentsClient client = AzureAIAgent.CreateAgentsClient("<your endpoint>", new AzureCliCredential());

PersistentAgent definition = await agentsClient.CreateAgentAsync(
    "<name of the the model used by the agent>",
    name: "<agent name>",
    description: "<agent description>",
    instructions: "<agent instructions>",
    tools: [new AzureAISearchToolDefinition()],
    toolResources: new()
    {
        AzureAISearch = new()
        {
            IndexList = { new AISearchIndexResource("<your connection id>", "<your index name>") }
        }
    });

AzureAIAgent agent = new(definition, agentsClient);
```



### Bing Grounding  必应接地

> Example coming soon.  示例即将推出。



### Retrieving an Existing `AzureAIAgent` 检索现有 `AzureAIAgent`

An existing agent can be retrieved and reused by specifying its assistant ID:
可以通过指定现有代理的助手 ID 来检索和重复使用现有代理：

C#Copy  复制

```csharp
PersistentAgent definition = await agentsClient.Administration.GetAgentAsync("<your agent id>");
AzureAIAgent agent = new(definition, agentsClient);
```



## Deleting an `AzureAIAgent` 删除 `AzureAIAgent`

Agents and their associated threads can be deleted when no longer needed:
当不再需要时，可以删除代理及其关联的线程：

C#Copy  复制

```csharp
await agentThread.DeleteAsync();
await agentsClient.Administration.DeleteAgentAsync(agent.Id);
```

If working with a vector store or files, they may be deleted as well:
如果使用矢量存储或文件，它们也可能被删除：

C#Copy  复制

```csharp
await agentsClient.VectorStores.DeleteVectorStoreAsync("<your store id>");
await agentsClient.Files.DeleteFileAsync("<your file id>");
```

> More information on the *file search* tool is described in the [Azure AI Agent Service file search tool](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/file-search) article.
> 有关*文件搜索*工具的详细信息，请参阅 [Azure AI 代理服务文件搜索工具](https://learn.microsoft.com/en-us/azure/ai-services/agents/how-to/tools/file-search)一文。



## How-To  作方法

For practical examples of using an `AzureAIAgent`, see our code samples on GitHub:
有关使用 `AzureAIAgent` 的实际示例，请参阅 GitHub 上的代码示例：

- [Getting Started with Azure AI Agents
  Azure AI 代理入门](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/GettingStartedWithAgents/AzureAIAgent)
- [Advanced Azure AI Agent Code Samples
  高级 Azure AI 代理代码示例](https://github.com/microsoft/semantic-kernel/tree/main/dotnet/samples/Concepts/Agents)



## Handling Intermediate Messages with an `AzureAIAgent` 使用 `AzureAIAgent` 处理中间消息

The Semantic Kernel `AzureAIAgent` is designed to invoke an agent that fulfills user queries or questions. During invocation, the agent may execute tools to derive the final answer. To access intermediate messages produced during this process, callers can supply a callback function that handles instances of `FunctionCallContent` or `FunctionResultContent`.
Semantic Kernel `AzureAIAgent` 旨在调用满足用户查询或问题的代理。在调用期间，代理可以执行工具来得出最终答案。若要访问在此过程中生成的中间消息，调用方可以提供处理 `FunctionCallContent` 或 `FunctionResultContent` 实例的回调函数。

> Callback documentation for the `AzureAIAgent` is coming soon.
> `AzureAIAgent` 的回调文档即将推出。



## Declarative Spec  声明式规范

> The documentation on using declarative specs is coming soon.
> 有关使用声明式规范的文档即将推出。



## Next Steps  后续步骤

[  探索基岩特工](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/bedrock-agent)