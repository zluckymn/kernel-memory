# Planning  规划

- 06/11/2025

 

Once you have multiple plugins, you then need a way for your AI agent to use them together to solve a user’s need. This is where planning comes in.
一旦你有了多个插件，你就需要一种方法让你的 AI 代理将它们一起使用来解决用户的需求。这就是规划的用武之地。

Early on, Semantic Kernel introduced the concept of planners that used prompts to request the AI to choose which functions to invoke. Since Semantic Kernel was introduced, however, OpenAI introduced a native way for the model to invoke or “call” a function: [function calling](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/). Other AI models like Gemini, Claude, and Mistral have since adopted function calling as a core capability, making it a cross-model supported feature.
早期，Semantic Kernel引入了规划器的概念，它使用提示来请求 AI 选择要调用的函数。然而，自从引入Semantic Kernel以来，OpenAI 为模型引入了一种调用或“调用”函数的原生方式：[ 函数调用 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/)。此后，Gemini、Claude 和 Mistral 等其他 AI 模型已采用函数调用作为核心功能，使其成为跨模型支持的功能。

Because of these advancements, Semantic Kernel has evolved to use function calling as the primary way to plan and execute tasks.
由于这些进步，Semantic Kernel已经发展到使用函数调用作为规划和执行任务的主要方式。

 Important  重要

Function calling is only available in OpenAI models that are 0613 or newer. If you use an older model (e.g., 0314), this functionality will return an error. We recommend using the latest OpenAI models to take advantage of this feature.
函数调用仅适用于 0613 或更高版本的 OpenAI 模型。如果您使用较旧的模型（例如 0314），此功能将返回错误。我们建议使用最新的 OpenAI 模型来利用此功能。



## How does function calling create a "plan"? 函数调用如何创建“计划”？

At its simplest, function calling is merely a way for an AI to invoke a function with the right parameters. Take for example a user wants to turn on a light bulb. Assuming the AI has the right plugin, it can call the function to turn on the light.
简单来说，函数调用只是 AI 使用正确参数调用函数的一种方式。以用户想要打开灯泡为例。假设 AI 有正确的插件，它可以调用函数来打开灯。

  展开表

| Role  角色                                               | Message  消息                                                |
| :------------------------------------------------------- | :----------------------------------------------------------- |
| 🔵 **User**  🔵 **用户**                                   | Please turn on light #1 请打开灯 #1                          |
| 🔴 **Assistant (function call)** 🔴 **助手 （函数 调用）** | `Lights.change_state(1, { "isOn": true })`                   |
| 🟢 **Tool**  🟢 **工具**                                   | `{ "id": 1, "name": "Table Lamp", "isOn": true, "brightness": 100, "hex": "FF0000" }` |
| 🔴 **Assistant**  🔴 **助理**                              | The lamp is now on 灯现在亮了                                |

But what if the user doesn't know the ID of the light? Or what if the user wants to turn on all the lights? This is where planning comes in. Today's LLM models are capable of iteratively calling functions to solve a user's need. This is accomplished by creating a feedback loop where the AI can call a function, check the result, and then decide what to do next.
但是，如果用户不知道灯的 ID 怎么办？或者，如果用户想打开所有灯怎么办？这就是规划的用武之地。当今的 LLM 模型能够迭代调用函数来解决用户的需求。这是通过创建一个反馈循环来实现的，人工智能可以在其中调用函数，检查结果，然后决定下一步该做什么。

For example, a user may ask the AI to "toggle" a light bulb. The AI would first need to check the state of the light bulb before deciding whether to turn it on or off.
例如，用户可能会要求人工智能“切换”灯泡。人工智能首先需要检查灯泡的状态，然后再决定是否打开或关闭它。

  展开表

| Role  角色                                               | Message  消息                                                |
| :------------------------------------------------------- | :----------------------------------------------------------- |
| 🔵 **User**  🔵 **用户**                                   | Please toggle all the lights 请切换所有灯                    |
| 🔴 **Assistant (function call)** 🔴 **助手 （函数 调用）** | `Lights.get_lights()`                                        |
| 🟢 **Tool**  🟢 **工具**                                   | `{ "lights": [ { "id": 1, "name": "Table Lamp", "isOn": true, "brightness": 100, "hex": "FF0000" }, { "id": 2, "name": "Ceiling Light", "isOn": false, "brightness": 0, "hex": "FFFFFF" } ] }` |
| 🔴 **Assistant (function call)** 🔴 **助手 （函数 调用）** | `Lights.change_state(1, { "isOn": false })` `Lights.change_state(2, { "isOn": true })` |
| 🟢 **Tool**  🟢 **工具**                                   | `{ "id": 1, "name": "Table Lamp", "isOn": false, "brightness": 0, "hex": "FFFFFF" }` |
| 🟢 **Tool**  🟢 **工具**                                   | `{ "id": 2, "name": "Ceiling Light", "isOn": true, "brightness": 100, "hex": "FF0000" }` |
| 🔴 **Assistant**  🔴 **助理**                              | The lights have been toggled 灯已切换                        |

 Note  注意

In this example, you also saw parallel function calling. This is where the AI can call multiple functions at the same time. This is a powerful feature that can help the AI solve complex tasks more quickly. It was added to the OpenAI models in 1106.
在此示例中，您还看到了并行函数调用。这是人工智能可以同时调用多个函数的地方。这是一个强大的功能，可以帮助人工智能更快地解决复杂的任务。它于 1106 年被添加到 OpenAI 模型中。



## The automatic planning loop 自动计划循环

Supporting function calling without Semantic Kernel is relatively complex. You would need to write a loop that would accomplish the following:
支持没有Semantic Kernel的函数调用相对复杂。您需要编写一个循环来完成以下作：

1. Create JSON schemas for each of your functions
   为每个函数创建 JSON 架构
2. Provide the LLM with the previous chat history and function schemas
   向 LLM 提供以前的聊天记录和函数模式
3. Parse the LLM's response to determine if it wants to reply with a message or call a function
   解析 LLM 的响应以确定它是否要使用消息回复或调用函数
4. If the LLM wants to call a function, you would need to parse the function name and parameters from the LLM's response
   如果 LLM 想要调用函数，则需要解析 LLM 响应中的函数名称和参数
5. Invoke the function with the right parameters
   使用正确的参数调用函数
6. Return the results of the function so that the LLM can determine what it should do next
   返回函数的结果，以便 LLM 可以确定下一步应该做什么
7. Repeat steps 2-6 until the LLM decides it has completed the task or needs help from the user
   重复步骤 2-6，直到 LLM 确定已完成任务或需要用户帮助

In Semantic Kernel, we make it easy to use function calling by automating this loop for you. This allows you to focus on building the plugins needed to solve your user's needs.
在Semantic Kernel中，我们通过自动执行此循环来简化函数调用。这使您可以专注于构建解决用户需求所需的插件。

 Note  注意

Understanding how the function calling loop works is essential for building performant and reliable AI agents. For an in-depth look at how the loop works, see the [function calling](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/) article.
了解函数调用循环的工作原理对于构建高性能且可靠的 AI 代理至关重要。要深入了解循环的工作原理，请参阅[函数调用](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/)一文。



## Using automatic function calling 使用自动函数调用

To use automatic function calling in Semantic Kernel, you need to do the following:
要在Semantic Kernel中使用自动函数调用，您需要执行以下作：

1. Register the plugin with the kernel
   向内核注册插件
2. Create an execution settings object that tells the AI to automatically call functions
   创建一个执行设置对象，告诉 AI 自动调用函数
3. Invoke the chat completion service with the chat history and the kernel
   使用聊天历史记录和内核调用聊天完成服务

 Tip  提示

The following code sample uses the `LightsPlugin` defined [here](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-native-plugins#defining-a-plugin-using-a-class).
以下代码示例使用[此处](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-native-plugins#defining-a-plugin-using-a-class)定义的 `LightsPlugin`。

C#Copy  复制

```csharp
using System.ComponentModel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

// 1. Create the kernel with the Lights plugin
var builder = Kernel.CreateBuilder().AddAzureOpenAIChatCompletion(modelId, endpoint, apiKey);
builder.Plugins.AddFromType<LightsPlugin>("Lights");
Kernel kernel = builder.Build();

var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// 2. Enable automatic function calling
OpenAIPromptExecutionSettings openAIPromptExecutionSettings = new() 
{
    FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
};

var history = new ChatHistory();

string? userInput;
do {
    // Collect user input
    Console.Write("User > ");
    userInput = Console.ReadLine();

    // Add user input
    history.AddUserMessage(userInput);

    // 3. Get the response from the AI with automatic function calling
    var result = await chatCompletionService.GetChatMessageContentAsync(
        history,
        executionSettings: openAIPromptExecutionSettings,
        kernel: kernel);

    // Print the results
    Console.WriteLine("Assistant > " + result);

    // Add the message from the agent to the chat history
    history.AddMessage(result.Role, result.Content ?? string.Empty);
} while (userInput is not null)
```

When you use automatic function calling, all of the steps in the automatic planning loop are handled for you and added to the `ChatHistory` object. After the function calling loop is complete, you can inspect the `ChatHistory` object to see all of the function calls made and results provided by Semantic Kernel.
使用自动函数调用时，系统会为你处理自动计划循环中的所有步骤，并将其添加到 `ChatHistory` 对象中。函数调用循环完成后，可以检查 `ChatHistory` 对象，查看Semantic Kernel提供的所有函数调用和结果。



## What happened to the Stepwise and Handlebars planners? Stepwise 和 Handlebars 规划器怎么了？

The Stepwise and Handlebars planners have been deprecated and removed from the Semantic Kernel package. These planners are no longer supported in either Python, .NET, or Java.
Stepwise 和 Handlebars 规划器已被弃用，并从 Semantic Kernel 包中删除。Python、.NET 或 Java 不再支持这些规划器。

We recommend using **function calling**, which is both more powerful and easier to use for most scenarios.
建议使用**函数调用** ，对于大多数方案，函数调用功能更强大且更易于使用。

To update existing solutions, follow our [Stepwise Planner Migration Guide](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/stepwise-planner-migration-guide).
要更新现有解决方案，请按照我们的 [Stepwise Planner 迁移指南](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/stepwise-planner-migration-guide)进行作。

 Tip  提示

For new AI agents, use function calling instead of the deprecated planners. It offers better flexibility, built-in tool support, and a simpler development experience.
对于新的 AI 代理，请使用函数调用而不是已弃用的规划器。它提供了更好的灵活性、内置的工具支持和更简单的开发体验。



## Next steps  后续步骤

Now that you understand how planners work in Semantic Kernel, you can learn more about how influence your AI agent so that they best plan and execute tasks on behalf of your users.
现在您已经了解了规划器在Semantic Kernel中的工作原理，您可以详细了解如何影响您的 AI 代理，以便他们代表您的用户最好地计划和执行任务。