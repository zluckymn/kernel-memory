# How-To: Create your first Process 作方法：创建您的第一个流程

- 02/26/2025



The *Semantic Kernel Process Framework* is experimental, still in development and is subject to change.
*Semantic Kernel进程框架*是实验性的，仍在开发中，可能会发生变化。



## Overview  概述

The Semantic Kernel Process Framework is a powerful orchestration SDK designed to simplify the development and execution of AI-integrated processes. Whether you are managing simple workflows or complex systems, this framework allows you to define a series of steps that can be executed in a structured manner, enhancing your application's capabilities with ease and flexibility.
Semantic Kernel进程框架是一个功能强大的编排 SDK，旨在简化 AI 集成流程的开发和执行。无论您是管理简单的工作流程还是复杂的系统，该框架都允许您定义一系列可以以结构化方式执行的步骤，从而轻松灵活地增强应用程序的功能。

Built for extensibility, the Process Framework supports diverse operational patterns such as sequential execution, parallel processing, fan-in and fan-out configurations, and even map-reduce strategies. This adaptability makes it suitable for a variety of real-world applications, particularly those that require intelligent decision-making and multi-step workflows.
流程框架专为可扩展性而构建，支持多种作模式，例如顺序执行、并行处理、扇入和扇出配置，甚至映射缩减策略。这种适应性使其适用于各种实际应用，特别是那些需要智能决策和多步骤工作流程的应用。



## Getting Started  开始

The Semantic Kernel Process Framework can be used to infuse AI into just about any business process you can think of. As an illustrative example to get started, let's look at building a process to generate documentation for a new product.
Semantic Kernel流程框架可用于将 AI 注入您能想到的几乎任何业务流程中。作为入门的说明性示例，让我们看看构建一个流程来为新产品生成文档。

Before we get started, make sure you have the required Semantic Kernel packages installed:
在开始之前，请确保您已安装所需的Semantic Kernel包：

.NET CLICopy  复制

```dotnetcli
// Install the Semantic Kernel Process Framework Local Runtime package
dotnet add package Microsoft.SemanticKernel.Process.LocalRuntime --version 1.46.0-alpha
// or
// Install the Semantic Kernel Process Framework Dapr Runtime package
dotnet add package Microsoft.SemanticKernel.Process.Runtime.Dapr --version 1.46.0-alpha
```



## Illustrative Example: Generating Documentation for a New Product 说明性示例：为新产品生成文档

In this example, we will utilize the Semantic Kernel Process Framework to develop an automated process for creating documentation for a new product. This process will start out simple and evolve as we go to cover more realistic scenarios.
在此示例中，我们将利用Semantic Kernel流程框架来开发一个自动化流程，用于为新产品创建文档。这个过程将从简单开始，随着我们涵盖更现实的场景而不断发展。

We will start by modeling the documentation process with a very basic flow:
我们将首先使用一个非常基本的流程对文档过程进行建模：

1. `GatherProductInfoStep`: Gather information about the product.
   `GatherProductInfoStep`：收集有关产品的信息。
2. `GenerateDocumentationStep`: Ask an LLM to generate documentation from the information gathered in step 1.
   `GenerateDocumentationStep`：要求 LLM 根据步骤 1 中收集的信息生成文档。
3. `PublishDocumentationStep`: Publish the documentation.
   `PublishDocumentationStep`：发布文档。

![Flow diagram of our first process: A[Request Feature Documentation] --> B[Ask LLM To Write Documentation] --> C[Publish Documentation To Public]](https://learn.microsoft.com/en-us/semantic-kernel/media/first-process-flow.png)

Now that we understand our processes, let's build it.
现在我们了解了我们的流程，让我们构建它。



### Define the process steps  定义流程步骤

Each step of a Process is defined by a class that inherits from our base step class. For this process we have three steps:
Process 的每个步骤都由继承自基阶级的类定义。对于此过程，我们有三个步骤：

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

// A process step to gather information about a product
public class GatherProductInfoStep: KernelProcessStep
{
    [KernelFunction]
    public string GatherProductInformation(string productName)
    {
        Console.WriteLine($"{nameof(GatherProductInfoStep)}:\n\tGathering product information for product named {productName}");

        // For example purposes we just return some fictional information.
        return
            """
            Product Description:
            GlowBrew is a revolutionary AI driven coffee machine with industry leading number of LEDs and programmable light shows. The machine is also capable of brewing coffee and has a built in grinder.

            Product Features:
            1. **Luminous Brew Technology**: Customize your morning ambiance with programmable LED lights that sync with your brewing process.
            2. **AI Taste Assistant**: Learns your taste preferences over time and suggests new brew combinations to explore.
            3. **Gourmet Aroma Diffusion**: Built-in aroma diffusers enhance your coffee's scent profile, energizing your senses before the first sip.

            Troubleshooting:
            - **Issue**: LED Lights Malfunctioning
                - **Solution**: Reset the lighting settings via the app. Ensure the LED connections inside the GlowBrew are secure. Perform a factory reset if necessary.
            """;
    }
}

// A process step to generate documentation for a product
public class GenerateDocumentationStep : KernelProcessStep<GeneratedDocumentationState>
{
    private GeneratedDocumentationState _state = new();

    private string systemPrompt =
            """
            Your job is to write high quality and engaging customer facing documentation for a new product from Contoso. You will be provide with information
            about the product in the form of internal documentation, specs, and troubleshooting guides and you must use this information and
            nothing else to generate the documentation. If suggestions are provided on the documentation you create, take the suggestions into account and
            rewrite the documentation. Make sure the product sounds amazing.
            """;

    // Called by the process runtime when the step instance is activated. Use this to load state that may be persisted from previous activations.
    override public ValueTask ActivateAsync(KernelProcessStepState<GeneratedDocumentationState> state)
    {
        this._state = state.State!;
        this._state.ChatHistory ??= new ChatHistory(systemPrompt);

        return base.ActivateAsync(state);
    }

    [KernelFunction]
    public async Task GenerateDocumentationAsync(Kernel kernel, KernelProcessStepContext context, string productInfo)
    {
        Console.WriteLine($"[{nameof(GenerateDocumentationStep)}]:\tGenerating documentation for provided productInfo...");

        // Add the new product info to the chat history
        this._state.ChatHistory!.AddUserMessage($"Product Info:\n{productInfo.Title} - {productInfo.Content}");

        // Get a response from the LLM
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var generatedDocumentationResponse = await chatCompletionService.GetChatMessageContentAsync(this._state.ChatHistory!);

        DocumentInfo generatedContent = new()
        {
            Id = Guid.NewGuid().ToString(),
            Title = $"Generated document - {productInfo.Title}",
            Content = generatedDocumentationResponse.Content!,
        };

        this._state!.LastGeneratedDocument = generatedContent;

        await context.EmitEventAsync("DocumentationGenerated", generatedContent);
    }

    public class GeneratedDocumentationState
    {
        public DocumentInfo LastGeneratedDocument { get; set; } = new();
        public ChatHistory? ChatHistory { get; set; }
    }
}

// A process step to publish documentation
public class PublishDocumentationStep : KernelProcessStep
{
    [KernelFunction]
    public DocumentInfo PublishDocumentation(DocumentInfo document)
    {
        // For example purposes we just write the generated docs to the console
        Console.WriteLine($"[{nameof(PublishDocumentationStep)}]:\tPublishing product documentation approved by user: \n{document.Title}\n{document.Content}");
        return document;
    }
}

// Custom classes must be serializable
public class DocumentInfo
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
```

The code above defines the three steps we need for our Process. There are a few points to call out here:
上面的代码定义了我们流程所需的三个步骤。这里有几点需要指出：

- In Semantic Kernel, a `KernelFunction` defines a block of code that is invocable by native code or by an LLM. In the case of the Process framework, `KernelFunction`s are the invocable members of a Step and each step requires at least one KernelFunction to be defined.
  在Semantic Kernel中，`KernelFunction` 定义了可由本机代码或 LLM 调用的代码块。对于 Process 框架，`KernelFunction` 是 Step 的可调用成员，每个步骤都需要至少定义一个 KernelFunction。
- The Process Framework has support for stateless and stateful steps. Stateful steps automatically checkpoint their progress and maintain state over multiple invocations. The `GenerateDocumentationStep` provides an example of this where the `GeneratedDocumentationState` class is used to persist the `ChatHistory` and `LastGeneratedDocument` object.
  流程框架支持无状态和有状态步骤。有状态步骤会自动检查其进度，并在多次调用中维护状态。`GenerateDocumentationStep` 提供了一个示例，其中 `GeneratedDocumentationState` 类用于持久化 `ChatHistory` 和 `LastGeneratedDocument` 对象。
- Steps can manually emit events by calling `EmitEventAsync` on the `KernelProcessStepContext` object. To get an instance of `KernelProcessStepContext` just add it as a parameter on your KernelFunction and the framework will automatically inject it.
  步骤可以通过在 `KernelProcessStepContext` 对象上调用 `EmitEventAsync` 来手动发出事件。要获取 `KernelProcessStepContext` 的实例，只需将其作为参数添加到 KernelFunction 上，框架就会自动注入它。



### Define the process flow  定义流程

C#Copy  复制

```csharp
// Create the process builder
ProcessBuilder processBuilder = new("DocumentationGeneration");

// Add the steps
var infoGatheringStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
var docsGenerationStep = processBuilder.AddStepFromType<GenerateDocumentationStep>();
var docsPublishStep = processBuilder.AddStepFromType<PublishDocumentationStep>();

// Orchestrate the events
processBuilder
    .OnInputEvent("Start")
    .SendEventTo(new(infoGatheringStep));

infoGatheringStep
    .OnFunctionResult()
    .SendEventTo(new(docsGenerationStep));

docsGenerationStep
    .OnFunctionResult()
    .SendEventTo(new(docsPublishStep));
```

There are a few things going on here so let's break it down step by step.
这里发生了一些事情，所以让我们一步一步地分解一下。

1. Create the builder: Processes use a builder pattern to simplify wiring everything up. The builder provides methods for managing the steps within a process and for managing the lifecycle of the process.
   创建构建器：流程使用构建器模式来简化连接所有内容。构建器提供了用于管理流程中的步骤和管理流程生命周期的方法。
2. Add the steps: Steps are added to the process by calling the `AddStepFromType` method of the builder. This allows the Process Framework to manage the lifecycle of steps by instantiating instances as needed. In this case we've added three steps to the process and created a variable for each one. These variables give us a handle to the unique instance of each step that we can use next to define the orchestration of events.
   添加步骤：通过调用生成器的 `AddStepFromType` 方法将步骤添加到进程中。这允许流程框架通过根据需要实例化实例来管理步骤的生命周期。在本例中，我们向流程添加了三个步骤，并为每个步骤创建了一个变量。这些变量为我们提供了每个步骤的唯一实例的句柄，我们可以使用它来定义事件的编排。
3. Orchestrate the events: This is where the routing of events from step to step are defined. In this case we have the following routes:
   编排事件：这是定义从步骤到步骤的事件路由的地方。在这种情况下，我们有以下路线：
   - When an external event with `id = Start` is sent to the process, this event and its associated data will be sent to the `infoGatheringStep` step.
     当 `id = Start` 的外部事件发送到流程时，此事件及其关联数据将发送到 `infoGatheringStep` 步骤。
   - When the `infoGatheringStep` finishes running, send the returned object to the `docsGenerationStep` step.
     当 `infoGatheringStep` 完成运行后，将返回的对象发送到 `docsGenerationStep` 步骤。
   - Finally, when the `docsGenerationStep` finishes running, send the returned object to the `docsPublishStep` step.
     最后，当 `docsGenerationStep` 运行完毕后，将返回的对象发送到 `docsPublishStep` 步骤。

 Tip  提示

**Event Routing in Process Framework:** You may be wondering how events that are sent to steps are routed to KernelFunctions within the step. In the code above, each step has only defined a single KernelFunction and each KernelFunction has only a single parameter (other than Kernel and the step context which are special, more on that later). When the event containing the generated documentation is sent to the `docsPublishStep` it will be passed to the `document` parameter of the `PublishDocumentation` KernelFunction of the `docsGenerationStep` step because there is no other choice. However, steps can have multiple KernelFunctions and KernelFunctions can have multiple parameters, in these advanced scenarios you need to specify the target function and parameter.
**流程框架中的事件路由：** 您可能想知道发送到步骤的事件是如何路由到步骤中的 KernelFunctions 的。在上面的代码中，每个步骤只定义了一个 KernelFunction，每个 KernelFunction 只有一个参数（除了 Kernel 和步骤上下文，它们是特殊的，稍后会详细介绍）。当包含生成文档的事件发送到 `docsPublishStep` 时，它将传递给 `docsGenerationStep` 步骤的 `PublishDocumentation` KernelFunction 的 `document` 参数，因为没有其他选择。但是，步骤可以有多个 KernelFunctions，KernelFunctions 可以有多个参数，在这些高级场景中，您需要指定目标函数和参数。



### Build and run the Process 生成并运行流程

C#Copy  复制

```csharp
// Configure the kernel with your LLM connection details
Kernel kernel = Kernel.CreateBuilder()
    .AddAzureOpenAIChatCompletion("myDeployment", "myEndpoint", "myApiKey")
    .Build();

// Build and run the process
var process = processBuilder.Build();
await process.StartAsync(kernel, new KernelProcessEvent { Id = "Start", Data = "Contoso GlowBrew" });
```

We build the process and call `StartAsync` to run it. Our process is expecting an initial external event called `Start` to kick things off and so we provide that as well. Running this process shows the following output in the Console:
我们生成进程并调用 `StartAsync` 来运行它。我们的流程是期待一个名为“` 开始”` 的初始外部事件来启动事情，因此我们也提供了它。运行此过程会在控制台中显示以下输出：

Copy  复制

```
GatherProductInfoStep: Gathering product information for product named Contoso GlowBrew
GenerateDocumentationStep: Generating documentation for provided productInfo
PublishDocumentationStep: Publishing product documentation:

# GlowBrew: Your Ultimate Coffee Experience Awaits!

Welcome to the world of GlowBrew, where coffee brewing meets remarkable technology! At Contoso, we believe that your morning ritual shouldn't just include the perfect cup of coffee but also a stunning visual experience that invigorates your senses. Our revolutionary AI-driven coffee machine is designed to transform your kitchen routine into a delightful ceremony.

## Unleash the Power of GlowBrew

### Key Features

- **Luminous Brew Technology**
  - Elevate your coffee experience with our cutting-edge programmable LED lighting. GlowBrew allows you to customize your morning ambiance, creating a symphony of colors that sync seamlessly with your brewing process. Whether you need a vibrant wake-up call or a soothing glow, you can set the mood for any moment!

- **AI Taste Assistant**
  - Your taste buds deserve the best! With the GlowBrew built-in AI taste assistant, the machine learns your unique preferences over time and curates personalized brew suggestions just for you. Expand your coffee horizons and explore delightful new combinations that fit your palate perfectly.

- **Gourmet Aroma Diffusion**
  - Awaken your senses even before that first sip! The GlowBrew comes equipped with gourmet aroma diffusers that enhance the scent profile of your coffee, diffusing rich aromas that fill your kitchen with the warm, inviting essence of freshly-brewed bliss.

### Not Just Coffee - An Experience

With GlowBrew, it's more than just making coffee-it's about creating an experience that invigorates the mind and pleases the senses. The glow of the lights, the aroma wafting through your space, and the exceptional taste meld into a delightful ritual that prepares you for whatever lies ahead.

## Troubleshooting Made Easy

While GlowBrew is designed to provide a seamless experience, we understand that technology can sometimes be tricky. If you encounter issues with the LED lights, we've got you covered:

- **LED Lights Malfunctioning?**
  - If your LED lights aren't working as expected, don't worry! Follow these steps to restore the glow:
    1. **Reset the Lighting Settings**: Use the GlowBrew app to reset the lighting settings.
    2. **Check Connections**: Ensure that the LED connections inside the GlowBrew are secure.
    3. **Factory Reset**: If you're still facing issues, perform a factory reset to rejuvenate your machine.

With GlowBrew, you not only brew the perfect coffee but do so with an ambiance that excites the senses. Your mornings will never be the same!

## Embrace the Future of Coffee

Join the growing community of GlowBrew enthusiasts today, and redefine how you experience coffee. With stunning visual effects, customized brewing suggestions, and aromatic enhancements, it's time to indulge in the delightful world of GlowBrew-where every cup is an adventure!

### Conclusion

Ready to embark on an extraordinary coffee journey? Discover the perfect blend of technology and flavor with Contoso's GlowBrew. Your coffee awaits!
```



## What's Next?  下一步是什么？

Our first draft of the documentation generation process is working but it leaves a lot to be desired. At a minimum, a production version would need:
我们的文档生成过程初稿正在发挥作用，但还有很多不足之处。生产版本至少需要：

- A proof reader agent that will grade the generated documentation and verify that it meets our standards of quality and accuracy.
  校对代理将对生成的文档进行分级并验证其是否符合我们的质量和准确性标准。
- An approval process where the documentation is only published after a human approves it (human-in-the-loop).
  一种审批流程，文档只有在人工批准后才会发布（人机交互）。

[  将校对代理添加到我们的流程中......](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/process/examples/example-cycles)