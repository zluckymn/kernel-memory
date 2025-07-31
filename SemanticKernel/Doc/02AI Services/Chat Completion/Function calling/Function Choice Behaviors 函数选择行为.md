# Function Choice Behaviors 函数选择行为

- 05/07/2025

Function choice behaviors are bits of configuration that allows a developer to configure:
函数选择行为是允许开发人员配置的配置位：

1. Which functions are advertised to AI models.
   哪些功能被通告给 AI 模型。
2. How the models should choose them for invocation.
   模型应如何选择它们进行调用。
3. How Semantic Kernel might invoke those functions.
   Kernel如何调用这些函数。

As of today, the function choice behaviors are represented by three static methods of the `FunctionChoiceBehavior` class:
截至目前，函数选择行为由 `FunctionChoiceBehavior` 类的三个静态方法表示：

- **Auto**: Allows the AI model to choose from zero or more function(s) from the provided function(s) for invocation.
  **自动** ：允许 AI 模型从提供的函数中选择零个或多个函数进行调用。
- **Required**: Forces the AI model to choose one or more function(s) from the provided function(s) for invocation.
  **必需** ：强制 AI 模型从提供的函数中选择一个或多个函数进行调用。
- **None**: Instructs the AI model not to choose any function(s).
  **无** ：指示 AI 模型不要选择任何函数。

 Note  注意

If your code uses the function-calling capabilities represented by the ToolCallBehavior class, please refer to the [migration guide](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/function-calling-migration-guide) to update the code to the latest function-calling model.
如果您的代码使用 ToolCallBehavior 类表示的函数调用能力，请参考[迁移指南](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/function-calling-migration-guide)将代码更新到最新的函数调用模型。

 Note  注意

The function-calling capabilities is only supported by a few AI connectors so far, see the [Supported AI Connectors](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors#supported-ai-connectors) section below for more details.
到目前为止，只有少数 AI 连接器支持函数调用功能，有关更多详细信息，请参阅下面的支持的 [AI 连接器](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-choice-behaviors#supported-ai-connectors)部分。



## Function Advertising  功能广告

Function advertising is the process of providing functions to AI models for further calling and invocation. All three function choice behaviors accept a list of functions to advertise as a `functions` parameter. By default, it is null, which means all functions from plugins registered on the Kernel are provided to the AI model.
函数广告是向 AI 模型提供函数以进行进一步调用和调用的过程。所有三个函数选择行为都接受要作为`函数`参数通告的函数列表。默认情况下，它为空，这意味着在内核上注册的插件中的所有函数都提供给 AI 模型。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

// All functions from the DateTimeUtils and WeatherForecastUtils plugins will be sent to AI model together with the prompt.
PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }; 

await kernel.InvokePromptAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(settings));
```

If a list of functions is provided, only those functions are sent to the AI model:
如果提供了函数列表，则仅将这些函数发送到 AI 模型：

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

KernelFunction getWeatherForCity = kernel.Plugins.GetFunction("WeatherForecastUtils", "GetWeatherForCity");
KernelFunction getCurrentTime = kernel.Plugins.GetFunction("DateTimeUtils", "GetCurrentUtcDateTime");

// Only the specified getWeatherForCity and getCurrentTime functions will be sent to AI model alongside the prompt.
PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: [getWeatherForCity, getCurrentTime]) }; 

await kernel.InvokePromptAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(settings));
```

An empty list of functions means no functions are provided to the AI model, which is equivalent to disabling function calling.
函数列表为空意味着没有向 AI 模型提供任何函数，这相当于禁用函数调用。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

// Disables function calling. Equivalent to var settings = new() { FunctionChoiceBehavior = null } or var settings = new() { }.
PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(functions: []) }; 

await kernel.InvokePromptAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(settings));
```



## Using Auto Function Choice Behavior 使用自动函数选择行为

The `Auto` function choice behavior instructs the AI model to choose from zero or more function(s) from the provided function(s) for invocation.
`自动`函数选择行为指示 AI 模型从提供的函数中选择零个或多个函数进行调用。

In this example, all functions from the `DateTimeUtils` and `WeatherForecastUtils` plugins will be provided to the AI model alongside the prompt. The model will first choose `GetCurrentTime` function for invocation to obtain the current date and time, as this information is needed as input for the `GetWeatherForCity` function. Next, it will choose `GetWeatherForCity` function for invocation to get the weather forecast for the city of Boston using the obtained date and time. With this information, the model will be able to determine the likely color of the sky in Boston.
在此示例中，`DateTimeUtils` 和 `WeatherForecastUtils` 插件中的所有函数都将与提示一起提供给 AI 模型。模型将首先选择 `GetCurrentTime` 函数进行调用以获取当前日期和时间，因为需要此信息作为 `GetWeatherForCity` 函数的输入。接下来，它将选择 `GetWeatherForCity` 函数进行调用，以使用获取的日期和时间获取波士顿市的天气预报。有了这些信息，模型将能够确定波士顿天空的可能颜色。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

// All functions from the DateTimeUtils and WeatherForecastUtils plugins will be provided to AI model alongside the prompt.
PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() }; 

await kernel.InvokePromptAsync("Given the current time of day and weather, what is the likely color of the sky in Boston?", new(settings));
```

The same example can be easily modeled in a YAML prompt template configuration:
可以在 YAML 提示模板配置中轻松建模相同的示例：

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

string promptTemplateConfig = """
    template_format: semantic-kernel
    template: Given the current time of day and weather, what is the likely color of the sky in Boston?
    execution_settings:
      default:
        function_choice_behavior:
          type: auto
    """;

KernelFunction promptFunction = KernelFunctionYaml.FromPromptYaml(promptTemplateConfig);

Console.WriteLine(await kernel.InvokeAsync(promptFunction));
```



## Using Required Function Choice Behavior 使用所需的函数选择行为

The `Required` behavior forces the model to choose one or more function(s) from the provided function(s) for invocation. This is useful for scenarios when the AI model must obtain required information from the specified functions rather than from it's own knowledge.
`Required` 行为强制模型从提供的函数中选择一个或多个函数进行调用。这对于 AI 模型必须从指定函数而不是从自己的知识中获取所需信息的场景非常有用。

 Note  注意

The behavior advertises functions in the first request to the AI model only and stops sending them in subsequent requests to prevent an infinite loop where the model keeps choosing the same functions for invocation repeatedly.
该行为仅在第一个请求中向 AI 模型通告函数，并在后续请求中停止发送它们，以防止模型不断重复选择相同的函数进行调用的无限循环。

Here, we specify that the AI model must choose the `GetWeatherForCity` function for invocation to obtain the weather forecast for the city of Boston, rather than guessing it based on its own knowledge. The model will first choose the `GetWeatherForCity` function for invocation to retrieve the weather forecast. With this information, the model can then determine the likely color of the sky in Boston using the response from the call to `GetWeatherForCity`.
在这里，我们指定 AI 模型必须选择 `GetWeatherForCity` 函数进行调用，以获取波士顿市的天气预报，而不是根据自己的知识进行猜测。模型将首先选择 `GetWeatherForCity` 函数进行调用以检索天气预报。有了这些信息，模型就可以使用对 `GetWeatherForCity` 的调用的响应来确定波士顿天空的可能颜色。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();

Kernel kernel = builder.Build();

KernelFunction getWeatherForCity = kernel.Plugins.GetFunction("WeatherForecastUtils", "GetWeatherForCity");

PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Required(functions: [getWeatherFunction]) };

await kernel.InvokePromptAsync("Given that it is now the 10th of September 2024, 11:29 AM, what is the likely color of the sky in Boston?", new(settings));
```

An identical example in a YAML template configuration:
YAML 模板配置中的相同示例：

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();

Kernel kernel = builder.Build();

string promptTemplateConfig = """
    template_format: semantic-kernel
    template: Given that it is now the 10th of September 2024, 11:29 AM, what is the likely color of the sky in Boston?
    execution_settings:
      default:
        function_choice_behavior:
          type: required
          functions:
            - WeatherForecastUtils.GetWeatherForCity
    """;

KernelFunction promptFunction = KernelFunctionYaml.FromPromptYaml(promptTemplateConfig);

Console.WriteLine(await kernel.InvokeAsync(promptFunction));
```

Alternatively, all functions registered in the kernel can be provided to the AI model as required. However, only the ones chosen by the AI model as a result of the first request will be invoked by the Semantic Kernel. The functions will not be sent to the AI model in subsequent requests to prevent an infinite loop, as mentioned above.
或者，可以根据需要将内核中注册的所有功能提供给 AI 模型。但是，只有 AI 模型根据第一个请求选择的那些才会被Kernel调用。如上所述，这些函数不会在后续请求中发送给 AI 模型，以防止无限循环。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();

Kernel kernel = builder.Build();

PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.Required() };

await kernel.InvokePromptAsync("Given that it is now the 10th of September 2024, 11:29 AM, what is the likely color of the sky in Boston?", new(settings));
```



## Using None Function Choice Behavior 使用 None 函数选择行为

The `None` behavior instructs the AI model to use the provided function(s) without choosing any of them for invocation and instead generate a message response. This is useful for dry runs when the caller may want to see which functions the model would choose without actually invoking them. For instance in the sample below the AI model correctly lists the functions it would choose to determine the color of the sky in Boston.
`None` 行为指示 AI 模型使用提供的函数，而不选择任何函数进行调用，而是生成消息响应。当调用方可能希望查看模型将选择哪些函数而不实际调用它们时，这对于试运行非常有用。例如，在下面的示例中，AI 模型正确列出了它将选择的函数来确定波士顿的天空颜色。

C#Copy  复制

~~~csharp
Here, we advertise all functions from the `DateTimeUtils` and `WeatherForecastUtils` plugins to the AI model but instruct it not to choose any of them.
Instead, the model will provide a response describing which functions it would choose to determine the color of the sky in Boston on a specified date.

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

KernelFunction getWeatherForCity = kernel.Plugins.GetFunction("WeatherForecastUtils", "GetWeatherForCity");

PromptExecutionSettings settings = new() { FunctionChoiceBehavior = FunctionChoiceBehavior.None() };

await kernel.InvokePromptAsync("Specify which provided functions are needed to determine the color of the sky in Boston on a specified date.", new(settings))

// Sample response: To determine the color of the sky in Boston on a specified date, first call the DateTimeUtils-GetCurrentUtcDateTime function to obtain the 
// current date and time in UTC. Next, use the WeatherForecastUtils-GetWeatherForCity function, providing 'Boston' as the city name and the retrieved UTC date and time. 
// These functions do not directly provide the sky's color, but the GetWeatherForCity function offers weather data, which can be used to infer the general sky condition (e.g., clear, cloudy, rainy).
~~~

A corresponding example in a YAML prompt template configuration:
YAML 提示模板配置中的相应示例：

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder builder = Kernel.CreateBuilder(); 
builder.AddOpenAIChatCompletion("<model-id>", "<api-key>");
builder.Plugins.AddFromType<WeatherForecastUtils>();
builder.Plugins.AddFromType<DateTimeUtils>(); 

Kernel kernel = builder.Build();

string promptTemplateConfig = """
    template_format: semantic-kernel
    template: Specify which provided functions are needed to determine the color of the sky in Boston on a specified date.
    execution_settings:
      default:
        function_choice_behavior:
          type: none
    """;

KernelFunction promptFunction = KernelFunctionYaml.FromPromptYaml(promptTemplateConfig);

Console.WriteLine(await kernel.InvokeAsync(promptFunction));
```



## Function Choice Behavior Options 函数选择行为选项

Certain aspects of the function choice behaviors can be configured through options that each function choice behavior class accepts via the `options` constructor parameter of the `FunctionChoiceBehaviorOptions` type. The following options are available:
函数选择行为的某些方面可以通过每个函数选择行为类通过 `FunctionChoiceBehaviorOptions` 类型的 `options` 构造函数参数接受的选项来配置。以下选项可用：

- **AllowConcurrentInvocation**: This option enables the concurrent invocation of functions by the Semantic Kernel. By default, it is set to false, meaning that functions are invoked sequentially. Concurrent invocation is only possible if the AI model can choose multiple functions for invocation in a single request; otherwise, there is no distinction between sequential and concurrent invocation
  **AllowConcurrentInvocation**：此选项允许Kernel并发调用函数。默认情况下，它设置为 false，这意味着按顺序调用函数。只有当 AI 模型可以在单个请求中选择多个函数进行调用时，才有可能并发调用;否则，顺序调用和并发调用之间没有区别

- **AllowParallelCalls**: This option allows the AI model to choose multiple functions in one request. Some AI models may not support this feature; in such cases, the option will have no effect. By default, this option is set to null, indicating that the AI model's default behavior will be used.
  **AllowParallelCalls**：此选项允许 AI 模型在一个请求中选择多个函数。某些 AI 模型可能不支持此功能;在这种情况下，该选项将无效。默认情况下，此选项设置为 null，表示将使用 AI 模型的默认行为。

  Copy  复制

  ```
  The following table summarizes the effects of various combinations of the AllowParallelCalls and AllowConcurrentInvocation options:
  
  | AllowParallelCalls  | AllowConcurrentInvocation | # of functions chosen per AI roundtrip  | Concurrent Invocation by SK |
  |---------------------|---------------------------|-----------------------------------------|-----------------------|
  | false               | false                     | one                                     | false                 |
  | false               | true                      | one                                     | false*                |
  | true                | false                     | multiple                                | false                 |
  | true                | true                      | multiple                                | true                  |
  
  `*` There's only one function to invoke
  ```



## Function Invocation  函数调用

Function invocation is the process whereby Semantic Kernel invokes functions chosen by the AI model. For more details on function invocation see [function invocation article](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-invocation).
函数调用是Kernel调用 AI 模型选择的函数的过程。有关函数调用的更多详细信息，请参阅[函数调用一文 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/function-calling/function-invocation)。



## Supported AI Connectors  支持的 AI 连接器

As of today, the following AI connectors in Semantic Kernel support the function calling model:
截至目前，Kernel中的以下 AI 连接器支持函数调用模型：

  展开表

| AI Connector  AI 连接器 | FunctionChoiceBehavior  函数选择行为 | ToolCallBehavior  工具调用行为 |
| :---------------------- | :----------------------------------- | :----------------------------- |
| Anthropic  人为的       | Planned  计划                        | ❌                              |
| AzureAIInference        | Coming soon  即将推出                | ❌                              |
| AzureOpenAI             | ✔️                                    | ✔️                              |
| Gemini  双子座          | Planned  计划                        | ✔️                              |
| HuggingFace  拥抱脸     | Planned  计划                        | ❌                              |
| Mistral  米斯特拉尔     | Planned  计划                        | ✔️                              |
| Ollama  奥拉玛          | Coming soon  即将推出                | ❌                              |
| Onnx  安克斯            | Coming soon  即将推出                | ❌                              |
| OpenAI  开放人工智能    | ✔️                                    | ✔️                              |