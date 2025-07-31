# Exploring the Semantic Kernel `CopilotStudioAgent` 探索Semantic Kernel `CopilotStudioAgent`

- 05/23/2025

Detailed API documentation related to this discussion is available at:
与此讨论相关的详细 API 文档可在以下位置获得：

> The CopilotStudioAgent for .NET is coming soon.
> CopilotStudioAgent for .NET 即将推出。



## What is a `CopilotStudioAgent`? 什么是 `CopilotStudioAgent`？

A `CopilotStudioAgent` is an integration point within the Semantic Kernel framework that enables seamless interaction with [Microsoft Copilot Studio](https://copilotstudio.microsoft.com/) agents using programmatic APIs. This agent allows you to:
`CopilotStudioAgent` 是Semantic Kernel框架中的一个集成点，可使用编程 API 与 [Microsoft Copilot Studio](https://copilotstudio.microsoft.com/) 代理进行无缝交互。该代理允许您：

- Automate conversations and invoke existing Copilot Studio agents from Python code.
  自动执行对话并从 Python 代码调用现有的 Copilot Studio 代理。
- Maintain rich conversational history using threads, preserving context across messages.
  使用线程维护丰富的对话历史记录，保留消息之间的上下文。
- Leverage advanced knowledge retrieval, web search, and data integration capabilities made available within Microsoft Copilot Studio.
  利用 Microsoft Copilot Studio 中提供的高级知识检索、Web 搜索和数据集成功能。

 Note  注意

Knowledge sources/tools must be configured **within** Microsoft Copilot Studio before they can be accessed via the agent.
必须在 Microsoft Copilot Studio **中**配置知识源/工具，然后才能通过代理访问它们。



## Preparing Your Development Environment 准备您的开发环境

To develop with the `CopilotStudioAgent`, you must have your environment and authentication set up correctly.
若要使用 `CopilotStudioAgent` 进行开发，必须正确设置环境和身份验证。

> The CopilotStudioAgent for .NET is coming soon.
> CopilotStudioAgent for .NET 即将推出。



## Creating and Configuring a `CopilotStudioAgent` Client 创建和配置 `CopilotStudioAgent` 客户端

You may rely on environment variables for most configuration, but can explicitly create and customize the agent client as needed.
大多数配置可能依赖环境变量，但可以根据需要显式创建和自定义代理客户端。

> The CopilotStudioAgent for .NET is coming soon.
> CopilotStudioAgent for .NET 即将推出。



## Interacting with a `CopilotStudioAgent` 与 `CopilotStudioAgent` 交互

The core workflow is similar to other Semantic Kernel agents: provide user input(s), receive responses, maintain context via threads.
核心工作流程与其他Semantic Kernel代理类似：提供用户输入、接收响应、通过线程维护上下文。

> The CopilotStudioAgent for .NET is coming soon.
> CopilotStudioAgent for .NET 即将推出。



## Using Plugins with a `CopilotStudioAgent` 将插件与 `CopilotStudioAgent` 一起使用

Semantic Kernel allows composition of agents and plugins. Although the primary extensibility for Copilot Studio comes via the Studio itself, you can compose plugins as with other agents.
Semantic Kernel允许组合代理和插件。尽管 Copilot Studio 的主要可扩展性来自 Studio 本身，但您可以像使用其他代理一样编写插件。

> The CopilotStudioAgent for .NET is coming soon.
> CopilotStudioAgent for .NET 即将推出。



## Advanced Features  高级功能

A `CopilotStudioAgent` can leverage advanced Copilot Studio-enhanced abilities, depending on how the target agent is configured in the Studio environment:
`CopilotStudioAgent` 可以利用高级 Copilot Studio 增强功能，具体取决于目标代理在 Studio 环境中的配置方式：

- **Knowledge Retrieval** — responds based on pre-configured knowledge sources in the Studio.
  **知识检索** — 根据 Studio 中预配置的知识源进行响应。
- **Web Search** — if web search is enabled in your Studio agent, queries will use Bing Search.
  **Web 搜索** - 如果在 Studio 代理中启用了 Web 搜索，则查询将使用必应搜索。
- **Custom Auth or APIs** — via Power Platform and Studio plug-ins; direct OpenAPI binding is not currently first-class in SK integration.
  **自定义身份验证或 API** - 通过 Power Platform 和 Studio 插件;直接 OpenAPI 绑定目前在 SK 集成中并不是一流的。

> The CopilotStudioAgent for .NET is coming soon.
> CopilotStudioAgent for .NET 即将推出。



## How-To  作方法

For practical examples of using a `CopilotStudioAgent`, see our code samples on GitHub:
有关使用 `CopilotStudioAgent` 的实际示例，请参阅 GitHub 上的代码示例：

> The CopilotStudioAgent for .NET is coming soon.
> CopilotStudioAgent for .NET 即将推出。

------

**Notes:  笔记：**

- For more information or troubleshooting, see [Microsoft Copilot Studio documentation](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/copilot-studio-agent-builder-build).
  有关详细信息或故障排除，请参阅 [Microsoft Copilot Studio 文档 ](https://learn.microsoft.com/en-us/microsoft-365-copilot/extensibility/copilot-studio-agent-builder-build)。
- Only features and tools separately enabled and published in your Studio agent will be available via the Semantic Kernel interface.
  只有在 Studio 代理中单独启用和发布的功能和工具才能通过Semantic Kernel界面使用。
- Streaming, plugin deployment, and programmatic tool addition are planned for future releases.
  流式处理、插件部署和编程工具添加计划在未来的版本中进行。