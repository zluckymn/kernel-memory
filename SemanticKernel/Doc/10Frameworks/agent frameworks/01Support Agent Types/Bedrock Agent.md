# Exploring the Semantic Kernel `BedrockAgent` 探索Semantic Kernel `BedrockAgent`

- 05/29/2025



Single-agent features, such as `BedrockAgent`, are currently in the experimental stage. These features are under active development and may change before reaching general availability.
单代理功能，例如 `BedrockAgent`，目前处于实验阶段。这些功能正在积极开发中，可能会在正式发布之前发生变化。

Detailed API documentation related to this discussion is available at:
与此讨论相关的详细 API 文档可在以下位置获得：

**BedrockAgent API documentation coming soon.BedrockAgent API 文档即将推出。**



## What is a `BedrockAgent`? 什么是`基岩代理 `？

The Bedrock Agent is a specialized AI agent within Semantic Kernel designed to integrate with Amazon Bedrock’s Agent service. Like the OpenAI and Azure AI agents, a Bedrock Agent enables advanced multi-turn conversational capabilities with seamless tool (action) integration, but it operates entirely in the AWS ecosystem. It automates function/tool invocation (called action groups in Bedrock), so you don’t have to manually parse and execute actions, and it securely manages conversation state on AWS via sessions, reducing the need to maintain chat history in your application.
Bedrock 代理是Semantic Kernel中的专用 AI 代理，旨在与 Amazon Bedrock 的代理服务集成。与 OpenAI 和 Azure AI 代理一样，基岩代理通过无缝工具（作）集成实现高级多轮对话功能，但它完全在 AWS 生态系统中运行。它可以自动调用函数/工具（在 Bedrock 中称为作组），因此您不必手动解析和执行作，并且它通过会话安全地管理 AWS 上的对话状态，从而减少在应用程序中维护聊天历史记录的需要。

A Bedrock Agent differs from other agent types in a few key ways:
基岩代理在几个关键方面与其他代理类型不同：

- **AWS Managed Execution:** Unlike the OpenAI Assistant which uses OpenAI’s cloud or the Azure AI Agent which uses Azure’s Foundry service, the Bedrock Agent runs on Amazon Bedrock. You must have an AWS account with access to Bedrock (and appropriate IAM permissions) to use it. The agent’s lifecycle (creation, sessions, deletion) and certain tool executions are managed by AWS services, while function-calling tools execute locally within your environment.
  **AWS 托管执行：** 与使用 OpenAI 云的 OpenAI Assistant 或使用 Azure Foundry 服务的 Azure AI 代理不同，Bedrock Agent 在 Amazon Bedrock 上运行。您必须拥有具有 Bedrock 访问权限（以及适当的 IAM 权限）的 AWS 账户 才能使用它。代理的生命周期（创建、会话、删除）和某些工具执行由 AWS 服务管理，而函数调用工具则在您的环境中本地执行。
- **Foundation Model Selection:** When creating a Bedrock Agent, you specify which foundation model (e.g. an Amazon Titan or partner model) it should use. Only models you have been granted access to can be used. This is different from Chat Completion agents (which you instantiate with a direct model endpoint) – with Bedrock, the model is chosen at agent creation time as the agent’s default capability.
  **基础型号选择：** 创建基岩代理时，您可以指定它应该使用哪个基础模型（例如 Amazon Titan 或合作伙伴模型）。只能使用您被授予访问权限的模型。这与聊天完成代理（使用直接模型端点实例化）不同 - 使用 Bedrock，该模型在代理创建时被选择为代理的默认功能。
- **IAM Role Requirement:** Bedrock Agents require an IAM role ARN to be provided at creation. This role must have permissions to invoke the chosen model (and any integrated tools) on your behalf. This ensures the agent has the necessary privileges to perform its actions (for example, running code or accessing other AWS services) under your AWS account.
  **IAM 角色要求：** 基岩代理要求在创建时提供 IAM 角色 ARN。此角色必须具有代表你调用所选模型（以及任何集成工具）的权限。这可确保代理具有在您的 AWS 账户下执行其作（例如，运行代码或访问其他 AWS 服务）所需的权限。
- **Built-in Tools (Action Groups):** Bedrock supports built-in “action groups” (tools) that can be attached to an agent. For example, you can enable a Code Interpreter action group to allow the agent to execute Python code, or a User Input action group to allow the agent to prompt for clarification. These capabilities are analogous to OpenAI’s Code Interpreter plugin or function calling, but in AWS they are configured explicitly on the agent. A Bedrock Agent can also be extended with custom Semantic Kernel plugins (functions) for domain-specific tools, similar to other agents.
  **内置工具（作组）：**Bedrock 支持可附加到代理的内置“作组”（工具）。例如，您可以启用代码解释器作组以允许代理执行 Python 代码，或启用用户输入作组以允许代理提示澄清。这些功能类似于 OpenAI 的代码解释器插件或函数调用，但在 AWS 中，它们是在代理上显式配置的。基岩代理还可以使用自定义Semantic Kernel插件（函数）进行扩展，用于特定于领域的工具，类似于其他代理。
- **Session-based Threads:** Conversations with a Bedrock Agent occur in threads tied to Bedrock sessions on AWS. Each thread (session) is identified by a unique ID provided by the Bedrock service, and the conversation history is stored by the service rather than in-process. This means multi-turn dialogues persist on AWS, and you retrieve context via the session ID. The Semantic Kernel `BedrockAgentThread` class abstracts this detail – when you use it, it creates or continues a Bedrock session behind the scenes for the agent.
  **基于会话的线程：** 与基岩代理的对话发生在与 AWS 上的基岩会话绑定的线程中。每个线程（会话）都由基岩服务提供的唯一 ID 标识，对话历史记录由服务存储，而不是进程内存储。这意味着多轮对话在 AWS 上保留，您可以通过会话 ID 检索上下文。Semantic Kernel `BedrockAgentThread` 类抽象了这一细节——当你使用它时，它会在幕后为代理创建或继续基岩会话。

In summary, `BedrockAgent` allows you to leverage Amazon Bedrock’s powerful agent-and-tools framework through Semantic Kernel, providing goal-directed dialogue with AWS-hosted models and tools. It automates the intricacies of Bedrock’s Agent API (agent creation, session management, tool invocation) so you can interact with it in a high-level, cross-language SK interface.
总之，`BedrockAgent` 允许您通过Semantic Kernel利用 Amazon Bedrock 强大的代理和工具框架，与 AWS 托管的模型和工具提供目标导向的对话。它自动执行 Bedrock 代理 API 的复杂性（代理创建、会话管理、工具调用），因此您可以在高级、跨语言的 SK 界面中与其交互。



## Preparing Your Development Environment 准备您的开发环境

To start developing with a `BedrockAgent`, set up your environment with the appropriate Semantic Kernel packages and ensure AWS prerequisites are met.
要开始使用 `BedrockAgent` 进行开发，请使用适当的Semantic Kernel包设置您的环境，并确保满足 AWS 先决条件。

 Tip  提示

Check out the [AWS documentation](https://boto3.amazonaws.com/v1/documentation/api/latest/guide/quickstart.html#configuration) on configuring your environment to use the Bedrock API.
查看有关配置环境以使用 Bedrock API 的 [AWS 文档 ](https://boto3.amazonaws.com/v1/documentation/api/latest/guide/quickstart.html#configuration)。

Add the Semantic Kernel Bedrock Agents package to your .NET project:
将Semantic Kernel基岩代理包添加到 .NET 项目：

pwsh  普什Copy  复制

```pwsh
dotnet add package Microsoft.SemanticKernel.Agents.Bedrock --prerelease
```

This will bring in the Semantic Kernel SDK support for Bedrock, including dependencies on the AWS SDK for Bedrock. You may also need to configure AWS credentials (e.g. via environment variables or the default AWS config). The AWS SDK will use your configured credentials; make sure you have your `AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`, and default region set in your environment or AWS profile. (See AWS’s documentation on credential configuration for more details.)
这将引入对 Bedrock 的Semantic Kernel SDK 支持，包括对 AWS SDK for Bedrock 的依赖项。您可能还需要配置 AWS 凭证（例如，通过环境变量或默认的 AWS 配置）。AWS 开发工具包将使用您配置的凭证;确保您在环境或 AWS 配置文件中设置`了 AWS_ACCESS_KEY_ID`、`AWS_SECRET_ACCESS_KEY` 和默认区域。（有关更多详细信息，请参阅 AWS 关于凭证配置的文档。



## Creating a `BedrockAgent` 创建`基岩代理`

Creating a Bedrock Agent involves two steps: first, defining the agent with Amazon Bedrock (including selecting a model and providing initial instructions), and then instantiating the Semantic Kernel agent object to interact with it. When you create the agent on AWS, it starts in a non-prepared state, so an additional “prepare” operation is performed to ready it for use.
创建基岩代理涉及两个步骤：首先，使用 Amazon Bedrock 定义代理（包括选择模型和提供初始指令），然后实例化Semantic Kernel代理对象以与其交互。当您在 AWS 上创建代理时，它会以未准备状态启动，因此会执行额外的“准备”作以准备使用。

C#Copy  复制

```csharp
using Amazon.Bedrock;
using Amazon.Bedrock.Model;
using Amazon.BedrockRuntime;
using Microsoft.SemanticKernel.Agents.Bedrock;

// 1. Define a new agent on the Amazon Bedrock service
IAmazonBedrock bedrockClient = new AmazonBedrockClient();  // uses default AWS credentials & region
var createRequest = new CreateAgentRequest 
{
    AgentName = "<foundation model ID>",          // e.g., "anthropic.claude-v2" or other model
    FoundationModel = "<foundation model ID>",    // the same model, or leave null if AgentName is the model
    AgentResourceArn = "<agent role ARN>",        // IAM role ARN with Bedrock permissions
    Instruction = "<agent instructions>"
};
CreateAgentResponse createResponse = await bedrockClient.CreateAgentAsync(createRequest);

// (Optional) Provide a description as needed:
// createRequest.Description = "<agent description>";

// After creation, the agent is in a "NOT_PREPARED" state.
// Prepare the agent to load tools and finalize setup:
await bedrockClient.PrepareAgentAsync(new PrepareAgentRequest 
{
    AgentId = createResponse.Agent.AgentId
});

// 2. Create a Semantic Kernel agent instance from the Bedrock agent definition
IAmazonBedrockRuntime runtimeClient = new AmazonBedrockRuntimeClient();
BedrockAgent agent = new BedrockAgent(createResponse.Agent, bedrockClient, runtimeClient);
```

In the code above, we first use the AWS SDK (`AmazonBedrockClient`) to create an agent on Bedrock, specifying the foundation model, a name, the instructions, and the ARN of the IAM role the agent should assume. The Bedrock service responds with an agent definition (including a unique AgentId). We then call `PrepareAgentAsync` to transition the agent into a ready state (the agent will move from a CREATING status to NOT_PREPARED, then to PREPARED once ready). Finally, we construct a `BedrockAgent` object using the returned definition and the AWS clients. This `BedrockAgent` instance is what we’ll use to send messages and receive responses.
在上面的代码中，我们首先使用 AWS SDK （`AmazonBedrockClient`） 在 Bedrock 上创建代理，指定代理应承担的 IAM 角色的基础模型、名称、说明和 ARN。基岩服务使用代理定义（包括唯一的 AgentId）进行响应。然后，我们调用 `PrepareAgentAsync` 将代理转换为就绪状态（代理将从 CREATING 状态变为 NOT_PREPARED，准备就绪后变为 READYED）。最后，我们使用返回的定义和 AWS 客户端构建一个 `BedrockAgent` 对象。我们将使用这个 `BedrockAgent` 实例来发送消息和接收响应。



## Retrieving an existing `BedrockAgent` 检索现有`的 BedrockAgent`

Once an agent has been created on Bedrock, its unique identifier (Agent ID) can be used to retrieve it later. This allows you to re-instantiate a `BedrockAgent` in Semantic Kernel without recreating it from scratch.
在基岩上创建代理后，其唯一标识符（代理 ID）可用于稍后检索它。这允许您在Semantic Kernel中重新实例化 `BedrockAgent`，而无需从头开始重新创建它。

For .NET, the Bedrock agent’s identifier is a string accessible via `agent.Id`. To retrieve an existing agent by ID, use the AWS Bedrock client and then construct a new `BedrockAgent`:
对于 .NET，Bedrock 代理的标识符是可通过代理访问的字符串 `。Id`. 要按 ID 检索现有代理，请使用 AWS Bedrock 客户端，然后构建新的 `BedrockAgent`：

C#Copy  复制

```csharp
string existingAgentId = "<your agent ID>";
var getResponse = await bedrockClient.GetAgentAsync(new GetAgentRequest { AgentId = existingAgentId });
BedrockAgent agent = new BedrockAgent(getResponse.Agent, bedrockClient, runtimeClient);
```

Here we call `GetAgentAsync` on the `IAmazonBedrock` client with the known ID, which returns the agent’s definition (name, model, instructions, etc.). We then initialize a new `BedrockAgent` with that definition and the same clients. This agent instance will be linked to the existing Bedrock agent.
在这里，我们使用已知 ID 在 `IAmazonBedrock` 客户端上调用 `GetAgentAsync`，该 ID 返回代理的定义（名称、模型、说明等）。然后，我们使用该定义和相同的客户端初始化一个新的 `BedrockAgent`。此代理实例将链接到现有的基岩代理。



## Interacting with a BedrockAgent 与基岩代理交互

Once you have a BedrockAgent instance, interacting with it (sending user messages and receiving AI responses) is straightforward. The agent uses threads to manage conversation context. For a Bedrock Agent, a thread corresponds to an AWS Bedrock session. The Semantic Kernel `BedrockAgentThread` class handles session creation and tracking: when you start a new conversation, a new Bedrock session is started, and as you send messages, Bedrock maintains the alternating user/assistant message history. (Bedrock requires that chat history alternates between user and assistant messages; Semantic Kernel’s channel logic will insert placeholders if necessary to enforce this pattern.) You can invoke the agent without specifying a thread (in which case SK will create a new `BedrockAgentThread` automatically), or you can explicitly create/maintain a thread if you want to continue a conversation across multiple calls. Each invocation returns one or more responses, and you can manage the thread lifetime (e.g., deleting it when done to end the AWS session).
拥有 BedrockAgent 实例后，与其交互（发送用户消息和接收 AI 响应）非常简单。代理使用线程来管理对话上下文。对于基岩代理，线程对应于 AWS 基岩会话。Semantic Kernel `BedrockAgentThread` 类处理会话创建和跟踪：当您开始新对话时，将启动新的 Bedrock 会话，当您发送消息时，Bedrock 会维护交替的用户/助手消息历史记录。（Bedrock 要求聊天记录在用户和助手消息之间交替;如有必要，Semantic Kernel的通道逻辑将插入占位符以强制执行此模式。您可以在不指定线程的情况下调用代理（在这种情况下，SK 将自动创建新的 `BedrockAgentThread`），或者如果您想在多个调用中继续对话，您可以显式创建/维护线程。每个调用都会返回一个或多个响应，您可以管理线程生命周期（例如，在完成时将其删除以结束 AWS 会话）。

The specifics of the Bedrock agent thread are abstracted by the `BedrockAgentThread` class (which implements the common `AgentThread` interface). The `BedrockAgent` currently only supports threads of type `BedrockAgentThread`.
Bedrock 代理线程的细节由 `BedrockAgentThread` 类（实现通用 `AgentThread` 接口）抽象出来。`BedrockAgent` 目前仅支持 `BedrockAgentThread` 类型的线程。

C#Copy  复制

```csharp
BedrockAgent agent = /* (your BedrockAgent instance, as created above) */;

// Start a new conversation thread for the agent
AgentThread agentThread = new BedrockAgentThread(runtimeClient);
try
{
    // Send a user message and iterate over the response(s)
    var userMessage = new ChatMessageContent(AuthorRole.User, "<your user input>");
    await foreach (ChatMessageContent response in agent.InvokeAsync(userMessage, agentThread))
    {
        Console.WriteLine(response.Content);
    }
}
finally
{
    // Clean up the thread and (optionally) the agent when done
    await agentThread.DeleteAsync();
    await agent.Client.DeleteAgentAsync(new DeleteAgentRequest { AgentId = agent.Id });
}
```

In this example, we explicitly create a `BedrockAgentThread` (passing in the `runtimeClient`, which it uses to communicate with the Bedrock runtime service). We then call `agent.InvokeAsync(...)` with a `ChatMessageContent` representing a user’s message. `InvokeAsync` returns an async stream of responses – in practice, a Bedrock Agent typically returns one final response per invocation (since intermediate tool actions are handled separately), so you’ll usually get a single `ChatMessageContent` from the loop. We print out the assistant’s reply (`response.Content`). In the finally block, we delete the thread, which ends the Bedrock session on AWS. We also delete the agent itself in this case (since we created it just for this example) – this step is optional and only needed if you do not intend to reuse the agent again (see Deleting a BedrockAgent below).
在此示例中，我们显式创建了一个 `BedrockAgentThread`（传入 `runtimeClient`，它使用它与 Bedrock 运行时服务进行通信）。然后我们打电话给`代理。InvokeAsync（...）` 与表示用户消息的 `ChatMessageContent` 一起使用。`InvokeAsync` 返回异步响应流 - 实际上，基岩代理通常每次调用返回一个最终响应（因为中间工具作是单独处理的），因此您通常会从循环中获得单个 `ChatMessageContent`。我们打印出助手的回复（` 回复。内容 `）。在 finally 块中，我们删除线程，这结束了 AWS 上的基岩会话。在这种情况下，我们还删除了代理本身（因为我们只是为这个例子创建了它）——此步骤是可选的，仅当你不打算再次重用代理时才需要（请参阅下面的删除 BedrockAgent）。

You can continue an existing conversation by reusing the same `agentThread` for subsequent calls. For example, you might loop reading user input and calling `InvokeAsync` each time with the same thread to carry on a multi-turn dialogue. You can also create a BedrockAgentThread with a known session ID to resume a conversation that was saved previously:
您可以通过在后续调用中重复使用相同的 `agentThread` 来继续现有对话。例如，可以循环读取用户输入，并每次使用同一线程调用 `InvokeAsync` 以进行多轮对话。您还可以使用已知会话 ID 创建 BedrockAgentThread，以恢复之前保存的对话：

C#Copy  复制

```csharp
string sessionId = "<existing Bedrock session ID>";
AgentThread thread = new BedrockAgentThread(runtimeClient, sessionId);
// Now `InvokeAsync` using this thread will continue the conversation from that session
```



## Deleting a `BedrockAgent` 删除 `BedrockAgent`

Bedrock Agents are persistent resources in your AWS account – they will remain (and potentially incur costs or count against service limits) until deleted. If you no longer need an agent you’ve created, you should delete it via the Bedrock service API.
基岩代理是您 AWS 账户中的持久资源 - 它们将保留（并可能产生成本或计入服务限制），直到被删除。如果您不再需要创建的代理，则应通过基岩服务 API 将其删除。

Use the Bedrock client to delete by agent ID. For example:
使用 Bedrock 客户端按代理 ID 删除。例如：

C#Copy  复制

```csharp
await bedrockAgent.Client.DeleteAgentAsync(new() { AgentId = bedrockAgent.Id });
```

After this call, the agent’s status will change and it will no longer be usable. (Attempting to invoke a deleted agent will result in an error.)
在此通话后，代理的状态将发生变化，并且将不再可用。（尝试调用已删除的代理将导致错误。

> **Note:** Deleting a Bedrock agent does not automatically terminate its ongoing sessions. If you have long-running sessions (threads), you should end those by deleting the threads (which calls Bedrock’s EndSession and DeleteSession under the hood). In practice, deleting a thread (as shown in the examples above) ends the session.
> **注意：** 删除基岩代理不会自动终止其正在进行的会话。如果您有长时间运行的会话（线程），您应该通过删除线程（在后台调用 Bedrock 的 EndSession 和 DeleteSession）来结束这些会话。实际上，删除线程（如上面的示例所示）会结束会话。



## Handling Intermediate Messages with a `BedrockAgent` 使用 `BedrockAgent` 处理中间消息

When a Bedrock Agent invokes tools (action groups) to arrive at an answer, those intermediate steps (function calls and results) are by default handled internally. The agent’s final answer will reference the outcome of those tools but will not automatically include verbose step-by-step details. However, Semantic Kernel allows you to tap into those intermediate messages for logging or custom handling by providing a callback.
当基岩代理调用工具（作组）来得出答案时，默认情况下，这些中间步骤（函数调用和结果）在内部处理。代理的最终答案将参考这些工具的结果，但不会自动包含详细的分步详细信息。但是，Semantic Kernel允许您通过提供回调来利用这些中间消息进行日志记录或自定义处理。

During `agent.invoke(...)` or `agent.invoke_stream(...)`, you can supply an `on_intermediate_message` callback function. This callback will be invoked for each intermediate message generated in the process of formulating the final response. Intermediate messages may include `FunctionCallContent` (when the agent decides to call a function/tool) and `FunctionResultContent` (when a tool returns a result).
在 `agent.invoke（...）` 或 `agent.invoke_stream（...）` 期间，您可以提供 `on_intermediate_message` 回调函数。对于在制定最终响应的过程中生成的每个中间消息，将调用此回调。中间消息可能包括 `FunctionCallContent`（当代理决定调用函数/工具时）和 `FunctionResultContent`（当工具返回结果时）。

For example, suppose our Bedrock Agent has access to a simple plugin (or built-in tool) for menu information, similar to the examples used with OpenAI Assistant:
例如，假设我们的基岩代理可以访问一个简单的插件（或内置工具）来获取菜单信息，类似于 OpenAI Assistant 使用的示例：

Callback support for intermediate messages in BedrockAgent (C#) follows a similar pattern, but the exact API is under development. (Future releases will enable registering a delegate to handle `FunctionCallContent` and `FunctionResultContent` during `InvokeAsync`.)
BedrockAgent （C#） 中对中间消息的回调支持遵循类似的模式，但确切的 API 正在开发中。（将来的版本将允许注册委托，以便在 `InvokeAsync` 期间处理 `FunctionCallContent` 和 `FunctionResultContent`。



## Using Declarative YAML to Define a Bedrock Agent 使用声明式 YAML 定义基岩代理

Semantic Kernel’s agent framework supports a declarative schema for defining agents via YAML (or JSON). This allows you to specify an agent’s configuration – its type, models, tools, etc. – in a file and then load that agent definition at runtime without writing imperative code to construct it.
Semantic Kernel的代理框架支持声明式模式，用于通过 YAML（或 JSON）定义代理。这允许您在文件中指定代理的配置（其类型、模型、工具等），然后在运行时加载该代理定义，而无需编写命令式代码来构造它。

> **Note:** YAML-based agent definitions are an emerging feature and may be experimental. Ensure you are using a Semantic Kernel version that supports YAML agent loading, and refer to the latest docs for any format changes.
> **注意：** 基于 YAML 的代理定义是一项新兴功能，可能是实验性的。确保您使用的是支持 YAML 代理加载的Semantic Kernel版本，并有关任何格式更改，请参阅最新文档。

Using a declarative spec can simplify configuration, especially if you want to easily switch agent setups or use a configuration file approach. For a Bedrock Agent, a YAML definition might look like:
使用声明性规范可以简化配置，特别是当您想要轻松切换代理设置或使用配置文件方法时。对于基岩代理，YAML 定义可能如下所示：

YAMLCopy  复制

```yaml
type: bedrock_agent
name: MenuAgent
description: Agent that answers questions about a restaurant menu
instructions: You are a restaurant assistant that provides daily specials and prices.
model:
  id: anthropic.claude-v2
agent_resource_role_arn: arn:aws:iam::123456789012:role/BedrockAgentRole
tools:
  - type: code_interpreter
  - type: user_input
  - name: MenuPlugin
    type: kernel_function
```

In this (hypothetical) YAML, we define an agent of type `bedrock_agent`, give it a name and instructions, specify the foundation model by ID, and provide the ARN of the role it should use. We also declare a couple of tools: one enabling the built-in Code Interpreter, another enabling the built-in User Input tool, and a custom MenuPlugin (which would be defined separately in code and registered as a kernel function). Such a file encapsulates the agent’s setup in a human-readable form.
在这个（假设的）YAML 中，我们定义了一个类型`为 bedrock_agent` 的代理，为其指定名称和指令，通过 ID 指定基础模型，并提供它应该使用的角色的 ARN。我们还声明了几个工具：一个启用内置代码解释器，另一个启用内置用户输入工具，以及自定义 MenuPlugin（将在代码中单独定义并注册为内核函数）。这样的文件以人类可读的形式封装代理的设置。

To instantiate an agent from YAML, use the static loader with an appropriate factory. For example:
要从 YAML 实例化代理，请使用带有适当工厂的静态加载器。例如：

C#Copy  复制

```csharp
string yamlText = File.ReadAllText("bedrock-agent.yaml");
var factory = new BedrockAgentFactory();  // or an AggregatorAgentFactory if multiple types are used
Agent myAgent = await KernelAgentYaml.FromAgentYamlAsync(kernel, yamlText, factory);
```

This will parse the YAML and produce a `BedrockAgent` instance (or other type based on the `type` field) using the provided kernel and factory.
这将解析 YAML 并使用提供的内核和工厂生成 `BedrockAgent` 实例（或基于`类型`字段的其他类型）。

Using a declarative schema can be particularly powerful for scenario configuration and testing, as you can swap out models or instructions by editing a config file rather than changing code. Keep an eye on Semantic Kernel’s documentation and samples for more on YAML agent definitions as the feature evolves.
使用声明性模式对于方案配置和测试特别强大，因为您可以通过编辑配置文件而不是更改代码来交换模型或指令。随着功能的发展，请密切关注Semantic Kernel的文档和示例，了解有关 YAML 代理定义的更多信息。



## Further Resources  更多资源

- **AWS Bedrock Documentation**: To learn more about Amazon Bedrock’s agent capabilities, see *Amazon Bedrock Agents* in the [AWS documentation](https://docs.aws.amazon.com/bedrock/) (e.g., how to configure foundation model access and IAM roles). Understanding the underlying service will help in setting correct permissions and making the most of built-in tools.
  **AWS 基岩文档** ：要了解有关 Amazon Bedrock 代理功能的更多信息，请参阅 [AWS 文档](https://docs.aws.amazon.com/bedrock/)中的 *Amazon 基岩代理* （例如，如何配置基础模型访问权限和 IAM 角色）。了解底层服务将有助于设置正确的权限并充分利用内置工具。
- **Semantic Kernel Samples**: The Semantic Kernel repository contains [concept samples](https://github.com/microsoft/semantic-kernel/tree/main/python/samples/concepts/agents/bedrock_agent) for Bedrock Agents. For example, the **Bedrock Agent basic chat sample** in the Python samples demonstrates simple Q&A with a `BedrockAgent`, and the **Bedrock Agent with Code Interpreter sample** shows how to enable and use the Code Interpreter tool. These samples can be a great starting point to see `BedrockAgent` in action.
  **Semantic Kernel示例：** Semantic Kernel存储库包含基岩代理的[概念示例 ](https://github.com/microsoft/semantic-kernel/tree/main/python/samples/concepts/agents/bedrock_agent)。例如，Python 示例中的 **Bedrock Agent 基本聊天示例**演示了使用 `BedrockAgent` 的简单问答， **而带有代码解释器的 Bedrock Agent 示例**演示了如何启用和使用代码解释器工具。这些示例可以作为了解 `BedrockAgent` 实际应用的一个很好的起点。

With the Amazon Bedrock Agent integrated, Semantic Kernel enables truly multi-platform AI solutions – whether you use OpenAI, Azure OpenAI, or AWS Bedrock, you can build rich conversational applications with tool integration using a consistent framework. The `BedrockAgent` opens the door to leveraging AWS’s latest foundation models and secure, extensible agent paradigm within your Semantic Kernel projects.
通过集成 Amazon Bedrock Agent，Semantic Kernel可实现真正的多平台 AI 解决方案 - 无论您使用 OpenAI、Azure OpenAI 还是 AWS Bedrock，您都可以使用一致的框架通过工具集成构建丰富的对话式应用程序。`BedrockAgent` 为您的在Semantic Kernel项目中利用 AWS 的最新基础模型和安全、可扩展的代理范例打开了大门。



## Next Steps  后续步骤

[  探索聊天完成代理](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/agent/agent-types/chat-completion-agent)