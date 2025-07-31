# What are Filters?  什么是过滤器？

- 02/19/2025



Filters enhance security by providing control and visibility over how and when functions run. This is needed to instill responsible AI principles into your work so that you feel confident your solution is enterprise ready.
过滤器通过提供对函数运行方式和时间的控制和可见性来增强安全性。这是将负责任的 AI 原则灌输到您的工作中所必需的，以便您确信您的解决方案已为企业做好准备。

For example, filters are leveraged to validate permissions before an approval flow begins. The filter runs to check the permissions of the person that’s looking to submit an approval. This means that only a select group of people will be able to kick off the process.
例如，在审批流开始之前，利用筛选器来验证权限。运行筛选器以检查要提交审批的人员的权限。这意味着只有选定的一组人才能启动该过程。

A good example of filters is provided [here](https://devblogs.microsoft.com/semantic-kernel/filters-in-semantic-kernel/) in our detailed Semantic Kernel blog post on Filters.  
我们关于过滤器的详细Semantic Kernel博客[文章中提供了](https://devblogs.microsoft.com/semantic-kernel/filters-in-semantic-kernel/)一个很好的过滤器示例。 ![Semantic Kernel Filters](https://learn.microsoft.com/en-us/semantic-kernel/media/whatarefilters.png)

There are three types of filters:
筛选器分为三种类型：

- **Function Invocation Filter** - this filter is executed each time a `KernelFunction` is invoked. It allows:
  **函数调用过滤器** - 每次调用 `KernelFunction` 时都会执行此过滤器。它允许：
  - Access to information about the function being executed and its arguments
    访问有关正在执行的函数及其参数的信息
  - Handling of exceptions during function execution
    函数执行期间异常的处理
  - Overriding of the function result, either before (for instance for caching scenario's) or after execution (for instance for responsible AI scenarios)
    重写函数结果，在执行之前（例如，对于缓存方案）或执行之后（例如，对于负责任的 AI 方案）
  - Retrying of the function in case of failure (e.g., [switching to an alternative AI model](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/RetryWithFilters.cs))
    在失败时重试函数（例如，[ 切换到替代 AI 模型 ](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/RetryWithFilters.cs)）
- **Prompt Render Filter** - this filter is triggered before the prompt rendering operation, enabling:
  **提示渲染过滤器** - 此过滤器在提示渲染作之前触发，从而启用：
  - Viewing and modifying the prompt that will be sent to the AI (e.g., for RAG or [PII redaction](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/PIIDetection.cs))
    查看和修改将发送给 AI 的提示（例如，用于 RAG 或 [PII 编辑 ](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/PIIDetection.cs)）
  - Preventing prompt submission to the AI by overriding the function result (e.g., for [Semantic Caching](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Caching/SemanticCachingWithFilters.cs))
    通过覆盖函数结果来防止提示提交给 AI（例如，用于[语义缓存 ](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Caching/SemanticCachingWithFilters.cs)）

- **Auto Function Invocation Filter** - similar to the function invocation filter, this filter operates within the scope of `automatic function calling`, providing additional context, including chat history, a list of all functions to be executed, and iteration counters. It also allows termination of the auto function calling process (e.g., if a desired result is obtained from the second of three planned functions).
  **自动函数调用过滤器** - 与函数调用过滤器类似，此过滤器在`自动函数调用`范围内运行，提供额外的上下文，包括聊天历史记录、要执行的所有函数的列表以及迭代计数器。它还允许终止自动函数调用过程（例如，如果从三个计划函数中的第二个获得所需的结果）。

Each filter includes a `context` object that contains all relevant information about the function execution or prompt rendering. Additionally, each filter has a `next` delegate/callback to execute the next filter in the pipeline or the function itself, offering control over function execution (e.g., in cases of malicious prompts or arguments). Multiple filters of the same type can be registered, each with its own responsibility.
每个过滤器都包含一个`上下文`对象，其中包含有关函数执行或提示呈现的所有相关信息。此外，每个过滤器都有一个下`一个`委托/回调，用于执行管道或函数本身中的下一个过滤器，从而提供对函数执行的控制（例如，在恶意提示或参数的情况下）。可以注册多个相同类型的过滤器，每个过滤器都有自己的责任。

In a filter, calling the `next` delegate is essential to proceed to the next registered filter or the original operation (whether function invocation or prompt rendering). Without calling `next`, the operation will not be executed.
在筛选器中，调用`下一个`委托对于继续执行下一个已注册的筛选器或原始作（无论是函数调用还是提示呈现）至关重要。如果不调用 `next`，则不会执行该作。

To use a filter, first define it, then add it to the `Kernel` object either through dependency injection or the appropriate `Kernel` property. When using dependency injection, the order of filters is not guaranteed, so with multiple filters, the execution order may be unpredictable.
要使用过滤器，请先定义它，然后通过依赖注入或相应的 `Kernel` 属性将其添加到 `Kernel` 对象。使用依赖注入时，无法保证过滤器的顺序，因此使用多个过滤器时，执行顺序可能无法预测。



## Function Invocation Filter 函数调用过滤器

This filter is triggered every time a Semantic Kernel function is invoked, regardless of whether it is a function created from a prompt or a method.
每次调用Semantic Kernel函数时都会触发此过滤器，无论它是从提示还是方法创建的函数。

C#Copy  复制

```csharp
/// <summary>
/// Example of function invocation filter to perform logging before and after function invocation.
/// </summary>
public sealed class LoggingFilter(ILogger logger) : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        logger.LogInformation("FunctionInvoking - {PluginName}.{FunctionName}", context.Function.PluginName, context.Function.Name);

        await next(context);

        logger.LogInformation("FunctionInvoked - {PluginName}.{FunctionName}", context.Function.PluginName, context.Function.Name);
    }
}
```

Add filter using dependency injection:
使用依赖注入添加过滤器：

C#Copy  复制

```csharp
IKernelBuilder builder = Kernel.CreateBuilder();

builder.Services.AddSingleton<IFunctionInvocationFilter, LoggingFilter>();

Kernel kernel = builder.Build();
```

Add filter using `Kernel` property:
使用 `Kernel` 属性添加过滤器：

C#Copy  复制

```csharp
kernel.FunctionInvocationFilters.Add(new LoggingFilter(logger));
```



### Code examples  代码示例

- [Function invocation filter examples
  函数调用过滤器示例](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/FunctionInvocationFiltering.cs)



## Prompt Render Filter  提示渲染过滤器

This filter is invoked only during a prompt rendering operation, such as when a function created from a prompt is called. It will not be triggered for Semantic Kernel functions created from methods.
仅在提示呈现作期间调用此筛选器，例如，当调用从提示创建的函数时。对于从方法创建的Semantic Kernel函数，它不会被触发。

C#Copy  复制

```csharp
/// <summary>
/// Example of prompt render filter which overrides rendered prompt before sending it to AI.
/// </summary>
public class SafePromptFilter : IPromptRenderFilter
{
    public async Task OnPromptRenderAsync(PromptRenderContext context, Func<PromptRenderContext, Task> next)
    {
        // Example: get function information
        var functionName = context.Function.Name;

        await next(context);

        // Example: override rendered prompt before sending it to AI
        context.RenderedPrompt = "Safe prompt";
    }
}
```

Add filter using dependency injection:
使用依赖注入添加过滤器：

C#Copy  复制

```csharp
IKernelBuilder builder = Kernel.CreateBuilder();

builder.Services.AddSingleton<IPromptRenderFilter, SafePromptFilter>();

Kernel kernel = builder.Build();
```

Add filter using `Kernel` property:
使用 `Kernel` 属性添加过滤器：

C#Copy  复制

```csharp
kernel.PromptRenderFilters.Add(new SafePromptFilter());
```



### Code examples  代码示例

- [Prompt render filter examples
  提示渲染过滤器示例](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/PromptRenderFiltering.cs)



## Auto Function Invocation Filter 自动函数调用过滤器

This filter is invoked only during an automatic function calling process. It will not be triggered when a function is invoked outside of this process.
此筛选器仅在自动函数调用过程中调用。在此进程之外调用函数时，不会触发它。

C#Copy  复制

```csharp
/// <summary>
/// Example of auto function invocation filter which terminates function calling process as soon as we have the desired result.
/// </summary>
public sealed class EarlyTerminationFilter : IAutoFunctionInvocationFilter
{
    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, Func<AutoFunctionInvocationContext, Task> next)
    {
        // Call the function first.
        await next(context);

        // Get a function result from context.
        var result = context.Result.GetValue<string>();

        // If the result meets the condition, terminate the process.
        // Otherwise, the function calling process will continue.
        if (result == "desired result")
        {
            context.Terminate = true;
        }
    }
}
```

Add filter using dependency injection:
使用依赖注入添加过滤器：

C#Copy  复制

```csharp
IKernelBuilder builder = Kernel.CreateBuilder();

builder.Services.AddSingleton<IAutoFunctionInvocationFilter, EarlyTerminationFilter>();

Kernel kernel = builder.Build();
```

Add filter using `Kernel` property:
使用 `Kernel` 属性添加过滤器：

C#Copy  复制

```csharp
kernel.AutoFunctionInvocationFilters.Add(new EarlyTerminationFilter());
```



### Code examples  代码示例

- [Auto function invocation filter examples
  自动函数调用筛选器示例](https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Filtering/AutoFunctionInvocationFiltering.cs)



## Streaming and non-streaming invocation 流式和非流式调用

Functions in Semantic Kernel can be invoked in two ways: streaming and non-streaming. In streaming mode, a function typically returns `IAsyncEnumerable<T>`, while in non-streaming mode, it returns `FunctionResult`. This distinction affects how results can be overridden in the filter: in streaming mode, the new function result value must be of type `IAsyncEnumerable<T>`, whereas in non-streaming mode, it can simply be of type `T`. To determine which result type needs to be returned, the `context.IsStreaming` flag is available in the filter context model.
Semantic Kernel中的函数可以通过两种方式调用：流式和非流式。在流式处理模式下，函数通常返回 `IAsyncEnumerable<T>`，而在非流式处理模式下，它返回 `FunctionResult`。此区别会影响在筛选器中重写结果的方式：在流式处理模式下，新函数结果值必须是 `IAsyncEnumerable<T>` 类型，而在非流式处理模式下，它可以只是 `T` 类型。若要确定需要返回哪种结果类型，上下文 `。IsStreaming` 标志在筛选器上下文模型中可用。

C#Copy  复制

```csharp
/// <summary>Filter that can be used for both streaming and non-streaming invocation modes at the same time.</summary>
public sealed class DualModeFilter : IFunctionInvocationFilter
{
    public async Task OnFunctionInvocationAsync(FunctionInvocationContext context, Func<FunctionInvocationContext, Task> next)
    {
        // Call next filter in pipeline or actual function.
        await next(context);

        // Check which function invocation mode is used.
        if (context.IsStreaming)
        {
            // Return IAsyncEnumerable<string> result in case of streaming mode.
            var enumerable = context.Result.GetValue<IAsyncEnumerable<string>>();
            context.Result = new FunctionResult(context.Result, OverrideStreamingDataAsync(enumerable!));
        }
        else
        {
            // Return just a string result in case of non-streaming mode.
            var data = context.Result.GetValue<string>();
            context.Result = new FunctionResult(context.Result, OverrideNonStreamingData(data!));
        }
    }

    private async IAsyncEnumerable<string> OverrideStreamingDataAsync(IAsyncEnumerable<string> data)
    {
        await foreach (var item in data)
        {
            yield return $"{item} - updated from filter";
        }
    }

    private string OverrideNonStreamingData(string data)
    {
        return $"{data} - updated from filter";
    }
}
```



## Using filters with `IChatCompletionService` 将筛选器与 `IChatCompletionService` 一起使用

In cases where `IChatCompletionService` is used directly instead of `Kernel`, filters will only be invoked when a `Kernel` object is passed as a parameter to the chat completion service methods, as filters are attached to the `Kernel` instance.
如果直接使用 `IChatCompletionService` 而不是`内核 `，则仅当内`核`对象作为参数传递给聊天完成服务方法时，才会调用筛选器，因为筛选器已附加到`内核`实例。

C#Copy  复制

```csharp
Kernel kernel = Kernel.CreateBuilder()
    .AddOpenAIChatCompletion("gpt-4", "api-key")
    .Build();

kernel.FunctionInvocationFilters.Add(new MyFilter());

IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();

// Passing a Kernel here is required to trigger filters.
ChatMessageContent result = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings, kernel);
```



## Ordering  订购

When using dependency injection, the order of filters is not guaranteed. If the order of filters is important, it is recommended to add filters directly to the `Kernel` object using appropriate properties. This approach allows filters to be added, removed, or reordered at runtime.
使用依赖注入时，不保证筛选器的顺序。如果筛选器的顺序很重要，建议使用适当的属性将筛选器直接添加到 `Kernel` 对象。此方法允许在运行时添加、删除或重新排序过滤器。