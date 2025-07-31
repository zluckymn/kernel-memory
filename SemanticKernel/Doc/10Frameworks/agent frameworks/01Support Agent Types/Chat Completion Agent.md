# Exploring the Semantic Kernel `ChatCompletionAgent` 探索Semantic Kernel `ChatCompletionAgent`

- 05/29/2025

Detailed API documentation related to this discussion is available at:
与此讨论相关的详细 API 文档可在以下位置获得：

- [`ChatCompletionAgent`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.agents.chatcompletionagent)
- [`Microsoft.SemanticKernel.Agents`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.agents)
- [`IChatCompletionService`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.chatcompletion.ichatcompletionservice)
- [`Microsoft.SemanticKernel.ChatCompletion`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.chatcompletion)



## Chat Completion in Semantic Kernel Semantic Kernel中的聊天完成

[Chat Completion](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/) is fundamentally a protocol for a chat-based interaction with an AI model where the chat-history is maintained and presented to the model with each request. Semantic Kernel [AI services](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/) offer a unified framework for integrating the chat-completion capabilities of various AI models.
[聊天完成](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/)从根本上说是一种与 AI 模型进行基于聊天的交互的协议，其中聊天记录被维护并通过每个请求呈现给模型。Semantic Kernel [AI 服务](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/)提供了一个统一的框架，用于集成各种 AI 模型的聊天完成功能。

A `ChatCompletionAgent` can leverage any of these [AI services](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/) to generate responses, whether directed to a user or another agent.
`ChatCompletionAgent` 可以利用这些 [AI 服务](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/)中的任何一个来生成响应，无论是针对用户还是其他代理。



## Preparing Your Development Environment 准备您的开发环境

To proceed with developing an `ChatCompletionAgent`, configure your development environment with the appropriate packages.
要继续开发 `ChatCompletionAgent`，请使用适当的包配置您的开发环境。

Add the `Microsoft.SemanticKernel.Agents.Core` package to your project:
将 `Microsoft.SemanticKernel.Agents.Core` 包添加到项目中：

pwsh  普什Copy  复制

```pwsh
dotnet add package Microsoft.SemanticKernel.Agents.Core --prerelease
```



## Creating a `ChatCompletionAgent` 创建 `ChatCompletionAgent`

A `ChatCompletionAgent` is fundamentally based on an [AI services](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/). As such, creating a `ChatCompletionAgent` starts with creating a [`Kernel`](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel) instance that contains one or more chat-completion services and then instantiating the agent with a reference to that [`Kernel`](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel) instance.
`ChatCompletionAgent` 从根本上讲是基于 [AI 服务 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/)。因此，创建 `ChatCompletionAgent` 首先要创建一个包含一个或多个聊天完成服务的[`内核`](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel)实例，然后使用对该[`内核`](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel)实例的引用实例化代理。

C#Copy  复制

```csharp
// Initialize a Kernel with a chat-completion service
IKernelBuilder builder = Kernel.CreateBuilder();

builder.AddAzureOpenAIChatCompletion(/*<...configuration parameters>*/);

Kernel kernel = builder.Build();

// Create the agent
ChatCompletionAgent agent =
    new()
    {
        Name = "SummarizationAgent",
        Instructions = "Summarize user input",
        Kernel = kernel
    };
```



## AI Service Selection  AI 服务选择

No different from using Semantic Kernel [AI services](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/) directly, a `ChatCompletionAgent` supports the specification of a service-selector. A service-selector identifies which [AI service](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/) to target when the [`Kernel`](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel) contains more than one.
与直接使用Semantic Kernel [AI 服务](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/)没有什么不同，`ChatCompletionAgent` 支持服务选择器的规范。当[`内核`](https://learn.microsoft.com/en-us/semantic-kernel/concepts/kernel)包含多个服务时，服务选择器会标识要定位的 [AI 服务 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/)。

 Note  注意

If multiple [AI services](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/) are present and no service-selector is provided, the same default logic is applied for the agent that you'd find when using an [AI services](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/) outside of the `Agent Framework`
如果存在多个 [AI 服务 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/)，并且未提供服务选择器，则在代理`框架`之外使用 [AI 服务](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/)时，将对代理应用相同的默认逻辑

C#Copy  复制

```csharp
IKernelBuilder builder = Kernel.CreateBuilder();

// Initialize multiple chat-completion services.
builder.AddAzureOpenAIChatCompletion(/*<...service configuration>*/, serviceId: "service-1");
builder.AddAzureOpenAIChatCompletion(/*<...service configuration>*/, serviceId: "service-2");

Kernel kernel = builder.Build();

ChatCompletionAgent agent =
    new()
    {
        Name = "<agent name>",
        Instructions = "<agent instructions>",
        Kernel = kernel,
        Arguments = // Specify the service-identifier via the KernelArguments
          new KernelArguments(
            new OpenAIPromptExecutionSettings() 
            { 
              ServiceId = "service-2" // The target service-identifier.
            })
    };
```



## Conversing with `ChatCompletionAgent` 与 `ChatCompletionAgent` 对话

Conversing with your `ChatCompletionAgent` is based on a `ChatHistory` instance, no different from interacting with a Chat Completion [AI service](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/).
与 `ChatCompletionAgent` 对话基于 `ChatHistory` 实例，与与聊天完成 [AI 服务](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/)交互没有什么不同。

You can simply invoke the agent with your user message.
只需使用用户消息调用代理即可。

C#Copy  复制

```csharp
// Define agent
ChatCompletionAgent agent = ...;

// Generate the agent response(s)
await foreach (ChatMessageContent response in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, "<user input>")))
{
  // Process agent response(s)...
}
```

You can also use an `AgentThread` to have a conversation with your agent. Here we are using a `ChatHistoryAgentThread`.
您还可以使用 `AgentThread` 与您的代理进行对话。这里我们使用 `ChatHistoryAgentThread`。

The `ChatHistoryAgentThread` can also take an optional `ChatHistory` object as input, via its constructor, if resuming a previous conversation. (not shown)
`ChatHistoryAgentThread` 还可以采用可选的 `ChatHistory` 对象作为输入，通过其构造函数，如果恢复之前的对话。（未显示）

C#Copy  复制

```csharp
// Define agent
ChatCompletionAgent agent = ...;

AgentThread thread = new ChatHistoryAgentThread();

// Generate the agent response(s)
await foreach (ChatMessageContent response in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, "<user input>"), thread))
{
  // Process agent response(s)...
}
```



## Handling Intermediate Messages with a `ChatCompletionAgent` 使用 `ChatCompletionAgent` 处理中间消息

The Semantic Kernel `ChatCompletionAgent` is designed to invoke an agent that fulfills user queries or questions. During invocation, the agent may execute tools to derive the final answer. To access intermediate messages produced during this process, callers can supply a callback function that handles instances of `FunctionCallContent` or `FunctionResultContent`.
Semantic Kernel `ChatCompletionAgent` 旨在调用满足用户查询或问题的代理。在调用期间，代理可以执行工具来得出最终答案。若要访问在此过程中生成的中间消息，调用方可以提供处理 `FunctionCallContent` 或 `FunctionResultContent` 实例的回调函数。

> Callback documentation for the `ChatCompletionAgent` is coming soon.
> `ChatCompletionAgent` 的回调文档即将推出。



## Declarative Spec  声明式规范

> The documentation on using declarative specs is coming soon.
> 有关使用声明式规范的文档即将推出。



## How-To  作方法

For an end-to-end example for a `ChatCompletionAgent`, see:
有关 `ChatCompletionAgent` 的端到端示例，请参阅：

- [How-To: `ChatCompletionAgent`
  作方法：`ChatCompletionAgent`](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/examples/example-chat-agent)



## Next Steps  后续步骤

[  探索 Copilot Studio 代理](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/copilot-studio-agent)