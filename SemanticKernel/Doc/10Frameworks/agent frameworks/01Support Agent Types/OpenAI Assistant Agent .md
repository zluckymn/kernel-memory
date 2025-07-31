# Exploring the Semantic Kernel `OpenAIAssistantAgent` 探索Semantic Kernel `OpenAIAssistantAgent`

- 05/29/2025



Single-agent features, such as `OpenAIAssistantAgent`, are in the release candidate stage. These features are nearly complete and generally stable, though they may undergo minor refinements or optimizations before reaching full general availability.
单代理功能（例如 `OpenAIAssistantAgent`）处于候选版本阶段。这些功能几乎完整且总体稳定，尽管它们在完全正式发布之前可能会进行细微的改进或优化。

 Tip  提示

Detailed API documentation related to this discussion is available at:
与此讨论相关的详细 API 文档可在以下位置获得：

- [`OpenAIAssistantAgent`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.agents.openai.openaiassistantagent)



## What is an Assistant?  什么是助理？

The OpenAI Assistants API is a specialized interface designed for more advanced and interactive AI capabilities, enabling developers to create personalized and multi-step task-oriented agents. Unlike the Chat Completion API, which focuses on simple conversational exchanges, the Assistant API allows for dynamic, goal-driven interactions with additional features like code-interpreter and file-search.
OpenAI Assistants API 是一个专门的界面，专为更高级和交互式的 AI 功能而设计，使开发人员能够创建个性化和面向多步骤任务的代理。与专注于简单对话交流的聊天完成 API 不同，Assistant API 允许通过代码解释器和文件搜索等附加功能进行动态、目标驱动的交互。

- [OpenAI Assistant Guide  OpenAI 助手指南](https://platform.openai.com/docs/assistants)
- [OpenAI Assistant API  OpenAI 助手 API](https://platform.openai.com/docs/api-reference/assistants)
- [Assistant API in Azure
  Azure 中的助手 API](https://learn.microsoft.com/en-us/azure/ai-services/openai/assistants-quickstart)



## Preparing Your Development Environment 准备您的开发环境

To proceed with developing an `OpenAIAssistantAgent`, configure your development environment with the appropriate packages.
要继续开发 `OpenAIAssistantAgent`，请使用适当的包配置您的开发环境。

Add the `Microsoft.SemanticKernel.Agents.OpenAI` package to your project:
将 `Microsoft.SemanticKernel.Agents.OpenAI` 包添加到项目中：

pwsh  普什Copy  复制

```pwsh
dotnet add package Microsoft.SemanticKernel.Agents.OpenAI --prerelease
```

You may also want to include the `Azure.Identity` package:
可能还需要包含 `Azure.Identity` 包：

pwsh  普什Copy  复制

```pwsh
dotnet add package Azure.Identity
```



## Creating an `OpenAIAssistantAgent` 创建 `OpenAIAssistantAgent`

Creating an `OpenAIAssistant` requires first creating a client to be able to talk a remote service.
创建 `OpenAIAssistant` 需要首先创建一个客户端才能与远程服务通话。

C#Copy  复制

```csharp
AssistantClient client = OpenAIAssistantAgent.CreateAzureOpenAIClient(...).GetAssistantClient();
Assistant assistant =
    await client.CreateAssistantAsync(
        "<model name>",
        "<agent name>",
        instructions: "<agent instructions>");
OpenAIAssistantAgent agent = new(assistant, client);
```



## Retrieving an `OpenAIAssistantAgent` 检索 `OpenAIAssistantAgent`

Once created, the identifier of the assistant may be access via its identifier. This identifier may be used to create an `OpenAIAssistantAgent` from an existing assistant definition.
创建后，可以通过其标识符访问助手的标识符。此标识符可用于从现有助手定义创建 `OpenAIAssistantAgent`。

For .NET, the agent identifier is exposed as a `string` via the property defined by any agent.
对于 .NET，代理标识符通过任何代理定义的属性公开为`字符串 `。

C#Copy  复制

```csharp
AssistantClient client = OpenAIAssistantAgent.CreateAzureOpenAIClient(...).GetAssistantClient();
Assistant assistant = await client.GetAssistantAsync("<assistant id>");
OpenAIAssistantAgent agent = new(assistant, client);
```



## Using an `OpenAIAssistantAgent` 使用 `OpenAIAssistantAgent`

As with all aspects of the Assistant API, conversations are stored remotely. Each conversation is referred to as a thread and identified by a unique `string` identifier. Interactions with your `OpenAIAssistantAgent` are tied to this specific thread identifier. The specifics of the Assistant API thread is abstracted away via the `OpenAIAssistantAgentThread` class, which is an implementation of `AgentThread`.
与 Assistant API 的所有方面一样，对话是远程存储的。每个对话称为线程，并由唯一的`字符串`标识符标识。与 `OpenAIAssistantAgent` 的交互与此特定线程标识符相关联。Assistant API 线程的细节通过 `OpenAIAssistantAgentThread` 类抽象出来，该类是 `AgentThread` 的实现。

The `OpenAIAssistantAgent` currently only supports threads of type `OpenAIAssistantAgentThread`.
`OpenAIAssistantAgent` 目前仅支持 `OpenAIAssistantAgentThread` 类型的线程。

You can invoke the `OpenAIAssistantAgent` without specifying an `AgentThread`, to start a new thread and a new `AgentThread` will be returned as part of the response.
您可以在不指定 `AgentThread` 的情况下调用 `OpenAIAssistantAgent` 来启动新线程，并且将返回新的 `AgentThread` 作为响应的一部分。

C#Copy  复制

```csharp
// Define agent
OpenAIAssistantAgent agent = ...;
AgentThread? agentThread = null;

// Generate the agent response(s)
await foreach (AgentResponseItem<ChatMessageContent> response in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, "<user input>")))
{
  // Process agent response(s)...
  agentThread = response.Thread;
}

// Delete the thread if no longer needed
if (agentThread is not null)
{
    await agentThread.DeleteAsync();
}
```

You can also invoke the `OpenAIAssistantAgent` with an `AgentThread` that you created.
您还可以使用您创建的 `AgentThread` 调用 `OpenAIAssistantAgent`。

C#Copy  复制

```csharp
// Define agent
OpenAIAssistantAgent agent = ...;

// Create a thread with some custom metadata.
AgentThread agentThread = new OpenAIAssistantAgentThread(client, metadata: myMetadata);

// Generate the agent response(s)
await foreach (ChatMessageContent response in agent.InvokeAsync(new ChatMessageContent(AuthorRole.User, "<user input>"), agentThread))
{
  // Process agent response(s)...
}

// Delete the thread when it is no longer needed
await agentThread.DeleteAsync();
```

You can also create an `OpenAIAssistantAgentThread` that resumes an earlier conversation by id.
您还可以创建一个 `OpenAIAssistantAgentThread`，该线程通过 id 恢复先前的对话。

C#Copy  复制

```csharp
// Create a thread with an existing thread id.
AgentThread agentThread = new OpenAIAssistantAgentThread(client, "existing-thread-id");
```



## Deleting an `OpenAIAssistantAgent` 删除 `OpenAIAssistantAgent`

Since the assistant's definition is stored remotely, it will persist if not deleted.
由于助手的定义是远程存储的，因此如果不删除，它将保留。
Deleting an assistant definition may be performed directly with the client.
删除助手定义可以直接与客户端一起执行。

> Note: Attempting to use an agent instance after being deleted will result in a service exception.
> 注意：删除代理实例后尝试使用将导致服务异常。

For .NET, the agent identifier is exposed as a `string` via the [`Agent.Id`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.agents.agent.id) property defined by any agent.
对于 .NET，代理标识符通过任何代理定义的 [`Agent.Id`](https://learn.microsoft.com/en-us/dotnet/api/microsoft.semantickernel.agents.agent.id) 属性公开为`字符串 `。

C#Copy  复制

```csharp
AssistantClient client = OpenAIAssistantAgent.CreateAzureOpenAIClient(...).GetAssistantClient();
await client.DeleteAssistantAsync("<assistant id>");
```



## Handling Intermediate Messages with an `OpenAIAssistantAgent` 使用 `OpenAIAssistantAgent` 处理中间消息

The Semantic Kernel `OpenAIAssistantAgent` is designed to invoke an agent that fulfills user queries or questions. During invocation, the agent may execute tools to derive the final answer. To access intermediate messages produced during this process, callers can supply a callback function that handles instances of `FunctionCallContent` or `FunctionResultContent`.
Semantic Kernel `OpenAIAssistantAgent` 旨在调用满足用户查询或问题的代理。在调用期间，代理可以执行工具来得出最终答案。若要访问在此过程中生成的中间消息，调用方可以提供处理 `FunctionCallContent` 或 `FunctionResultContent` 实例的回调函数。

> Callback documentation for the `OpenAIAssistantAgent` is coming soon.
> `OpenAIAssistantAgent` 的回调文档即将推出。



## Declarative Spec  声明式规范

> The documentation on using declarative specs is coming soon.
> 有关使用声明式规范的文档即将推出。



## How-To  作方法

For an end-to-end example for a `OpenAIAssistantAgent`, see:
有关 `OpenAIAssistantAgent` 的端到端示例，请参阅：

- [How-To: `OpenAIAssistantAgent` Code Interpreter
  作方法：`OpenAIAssistantAgent` 代码解释器](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/examples/example-assistant-code)
- [How-To: `OpenAIAssistantAgent` File Search
  作方法：`OpenAIAssistantAgent` 文件搜索](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/examples/example-assistant-search)



## Next Steps  后续步骤

[  探索 OpenAI 响应代理](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/responses-agent)