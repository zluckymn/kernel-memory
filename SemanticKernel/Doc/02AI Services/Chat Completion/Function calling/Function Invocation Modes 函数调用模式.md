
  询问学习  对焦模式

# Function Invocation Modes 函数调用模式

- 11/23/2024

Choose a programming language


When the AI model receives a prompt containing a list of functions, it may choose one or more of them for invocation to complete the prompt. When a function is chosen by the model, it needs be **invoked** by Semantic Kernel.
当 AI 模型收到包含函数列表的提示时，它可能会选择其中一个或多个函数进行调用以完成提示。当模型选择函数时，需要由语义内**核调用它** 。

The function calling subsystem in Semantic Kernel has two modes of function invocation: **auto** and **manual**.
Kernel中的函数调用子系统有两种函数调用模式： **自动**和**手动** 。

Depending on the invocation mode, Semantic Kernel either does end-to-end function invocation or gives the caller control over the function invocation process.
根据调用模式，Kernel要么执行端到端函数调用，要么让调用方控制函数调用过程。



## Auto Function Invocation  自动函数调用

Auto function invocation is the default mode of the Semantic Kernel function-calling subsystem. When the AI model chooses one or more functions, Semantic Kernel automatically invokes the chosen functions. The results of these function invocations are added to the chat history and sent to the model automatically in subsequent requests. The model then reasons about the chat history, chooses additional functions if needed, or generates the final response. This approach is fully automated and requires no manual intervention from the caller.
自动函数调用是Kernel函数调用子系统的默认模式。当 AI 模型选择一个或多个函数时，Kernel会自动调用所选函数。这些函数调用的结果将添加到聊天历史记录中，并在后续请求中自动发送到模型。然后，模型对聊天记录进行推理，根据需要选择其他功能，或生成最终响应。这种方法是完全自动化的，不需要呼叫者进行手动干预。

 Tip  提示

Auto function invocation is different from the [auto function choice behavior](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors#using-auto-function-choice-behavior). The former dictates if functions should be invoked automatically by Semantic Kernel, while the latter determines if functions should be chosen automatically by the AI model.
自动函数调用与[自动函数选择行为](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors#using-auto-function-choice-behavior)不同。前者决定是否应由Kernel自动调用函数，而后者则决定是否应由 AI 模型自动选择函数。

This example demonstrates how to use the auto function invocation in Semantic Kernel. AI model decides which functions to call to complete the prompt and Semantic Kernel does the rest and invokes them automatically.
此示例演示了如何在Kernel中使用自动函数调用。AI 模型决定调用哪些函数来完成提示，Kernel会完成剩下的工作并自动调用它们。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

// By default, functions are set to be automatically invoked.  
// If you want to explicitly enable this behavior, you can do so with the following code:  
// PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(autoInvoke: true) };  
PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }; 

await kernel.InvokePromptAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(settings));
```

Some AI models support parallel function calling, where the model chooses multiple functions for invocation. This can be useful in cases when invoking chosen functions takes a long time. For example, the AI may choose to retrieve the latest news and the current time simultaneously, rather than making a round trip per function.
一些 AI 模型支持并行函数调用，其中模型选择多个函数进行调用。这在调用所选函数需要很长时间的情况下非常有用。例如，人工智能可能会选择同时检索最新新闻和当前时间，而不是按函数进行往返。

Semantic Kernel can invoke these functions in two different ways:
Kernel可以通过两种不同的方式调用这些函数：

- **Sequentially**: The functions are invoked one after another. This is the default behavior.
  按**顺序：** 函数被逐个调用。这是默认行为。
- **Concurrently**: The functions are invoked at the same time. This can be enabled by setting the `FunctionChoiceBehaviorOptions.AllowConcurrentInvocation` property to `true`, as shown in the example below.
  **同时** ：同时调用函数。这可以通过将 `FunctionChoiceBehaviorOptions.AllowConcurrentInvocation` 属性设置为 `true` 来启用，如下面的示例所示。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<NewsUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

// Enable concurrent invocation of functions to get the latest news and the current time.
FunctionChoiceBehaviorOptions options = new() { AllowConcurrentInvocation = true };

PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(options: options) }; 

await kernel.InvokePromptAsync("Good morning! What is the current time and latest news headlines?", new(settings));
```



## Manual Function Invocation 手动函数调用

In cases when the caller wants to have more control over the function invocation process, manual function invocation can be used.
如果调用方希望更好地控制函数调用过程，可以使用手动函数调用。

When manual function invocation is enabled, Semantic Kernel does not automatically invoke the functions chosen by the AI model. Instead, it returns a list of chosen functions to the caller, who can then decide which functions to invoke, invoke them sequentially or in parallel, handle exceptions, and so on. The function invocation results need to be added to the chat history and returned to the model, which will reason about them and decide whether to choose additional functions or generate a final response.
启用手动函数调用后，Kernel不会自动调用 AI 模型选择的函数。相反，它会向调用方返回所选函数的列表，然后调用方可以决定调用哪些函数、按顺序或并行调用它们、处理异常等。函数调用结果需要添加到聊天记录中并返回给模型，模型会对其进行推理并决定是选择其他函数还是生成最终响应。

The example below demonstrates how to use manual function invocation.
下面的示例演示了如何使用手动函数调用。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Manual function invocation needs to be enabled explicitly by setting autoInvoke to false.
PromptExecutionSettings settings = new() { FunctionChoiceBehavior = Microsoft.SemanticKernel.FunctionChoiceBehavior.Auto(autoInvoke: false) };

ChatHistory chatHistory = [];
chatHistory.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

while (true)
{
    ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, settings, kernel);

    // Check if the AI model has generated a response.
    if (result.Content is not null)
    {
        Console.Write(result.Content);
        // Sample output: "Considering the current weather conditions in Boston with a tornado watch in effect resulting in potential severe thunderstorms,
        // the sky color is likely unusual such as green, yellow, or dark gray. Please stay safe and follow instructions from local authorities."
        break;
    }

    // Adding AI model response containing chosen functions to chat history as it's required by the models to preserve the context.
    chatHistory.Add(result); 

    // Check if the AI model has chosen any function for invocation.
    IEnumerable<FunctionCallContent> functionCalls = FunctionCallContent.GetFunctionCalls(result);
    if (!functionCalls.Any())
    {
        break;
    }

    // Sequentially iterating over each chosen function, invoke it, and add the result to the chat history.
    foreach (FunctionCallContent functionCall in functionCalls)
    {
        try
        {
            // Invoking the function
            FunctionResultContent resultContent = await functionCall.InvokeAsync(kernel);

            // Adding the function result to the chat history
            chatHistory.Add(resultContent.ToChatMessage());
        }
        catch (Exception ex)
        {
            // Adding function exception to the chat history.
            chatHistory.Add(new FunctionResultContent(functionCall, ex).ToChatMessage());
            // or
            //chatHistory.Add(new FunctionResultContent(functionCall, "Error details that the AI model can reason about.").ToChatMessage());
        }
    }
}
```

 Note  注意

The FunctionCallContent and FunctionResultContent classes are used to represent AI model function calls and Semantic Kernel function invocation results, respectively. They contain information about chosen function, such as the function ID, name, and arguments, and function invocation results, such as function call ID and result.
FunctionCallContent 和 FunctionResultContent 类分别用于表示 AI 模型函数调用和Kernel函数调用结果。它们包含有关所选函数的信息，例如函数 ID、名称和参数，以及函数调用结果，例如函数调用 ID 和结果。

The following example demonstrates how to use manual function invocation with the streaming chat completion API. Note the usage of the `FunctionCallContentBuilder` class to build function calls from the streaming content. Due to the streaming nature of the API, function calls are also streamed. This means that the caller must build the function calls from the streaming content before invoking them.
以下示例演示了如何将手动函数调用与流式聊天完成 API 一起使用。请注意使用 `FunctionCallContentBuilder` 类从流式处理内容生成函数调用。由于 API 的流式处理性质，函数调用也会流式传输。这意味着调用方必须先从流式处理内容生成函数调用，然后才能调用它们。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Manual function invocation needs to be enabled explicitly by setting autoInvoke to false.
PromptExecutionSettings settings = new() { FunctionChoiceBehavior = Microsoft.SemanticKernel.FunctionChoiceBehavior.Auto(autoInvoke: false) };

ChatHistory chatHistory = [];
chatHistory.AddUserMessage("Given the current time of day and weather, what is the likely color of the sky in Boston?");

while (true)
{
    AuthorRole? authorRole = null;
    FunctionCallContentBuilder fccBuilder = new ();

    // Start or continue streaming chat based on the chat history
    await foreach (StreamingChatMessageContent streamingContent in chatCompletionService.GetStreamingChatMessageContentsAsync(chatHistory, settings, kernel))
    {
        // Check if the AI model has generated a response.
        if (streamingContent.Content is not null)
        {
            Console.Write(streamingContent.Content);
            // Sample streamed output: "The color of the sky in Boston is likely to be gray due to the rainy weather."
        }
        authorRole ??= streamingContent.Role;

        // Collect function calls details from the streaming content
        fccBuilder.Append(streamingContent);
    }

    // Build the function calls from the streaming content and quit the chat loop if no function calls are found
    IReadOnlyList<FunctionCallContent> functionCalls = fccBuilder.Build();
    if (!functionCalls.Any())
    {
        break;
    }

    // Creating and adding chat message content to preserve the original function calls in the chat history.
    // The function calls are added to the chat message a few lines below.
    ChatMessageContent fcContent = new ChatMessageContent(role: authorRole ?? default, content: null);
    chatHistory.Add(fcContent);

    // Iterating over the requested function calls and invoking them.
    // The code can easily be modified to invoke functions concurrently if needed.
    foreach (FunctionCallContent functionCall in functionCalls)
    {
        // Adding the original function call to the chat message content
        fcContent.Items.Add(functionCall);

        // Invoking the function
        FunctionResultContent functionResult = await functionCall.InvokeAsync(kernel);

        // Adding the function result to the chat history
        chatHistory.Add(functionResult.ToChatMessage());
    }
}
```