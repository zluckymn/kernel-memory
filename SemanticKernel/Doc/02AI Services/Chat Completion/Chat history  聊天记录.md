# Chat history  聊天记录

- 01/31/2025

The chat history object is used to maintain a record of messages in a chat session. It is used to store messages from different authors, such as users, assistants, tools, or the system. As the primary mechanism for sending and receiving messages, the chat history object is essential for maintaining context and continuity in a conversation.
聊天历史记录对象用于维护聊天会话中的消息记录。它用于存储来自不同作者的消息，例如用户、助手、工具或系统。作为发送和接收消息的主要机制，聊天记录对象对于维护对话中的上下文和连续性至关重要。



## Creating a chat history object 创建聊天记录对象

A chat history object is a list under the hood, making it easy to create and add messages to.
聊天记录对象是底层列表，可以轻松创建和添加消息。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.ChatCompletion;

// Create a chat history object
ChatHistory chatHistory = [];

chatHistory.AddSystemMessage("You are a helpful assistant.");
chatHistory.AddUserMessage("What's available to order?");
chatHistory.AddAssistantMessage("We have pizza, pasta, and salad available to order. What would you like to order?");
chatHistory.AddUserMessage("I'd like to have the first option, please.");
```



## Adding richer messages to a chat history 向聊天记录添加更丰富的消息

The easiest way to add messages to a chat history object is to use the methods above. However, you can also add messages manually by creating a new `ChatMessage` object. This allows you to provide additional information, like names and images content.
将消息添加到聊天记录对象的最简单方法是使用上述方法。但是，您也可以通过创建新的 `ChatMessage` 对象来手动添加消息。这允许您提供其他信息，例如名称和图像内容。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.ChatCompletion;

// Add system message
chatHistory.Add(
    new() {
        Role = AuthorRole.System,
        Content = "You are a helpful assistant"
    }
);

// Add user message with an image
chatHistory.Add(
    new() {
        Role = AuthorRole.User,
        AuthorName = "Laimonis Dumins",
        Items = [
            new TextContent { Text = "What available on this menu" },
            new ImageContent { Uri = new Uri("https://example.com/menu.jpg") }
        ]
    }
);

// Add assistant message
chatHistory.Add(
    new() {
        Role = AuthorRole.Assistant,
        AuthorName = "Restaurant Assistant",
        Content = "We have pizza, pasta, and salad available to order. What would you like to order?"
    }
);

// Add additional message from a different user
chatHistory.Add(
    new() {
        Role = AuthorRole.User,
        AuthorName = "Ema Vargova",
        Content = "I'd like to have the first option, please."
    }
);
```



## Simulating function calls 模拟函数调用

In addition to user, assistant, and system roles, you can also add messages from the tool role to simulate function calls. This is useful for teaching the AI how to use plugins and to provide additional context to the conversation.
除了用户、助手和系统角色外，您还可以添加来自工具角色的消息来模拟函数调用。这对于教 AI 如何使用插件以及为对话提供额外的上下文非常有用。

For example, to inject information about the current user in the chat history without requiring the user to provide the information or having the LLM waste time asking for it, you can use the tool role to provide the information directly.
例如，要在聊天记录中注入有关当前用户的信息，而无需用户提供信息或让 LLM 浪费时间请求信息，您可以使用工具角色直接提供信息。

Below is an example of how we're able to provide user allergies to the assistant by simulating a function call to the `User` plugin.
下面是一个示例，说明我们如何通过模拟对 `User` 插件的函数调用来提供对助手的用户过敏。

 Tip  提示

Simulated function calls is particularly helpful for providing details about the current user(s). Today's LLMs have been trained to be particularly sensitive to user information. Even if you provide user details in a system message, the LLM may still choose to ignore it. If you provide it via a user message, or tool message, the LLM is more likely to use it.
模拟函数调用对于提供有关当前用户的详细信息特别有用。今天的法学硕士经过训练，对用户信息特别敏感。即使您在系统消息中提供了用户详细信息，LLM 仍可能选择忽略它。如果您通过用户消息或工具消息提供它，LLM 更有可能使用它。

C#Copy  复制

```csharp
// Add a simulated function call from the assistant
chatHistory.Add(
    new() {
        Role = AuthorRole.Assistant,
        Items = [
            new FunctionCallContent(
                functionName: "get_user_allergies",
                pluginName: "User",
                id: "0001",
                arguments: new () { {"username", "laimonisdumins"} }
            ),
            new FunctionCallContent(
                functionName: "get_user_allergies",
                pluginName: "User",
                id: "0002",
                arguments: new () { {"username", "emavargova"} }
            )
        ]
    }
);

// Add a simulated function results from the tool role
chatHistory.Add(
    new() {
        Role = AuthorRole.Tool,
        Items = [
            new FunctionResultContent(
                functionName: "get_user_allergies",
                pluginName: "User",
                id: "0001",
                result: "{ \"allergies\": [\"peanuts\", \"gluten\"] }"
            )
        ]
    }
);
chatHistory.Add(
    new() {
        Role = AuthorRole.Tool,
        Items = [
            new FunctionResultContent(
                functionName: "get_user_allergies",
                pluginName: "User",
                id: "0002",
                result: "{ \"allergies\": [\"dairy\", \"soy\"] }"
            )
        ]
    }
);
```

 Important  重要

When simulating tool results, you must always provide the `id` of the function call that the result corresponds to. This is important for the AI to understand the context of the result. Some LLMs, like OpenAI, will throw an error if the `id` is missing or if the `id` does not correspond to a function call.
模拟工具结果时，必须始终提供结果对应的函数调用的 `ID`。这对于人工智能理解结果的上下文非常重要。如果 `id` 丢失或 `id` 与函数调用不对应，某些 LLM（如 OpenAI）将抛出错误。



## Inspecting a chat history object 检查聊天记录对象

Whenever you pass a chat history object to a chat completion service with auto function calling enabled, the chat history object will be manipulated so that it includes the function calls and results. This allows you to avoid having to manually add these messages to the chat history object and also allows you to inspect the chat history object to see the function calls and results.
每当您将聊天历史记录对象传递给启用了自动函数调用的聊天完成服务时，都会对聊天历史记录对象进行作，以便它包含函数调用和结果。这样，就可以避免手动将这些消息添加到聊天记录对象，还可以检查聊天记录对象以查看函数调用和结果。

You must still, however, add the final messages to the chat history object. Below is an example of how you can inspect the chat history object to see the function calls and results.
但是，您仍然必须将最终消息添加到聊天记录对象中。下面是一个示例，说明如何检查聊天记录对象以查看函数调用和结果。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.ChatCompletion;

ChatHistory chatHistory = [
    new() {
        Role = AuthorRole.User,
        Content = "Please order me a pizza"
    }
];

// Get the current length of the chat history object
int currentChatHistoryLength = chatHistory.Count;

// Get the chat message content
ChatMessageContent results = await chatCompletionService.GetChatMessageContentAsync(
    chatHistory,
    kernel: kernel
);

// Get the new messages added to the chat history object
for (int i = currentChatHistoryLength; i < chatHistory.Count; i++)
{
    Console.WriteLine(chatHistory[i]);
}

// Print the final message
Console.WriteLine(results);

// Add the final message to the chat history object
chatHistory.Add(results);
```



## Chat History Reduction  聊天记录减少

Managing chat history is essential for maintaining context-aware conversations while ensuring efficient performance. As a conversation progresses, the history object can grow beyond the limits of a model’s context window, affecting response quality and slowing down processing. A structured approach to reducing chat history ensures that the most relevant information remains available without unnecessary overhead.
管理聊天记录对于维护上下文感知对话同时确保高效性能至关重要。随着对话的进行，历史记录对象可能会超出模型上下文窗口的限制，从而影响响应质量并减慢处理速度。减少聊天记录的结构化方法可确保最相关的信息仍然可用，而不会产生不必要的开销。



### Why Reduce Chat History?  为什么要减少聊天记录？

- Performance Optimization: Large chat histories increase processing time. Reducing their size helps maintain fast and efficient interactions.
  性能优化：大型聊天记录会增加处理时间。减小它们的大小有助于保持快速高效的交互。
- Context Window Management: Language models have a fixed context window. When the history exceeds this limit, older messages are lost. Managing chat history ensures that the most important context remains accessible.
  上下文窗口管理：语言模型具有固定的上下文窗口。当历史记录超过此限制时，较旧的邮件将丢失。管理聊天记录可确保最重要的上下文仍然可访问。
- Memory Efficiency: In resource-constrained environments such as mobile applications or embedded systems, unbounded chat history can lead to excessive memory usage and slow performance.
  内存效率：在移动应用程序或嵌入式系统等资源受限的环境中，无限制的聊天记录可能会导致内存使用过多和性能下降。
- Privacy and Security: Retaining unnecessary conversation history increases the risk of exposing sensitive information. A structured reduction process minimizes data retention while maintaining relevant context.
  隐私和安全：保留不必要的对话历史记录会增加敏感信息泄露的风险。结构化缩减过程可最大限度地减少数据保留，同时保持相关上下文。



### Strategies for Reducing Chat History 减少聊天记录的策略

Several approaches can be used to keep chat history manageable while preserving essential information:
可以使用多种方法来保持聊天记录的可管理性，同时保留重要信息：

- Truncation: The oldest messages are removed when the history exceeds a predefined limit, ensuring only recent interactions are retained.
  截断：当历史记录超过预定义限制时，最旧的消息将被删除，确保仅保留最近的交互。
- Summarization: Older messages are condensed into a summary, preserving key details while reducing the number of stored messages.
  摘要：较旧的消息被压缩为摘要，保留关键细节，同时减少存储的消息数量。
- Token-Based: Token-based reduction ensures chat history stays within a model’s token limit by measuring total token count and removing or summarizing older messages when the limit is exceeded.
  基于令牌：基于令牌的减少通过测量令牌总数并在超过限制时删除或汇总旧消息，确保聊天历史记录保持在模型的令牌限制范围内。

A Chat History Reducer automates these strategies by evaluating the history’s size and reducing it based on configurable parameters such as target count (the desired number of messages to retain) and threshold count (the point at which reduction is triggered). By integrating these reduction techniques, chat applications can remain responsive and performant without compromising conversational context.
聊天历史记录减少器通过评估历史记录的大小并根据可配置的参数（例如目标计数（要保留的所需消息数）和阈值计数（触发减少的点））来减少历史记录的大小来自动执行这些策略。通过集成这些缩减技术，聊天应用程序可以保持响应速度和性能，而不会影响对话上下文。

In the .NET version of Semantic Kernel, the Chat History Reducer abstraction is defined by the `IChatHistoryReducer` interface:
在 .NET 版本的Kernel中，聊天历史 Reducer 抽象由 `IChatHistoryReducer` 接口定义：

C#Copy  复制

```csharp
namespace Microsoft.SemanticKernel.ChatCompletion;

[Experimental("SKEXP0001")]
public interface IChatHistoryReducer
{
    Task<IEnumerable<ChatMessageContent>?> ReduceAsync(IReadOnlyList<ChatMessageContent> chatHistory, CancellationToken cancellationToken = default);
}
```

This interface allows custom implementations for chat history reduction.
该界面允许自定义实现以减少聊天记录。

Additionally, Semantic Kernel provides built-in reducers:
此外，Semantic Kernel 还提供了内置的 reducer：

- `ChatHistoryTruncationReducer` - truncates chat history to a specified size and discards the removed messages. The reduction is triggered when the chat history length exceeds the limit.
  `ChatHistoryTruncationReducer` - 将聊天记录截断为指定大小并丢弃已删除的消息。当聊天记录长度超过限制时，将触发减少。
- `ChatHistorySummarizationReducer` - truncates chat history, summarizes the removed messages and adds the summary back into the chat history as a single message.
  `ChatHistorySummarizationReducer` - 截断聊天记录，汇总已删除的消息，并将摘要作为单个消息添加回聊天记录。

Both reducers always preserve system messages to retain essential context for the model.
两个 reducer 始终保留系统消息以保留模型的基本上下文。

The following example demonstrates how to retain only the last two user messages while maintaining conversation flow:
以下示例演示了如何在保持对话流的同时仅保留最后两条用户消息：

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

var chatService = new OpenAIChatCompletionService(
    modelId: "<model-id>",
    apiKey: "<api-key>");

var reducer = new ChatHistoryTruncationReducer(targetCount: 2); // Keep system message and last user message

var chatHistory = new ChatHistory("You are a librarian and expert on books about cities");

string[] userMessages = [
    "Recommend a list of books about Seattle",
    "Recommend a list of books about Dublin",
    "Recommend a list of books about Amsterdam",
    "Recommend a list of books about Paris",
    "Recommend a list of books about London"
];

int totalTokenCount = 0;

foreach (var userMessage in userMessages)
{
    chatHistory.AddUserMessage(userMessage);

    Console.WriteLine($"\n>>> User:\n{userMessage}");

    var reducedMessages = await reducer.ReduceAsync(chatHistory);

    if (reducedMessages is not null)
    {
        chatHistory = new ChatHistory(reducedMessages);
    }

    var response = await chatService.GetChatMessageContentAsync(chatHistory);

    chatHistory.AddAssistantMessage(response.Content!);

    Console.WriteLine($"\n>>> Assistant:\n{response.Content!}");

    if (response.InnerContent is OpenAI.Chat.ChatCompletion chatCompletion)
    {
        totalTokenCount += chatCompletion.Usage?.TotalTokenCount ?? 0;
    }
}

Console.WriteLine($"Total Token Count: {totalTokenCount}");
```

More examples can be found in the Semantic Kernel [repository](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/ChatCompletion/MultipleProviders_ChatHistoryReducer.cs).
更多示例可以在Kernel[存储库](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/ChatCompletion/MultipleProviders_ChatHistoryReducer.cs)中找到。



## Next steps  后续步骤

Now that you know how to create and manage a chat history object, you can learn more about function calling in the [Function calling](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/) topic.
现在，你已经知道如何创建和管理聊天记录对象，可以在[函数调用主题](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/)中了解有关函数调用的更多信息。