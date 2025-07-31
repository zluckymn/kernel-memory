# Multi-modal chat completion 多模态聊天完成

- 11/22/2024

Many AI services support input using images, text and potentially more at the same time, allowing developers to blend together these different inputs. This allows for scenarios such as passing an image and asking the AI model a specific question about the image.
许多 AI 服务支持同时使用图像、文本等进行输入，从而允许开发人员将这些不同的输入混合在一起。这允许诸如传递图像并向 AI 模型询问有关图像的特定问题等场景。



## Using images with chat completion 使用图像完成聊天

The Semantic Kernel chat completion connectors support passing both images and text at the same time to a chat completion AI model. Note that not all AI models or AI services support this behavior.
Kernel聊天完成连接器支持将图像和文本同时传递给聊天完成 AI 模型。请注意，并非所有 AI 模型或 AI 服务都支持此行为。

After you have constructed a chat completion service using the steps outlined in the [Chat completion](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/) article, you can provide images and text in the following way.
使用[聊天完成](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/)一文中概述的步骤构建聊天完成服务后，可以通过以下方式提供图像和文本。

C#Copy  复制

```csharp
// Load an image from disk.
byte[] bytes = File.ReadAllBytes("path/to/image.jpg");

// Create a chat history with a system message instructing
// the LLM on its required role.
var chatHistory = new ChatHistory("Your job is describing images.");

// Add a user message with both the image and a question
// about the image.
chatHistory.AddUserMessage(
[
    new TextContent("What’s in this image?"),
    new ImageContent(bytes, "image/jpeg"),
]);

// Invoke the chat completion model.
var reply = await chatCompletionService.GetChatMessageContentAsync(chatHistory);
Console.WriteLine(reply.Content);
```