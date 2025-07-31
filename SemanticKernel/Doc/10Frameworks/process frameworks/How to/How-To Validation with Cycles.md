# How-To: Using Cycles  作方法：使用循环

- 02/26/2025



 Warning  警告

The *Semantic Kernel Process Framework* is experimental, still in development and is subject to change.
*Semantic Kernel进程框架*是实验性的，仍在开发中，可能会发生变化。



## Overview  概述

In the previous section we built a simple Process to help us automate the creation of documentation for our new product. In this section we will improve on that process by adding a proofreading step. This step will use and LLM to grade the generated documentation as Pass/Fail, and provide recommended changes if needed. By taking advantage of the Process Frameworks' support for cycles, we can go one step further and automatically apply the recommended changes (if any) and then start the cycle over, repeating this until the content meets our quality bar. The updated process will look like this:
在上一节中，我们构建了一个简单的流程来帮助我们自动创建新产品的文档。在本节中，我们将通过添加校对步骤来改进该过程。此步骤将使用 和 LLM 将生成的文档评为通过/失败，并在需要时提供建议的更改。通过利用流程框架对循环的支持，我们可以更进一步，自动应用建议的更改（如果有），然后重新开始循环，重复此作，直到内容达到我们的质量标准。更新后的过程将如下所示：

![Flow diagram for our process with a cycle for author-critic pattern.](https://learn.microsoft.com/en-us/semantic-kernel/media/process-cycle-flow.png)



## Updates to the process  流程更新

We need to create our new proofreader step and also make a couple changes to our document generation step that will allow us to apply suggestions if needed.
我们需要创建新的校对步骤，并对文档生成步骤进行一些更改，以便我们在需要时应用建议。



### Add the proofreader step  添加校对步骤

C#Copy  复制

```csharp
// A process step to proofread documentation
public class ProofreadStep : KernelProcessStep
{
    [KernelFunction]
    public async Task ProofreadDocumentationAsync(Kernel kernel, KernelProcessStepContext context, string documentation)
    {
        Console.WriteLine($"{nameof(ProofreadDocumentationAsync)}:\n\tProofreading documentation...");

        var systemPrompt =
            """
        Your job is to proofread customer facing documentation for a new product from Contoso. You will be provide with proposed documentation
        for a product and you must do the following things:

        1. Determine if the documentation is passes the following criteria:
            1. Documentation must use a professional tone.
            1. Documentation should be free of spelling or grammar mistakes.
            1. Documentation should be free of any offensive or inappropriate language.
            1. Documentation should be technically accurate.
        2. If the documentation does not pass 1, you must write detailed feedback of the changes that are needed to improve the documentation. 
        """;

        ChatHistory chatHistory = new ChatHistory(systemPrompt);
        chatHistory.AddUserMessage(documentation);

        // Use structured output to ensure the response format is easily parsable
        OpenAIPromptExecutionSettings settings = new OpenAIPromptExecutionSettings();
        settings.ResponseFormat = typeof(ProofreadingResponse);

        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var proofreadResponse = await chatCompletionService.GetChatMessageContentAsync(chatHistory, executionSettings: settings);
        var formattedResponse = JsonSerializer.Deserialize<ProofreadingResponse>(proofreadResponse.Content!.ToString());

        Console.WriteLine($"\n\tGrade: {(formattedResponse!.MeetsExpectations ? "Pass" : "Fail")}\n\tExplanation: {formattedResponse.Explanation}\n\tSuggestions: {string.Join("\n\t\t", formattedResponse.Suggestions)}");

        if (formattedResponse.MeetsExpectations)
        {
            await context.EmitEventAsync("DocumentationApproved", data: documentation);
        }
        else
        {
            await context.EmitEventAsync("DocumentationRejected", data: new { Explanation = formattedResponse.Explanation, Suggestions = formattedResponse.Suggestions});
        }
    }

    // A class 
    private class ProofreadingResponse
    {
        [Description("Specifies if the proposed documentation meets the expected standards for publishing.")]
        public bool MeetsExpectations { get; set; }

        [Description("An explanation of why the documentation does or does not meet expectations.")]
        public string Explanation { get; set; } = "";

        [Description("A lis of suggestions, may be empty if there no suggestions for improvement.")]
        public List<string> Suggestions { get; set; } = new();
    }
}
```

A new step named `ProofreadStep` has been created. This step uses the LLM to grade the generated documentation as discussed above. Notice that this step conditionally emits either the `DocumentationApproved` event or the `DocumentationRejected` event based on the response from the LLM. In the case of `DocumentationApproved`, the event will include the approved documentation as it's payload and in the case of `DocumentationRejected` it will include the suggestions from the proofreader.
已创建名为 `ProofreadStep` 的新步骤。如上所述，此步骤使用 LLM 对生成的文档进行评分。请注意，此步骤会根据 LLM 的响应有条件地发出 `DocumentationApproved` 事件或 `DocumentationRejected` 事件。对于 `DocumentationApproved`，该事件将包括已批准的文档作为有效负载，如果是 `DocumentationRejected`，它将包括校对员的建议。



### Update the documentation generation step 更新文档生成步骤

C#Copy  复制

```csharp
// Updated process step to generate and edit documentation for a product
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

    override public ValueTask ActivateAsync(KernelProcessStepState<GeneratedDocumentationState> state)
    {
        this._state = state.State!;
        this._state.ChatHistory ??= new ChatHistory(systemPrompt);

        return base.ActivateAsync(state);
    }

    [KernelFunction]
    public async Task GenerateDocumentationAsync(Kernel kernel, KernelProcessStepContext context, string productInfo)
    {
        Console.WriteLine($"{nameof(GenerateDocumentationStep)}:\n\tGenerating documentation for provided productInfo...");

        // Add the new product info to the chat history
        this._state.ChatHistory!.AddUserMessage($"Product Info:\n\n{productInfo}");

        // Get a response from the LLM
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var generatedDocumentationResponse = await chatCompletionService.GetChatMessageContentAsync(this._state.ChatHistory!);

        await context.EmitEventAsync("DocumentationGenerated", generatedDocumentationResponse.Content!.ToString());
    }

    [KernelFunction]
    public async Task ApplySuggestionsAsync(Kernel kernel, KernelProcessStepContext context, string suggestions)
    {
        Console.WriteLine($"{nameof(GenerateDocumentationStep)}:\n\tRewriting documentation with provided suggestions...");

        // Add the new product info to the chat history
        this._state.ChatHistory!.AddUserMessage($"Rewrite the documentation with the following suggestions:\n\n{suggestions}");

        // Get a response from the LLM
        IChatCompletionService chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        var generatedDocumentationResponse = await chatCompletionService.GetChatMessageContentAsync(this._state.ChatHistory!);

        await context.EmitEventAsync("DocumentationGenerated", generatedDocumentationResponse.Content!.ToString());
    }

    public class GeneratedDocumentationState
    {
        public ChatHistory? ChatHistory { get; set; }
    }
}
```

The `GenerateDocumentationStep` has been updated to include a new KernelFunction. The new function will be used to apply suggested changes to the documentation if our proofreading step requires them. Notice that both functions for generating or rewriting documentation emit the same event named `DocumentationGenerated` indicating that new documentation is available.
`GenerateDocumentationStep` 已更新，以包含新的 KernelFunction。如果我们的校对步骤需要，新功能将用于将建议的更改应用于文档。请注意，用于生成或重写文档的两个函数都会发出名为 `DocumentationGenerated` 的相同事件，指示新文档可用。



### Flow updates  流更新

C#Copy  复制

```csharp
// Create the process builder
ProcessBuilder processBuilder = new("DocumentationGeneration");

// Add the steps
var infoGatheringStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
var docsGenerationStep = processBuilder.AddStepFromType<GenerateDocumentationStepV2>();
var docsProofreadStep = processBuilder.AddStepFromType<ProofreadStep>(); // Add new step here
var docsPublishStep = processBuilder.AddStepFromType<PublishDocumentationStep>();

// Orchestrate the events
processBuilder
    .OnInputEvent("Start")
    .SendEventTo(new(infoGatheringStep));

infoGatheringStep
    .OnFunctionResult()
    .SendEventTo(new(docsGenerationStep, functionName: "GenerateDocumentation"));

docsGenerationStep
    .OnEvent("DocumentationGenerated")
    .SendEventTo(new(docsProofreadStep));

docsProofreadStep
    .OnEvent("DocumentationRejected")
    .SendEventTo(new(docsGenerationStep, functionName: "ApplySuggestions"));

docsProofreadStep
    .OnEvent("DocumentationApproved")
    .SendEventTo(new(docsPublishStep));

var process = processBuilder.Build();
return process;
```

Our updated process routing now does the following:
我们更新的流程路由现在执行以下作：

- When an external event with `id = Start` is sent to the process, this event and its associated data will be sent to the `infoGatheringStep`.
  当将 `id = Start` 的外部事件发送到进程时，此事件及其关联数据将发送到 `infoGatheringStep`。
- When the `infoGatheringStep` finishes running, send the returned object to the `docsGenerationStep`.
  当 `infoGatheringStep` 完成运行时，将返回的对象发送到 `docsGenerationStep`。
- When the `docsGenerationStep` finishes running, send the generated docs to the `docsProofreadStep`.
  当 `docsGenerationStep` 运行完毕后，将生成的文档发送到 `docsProofreadStep`。
- When the `docsProofreadStep` rejects our documentation and provides suggestions, send the suggestions back to the `docsGenerationStep`.
  当 `docsProofreadStep` 拒绝我们的文档并提供建议时，将建议发送回 `docsGenerationStep`。
- Finally, when the `docsProofreadStep` approves our documentation, send the returned object to the `docsPublishStep`.
  最后，当 `docsProofreadStep` 批准我们的文档时，将返回的对象发送到 `docsPublishStep`。



### Build and run the Process 生成并运行流程

Running our updated process shows the following output in the console:
运行更新的进程会在控制台中显示以下输出：

Markdown  降价Copy  复制

```md
GatherProductInfoStep:
        Gathering product information for product named Contoso GlowBrew
GenerateDocumentationStep:
        Generating documentation for provided productInfo...
ProofreadDocumentationAsync:
        Proofreading documentation...

        Grade: Fail
        Explanation: The proposed documentation has an overly casual tone and uses informal expressions that might not suit all customers. Additionally, some phrases may detract from the professionalism expected in customer-facing documentation. There are minor areas that could benefit from clarity and conciseness.
        Suggestions: Adjust the tone to be more professional and less casual; phrases like 'dazzling light show' and 'coffee performing' could be simplified.
                Remove informal phrases such as 'who knew coffee could be so... illuminating?'
                Consider editing out overly whimsical phrases like 'it's like a warm hug for your nose!' for a more straightforward description.
                Clarify the troubleshooting section for better customer understanding; avoid metaphorical language like 'secure that coffee cup when you realize Monday is still a thing.'
GenerateDocumentationStep:
        Rewriting documentation with provided suggestions...
ProofreadDocumentationAsync:
        Proofreading documentation...

        Grade: Fail
        Explanation: The documentation generally maintains a professional tone but contains minor phrasing issues that could be improved. There are no spelling or grammar mistakes noted, and it excludes any offensive language. However, the content could be more concise, and some phrases can be streamlined for clarity. Additionally, technical accuracy regarding troubleshooting solutions may require more details for the user's understanding. For example, clarifying how to 'reset the lighting settings through the designated app' would enhance user experience.
        Suggestions: Rephrase 'Join us as we elevate your coffee experience to new heights!' to make it more straightforward, such as 'Experience an elevated coffee journey with us.'
                In the 'Solution' section for the LED lights malfunction, add specific instructions on how to find and use the 'designated app' for resetting the lighting settings.
                Consider simplifying sentences such as 'Meet your new personal barista!' to be more straightforward, for example, 'Introducing your personal barista.'
                Ensure clarity in troubleshooting steps by elaborating on what a 'factory reset' entails.
GenerateDocumentationStep:
        Rewriting documentation with provided suggestions...
ProofreadDocumentationAsync:
        Proofreading documentation...

        Grade: Pass
        Explanation: The documentation presents a professional tone, contains no spelling or grammar mistakes, is free of offensive language, and is technically accurate regarding the product's features and troubleshooting guidance.
        Suggestions:
PublishDocumentationStep:
        Publishing product documentation:

# GlowBrew User Documentation

## Product Overview
Introducing GlowBrew-your new partner in coffee brewing that brings together advanced technology and aesthetic appeal. This innovative AI-driven coffee machine not only brews your favorite coffee but also features the industry's leading number of customizable LEDs and programmable light shows.

## Key Features

1. **Luminous Brew Technology**: Transform your morning routine with our customizable LED lights that synchronize with your brewing process, creating the perfect ambiance to start your day.

2. **AI Taste Assistant**: Our intelligent system learns your preferences over time, recommending exciting new brew combinations tailored to your unique taste.

3. **Gourmet Aroma Diffusion**: Experience an enhanced aroma with built-in aroma diffusers that elevate your coffee's scent profile, invigorating your senses before that all-important first sip.

## Troubleshooting

### Issue: LED Lights Malfunctioning

**Solution**:
- Begin by resetting the lighting settings via the designated app. Open the app, navigate to the settings menu, and select "Reset LED Lights."
- Ensure that all LED connections inside the GlowBrew are secure and properly connected.
- If issues persist, you may consider performing a factory reset. To do this, hold down the reset button located on the machine's back panel for 10 seconds while the device is powered on.

We hope you enjoy your GlowBrew experience and that it brings a delightful blend of flavor and brightness to your coffee moments!
```



## What's Next?  下一步是什么？

Our process is now reliably generating documentation that meets our defined standards. This is great, but before we publish our documentation publicly we really should require a human to review and approve. Let's do that next.
我们的流程现在正在可靠地生成符合我们定义标准的文档。这很好，但在我们公开发布文档之前，我们确实应该要求人工审查和批准。接下来让我们这样做。

[  人机交互](https://learn.microsoft.com/en-us/semantic-kernel/frameworks/process/examples/example-human-in-loop)