# How-To: Human-in-the-Loop 作方法：人机交互

- 04/12/2025



 Warning  警告

The *Semantic Kernel Process Framework* is experimental, still in development and is subject to change.
*Semantic Kernel进程框架*是实验性的，仍在开发中，可能会发生变化。



## Overview  概述

In the previous sections we built a Process to help us automate the creation of documentation for our new product. Our process can now generate documentation that is specific to our product, and can ensure it meets our quality bar by running it through a proofread and edit cycle. In this section we will improve on that process again by requiring a human to approve or reject the documentation before it's published. The flexibility of the process framework means that there are several ways that we could go about doing this but in this example we will demonstrate integration with an external pubsub system for requesting approval.
在前面的部分中，我们构建了一个流程来帮助我们自动创建新产品的文档。我们的流程现在可以生成特定于我们产品的文档，并通过校对和编辑周期来确保它符合我们的质量标准。在本节中，我们将再次改进该过程，要求人工在文档发布之前批准或拒绝文档。流程框架的灵活性意味着我们可以通过多种方式做到这一点，但在此示例中，我们将演示与外部 pubsub 系统的集成以请求批准。

![Flow diagram for our process with a human-in-the-loop pattern.](https://learn.microsoft.com/en-us/semantic-kernel/media/process-human-in-the-loop-2.png)



### Make publishing wait for approval 使发布等待批准

The first change we need to make to the process is to make the publishing step wait for the approval before it publishes the documentation. One option is to simply add a second parameter for the approval to the `PublishDocumentation` function in the `PublishDocumentationStep`. This works because a KernelFunction in a step will only be invoked when all of its required parameters have been provided.
我们需要对流程进行的第一个更改是让发布步骤在发布文档之前等待批准。一种选择是简单地将第二个批准参数添加到 `PublishDocumentationStep` 中的 `PublishDocumentation` 函数中。这是有效的，因为只有在提供了所有必需的参数时，才会调用步骤中的 KernelFunction。

C#Copy  复制

```csharp
// A process step to publish documentation
public class PublishDocumentationStep : KernelProcessStep
{
    [KernelFunction]
    public DocumentInfo PublishDocumentation(DocumentInfo document, bool userApproval) // added the userApproval parameter
    {
        // Only publish the documentation if it has been approved
        if (userApproval)
        {
            // For example purposes we just write the generated docs to the console
            Console.WriteLine($"[{nameof(PublishDocumentationStep)}]:\tPublishing product documentation approved by user: \n{document.Title}\n{document.Content}");
        }
        return document;
    }
}
```

With the code above, the `PublishDocumentation` function in the `PublishDocumentationStep` will only be invoked when the generated documentation has been sent to the `document` parameter and the result of the approval has been sent to the `userApproval` parameter.
使用上述代码，仅当生成的文档已发送到 `document` 参数并且审批结果已发送到 `userApproval` 参数时，才会调用 `PublishDocumentationStep` 中的 `PublishDocumentation` 函数。

We can now reuse the existing logic of `ProofreadStep` step to additionally emit an event to our external pubsub system which will notify the human approver that there is a new request.
我们现在可以重用 `ProofreadStep` 步骤的现有逻辑，向外部 pubsub 系统额外发出一个事件，该事件将通知人工审批者有新请求。

C#Copy  复制

```csharp
// A process step to publish documentation
public class ProofReadDocumentationStep : KernelProcessStep
{
    ...

    if (formattedResponse.MeetsExpectations)
    {
        // Events that are getting piped to steps that will be resumed, like PublishDocumentationStep.OnPublishDocumentation
        // require events to be marked as public so they are persisted and restored correctly
        await context.EmitEventAsync("DocumentationApproved", data: document, visibility: KernelProcessEventVisibility.Public);
    }
    ...
}
```

Since we want to publish the newly generated documentation when it is approved by the proofread agent, the approved documents will be queued on the publishing step. In addition, a human will be notified via our external pubsub system with an update on the latest document. Let's update the process flow to match this new design.
由于我们希望在校对代理批准新生成的文档时发布，因此已批准的文档将在发布步骤中排队。此外，我们将通过我们的外部 pubsub 系统通知人工，并更新最新文档。让我们更新流程以匹配这个新设计。

C#Copy  复制

```csharp
// Create the process builder
ProcessBuilder processBuilder = new("DocumentationGeneration");

// Add the steps
var infoGatheringStep = processBuilder.AddStepFromType<GatherProductInfoStep>();
var docsGenerationStep = processBuilder.AddStepFromType<GenerateDocumentationStepV2>();
var docsProofreadStep = processBuilder.AddStepFromType<ProofreadStep>();
var docsPublishStep = processBuilder.AddStepFromType<PublishDocumentationStep>();

// internal component that allows emitting SK events externally, a list of topic names
// is needed to link them to existing SK events
var proxyStep = processBuilder.AddProxyStep(["RequestUserReview", "PublishDocumentation"]);

// Orchestrate the events
processBuilder
    .OnInputEvent("StartDocumentGeneration")
    .SendEventTo(new(infoGatheringStep));

processBuilder
    .OnInputEvent("UserRejectedDocument")
    .SendEventTo(new(docsGenerationStep, functionName: "ApplySuggestions"));

// When external human approval event comes in, route it to the 'isApproved' parameter of the docsPublishStep
processBuilder
    .OnInputEvent("UserApprovedDocument")
    .SendEventTo(new(docsPublishStep, parameterName: "userApproval"));

// Hooking up the rest of the process steps
infoGatheringStep
    .OnFunctionResult()
    .SendEventTo(new(docsGenerationStep, functionName: "GenerateDocumentation"));

docsGenerationStep
    .OnEvent("DocumentationGenerated")
    .SendEventTo(new(docsProofreadStep));

docsProofreadStep
    .OnEvent("DocumentationRejected")
    .SendEventTo(new(docsGenerationStep, functionName: "ApplySuggestions"));

// When the proofreader approves the documentation, send it to the 'document' parameter of the docsPublishStep
// Additionally, the generated document is emitted externally for user approval using the pre-configured proxyStep
docsProofreadStep
    .OnEvent("DocumentationApproved")
    // [NEW] addition to emit messages externally
    .EmitExternalEvent(proxyStep, "RequestUserReview") // Hooking up existing "DocumentationApproved" to external topic "RequestUserReview"
    .SendEventTo(new(docsPublishStep, parameterName: "document"));

// When event is approved by user, it gets published externally too
docsPublishStep
    .OnFunctionResult()
    // [NEW] addition to emit messages externally
    .EmitExternalEvent(proxyStep, "PublishDocumentation");

var process = processBuilder.Build();
return process;
```

Finally, an implementation of the interface `IExternalKernelProcessMessageChannel` should be provided since it is internally use by the new `ProxyStep`. This interface is used to emit messages externally. The implementation of this interface will depend on the external system that you are using. In this example, we will use a custom client that we have created to send messages to an external pubsub system.
最后，应该提供接口 `IExternalKernelProcessMessageChannel` 的实现，因为它是由新的 `ProxyStep` 内部使用的。该接口用于向外部发出消息。此接口的实现将取决于您正在使用的外部系统。在此示例中，我们将使用我们创建的自定义客户端将消息发送到外部 pubsub 系统。

C#Copy  复制

```csharp
// Example of potential custom IExternalKernelProcessMessageChannel implementation 
public class MyCloudEventClient : IExternalKernelProcessMessageChannel
{
    private MyCustomClient? _customClient;

    // Example of an implementation for the process
    public async Task EmitExternalEventAsync(string externalTopicEvent, KernelProcessProxyMessage message)
    {
        // logic used for emitting messages externally.
        // Since all topics are received here potentially 
        // some if else/switch logic is needed to map correctly topics with external APIs/endpoints.
        if (this._customClient != null)
        {
            switch (externalTopicEvent) 
            {
                case "RequestUserReview":
                    var requestDocument = message.EventData.ToObject() as DocumentInfo;
                    // As an example only invoking a sample of a custom client with a different endpoint/api route
                    this._customClient.InvokeAsync("REQUEST_USER_REVIEW", requestDocument);
                    return;

                case "PublishDocumentation":
                    var publishedDocument = message.EventData.ToObject() as DocumentInfo;
                    // As an example only invoking a sample of a custom client with a different endpoint/api route
                    this._customClient.InvokeAsync("PUBLISH_DOC_EXTERNALLY", publishedDocument);
                    return;
            }
        }
    }

    public async ValueTask Initialize()
    {
        // logic needed to initialize proxy step, can be used to initialize custom client
        this._customClient = new MyCustomClient("http://localhost:8080");
        this._customClient.Initialize();
    }

    public async ValueTask Uninitialize()
    {
        // Cleanup to be executed when proxy step is uninitialized
        if (this._customClient != null)
        {
            await this._customClient.ShutdownAsync();
        }
    }
}
```

Finally to allow the process `ProxyStep` to make use of the `IExternalKernelProcessMessageChannel` implementation, in this case `MyCloudEventClient`, we need to pipe it properly.
最后，为了允许进程 `ProxyStep` 使用 `IExternalKernelProcessMessageChannel` 实现，在本例中为 `MyCloudEventClient`，我们需要正确地对其进行管道处理。

When using Local Runtime, the implemented class can be passed when invoking `StartAsync` on the `KernelProcess` class.
使用本地运行时时，可以在 `KernelProcess` 类上调用 `StartAsync` 时传递实现的类。

C#Copy  复制

```csharp
KernelProcess process;
IExternalKernelProcessMessageChannel myExternalMessageChannel = new MyCloudEventClient();
// Start the process with the external message channel
await process.StartAsync(kernel, new KernelProcessEvent 
    {
        Id = inputEvent,
        Data = input,
    },
    myExternalMessageChannel)
```

When using Dapr Runtime, the plumbing has to be done through dependency injection at the Program setup of the project.
使用 Dapr 运行时时，必须通过项目的程序设置中的依赖项注入来完成管道。

C#Copy  复制

```csharp
var builder = WebApplication.CreateBuilder(args);
...
// depending on the application a singleton or scoped service can be used
// Injecting SK Process custom client IExternalKernelProcessMessageChannel implementation
builder.Services.AddSingleton<IExternalKernelProcessMessageChannel, MyCloudEventClient>();
```

Two changes have been made to the process flow:
对流程进行了两项更改：

- Added an input event named `HumanApprovalResponse` that will be routed to the `userApproval` parameter of the `docsPublishStep` step.
  添加了一个名为 `HumanApprovalResponse` 的输入事件，该事件将路由到 `docsPublishStep` 步骤的 `userApproval` 参数。
- Since the KernelFunction in `docsPublishStep` now has two parameters, we need to update the existing route to specify the parameter name of `document`.
  由于 `docsPublishStep` 中的 KernelFunction 现在有两个参数，因此我们需要更新现有路由来指定`文档`的参数名称。

Run the process as you did before and notice that this time when the proofreader approves the generated documentation and sends it to the `document` parameter of the `docPublishStep` step, the step is no longer invoked because it is waiting for the `userApproval` parameter. At this point the process goes idle because there are no steps ready to be invoked and the call that we made to start the process returns. The process will remain in this idle state until our "human-in-the-loop" takes action to approve or reject the publish request. Once this has happened and the result has been communicated back to our program, we can restart the process with the result.
像以前一样运行该过程，并注意到，这一次，当校对员批准生成的文档并将其发送到 `docPublishStep` 步骤的`文档`参数时，不再调用该步骤，因为它正在等待 `userApproval` 参数。此时，进程将处于空闲状态，因为没有准备好调用的步骤，并且我们为启动进程而进行的调用将返回。该进程将保持此空闲状态，直到我们的“人机交互”采取措施批准或拒绝发布请求。一旦发生这种情况并将结果传回我们的程序，我们就可以使用结果重新启动该过程。

C#Copy  复制

```csharp
// Restart the process with approval for publishing the documentation.
await process.StartAsync(kernel, new KernelProcessEvent { Id = "UserApprovedDocument", Data = true });
```

When the process is started again with the `UserApprovedDocument` it will pick up from where it left off and invoke the `docsPublishStep` with `userApproval` set to `true` and our documentation will be published. If it is started again with the `UserRejectedDocument` event, the process will kick off the `ApplySuggestions` function in the `docsGenerationStep` step and the process will continue as before.
当使用 `UserApprovedDocument` 再次启动该过程时，它将从中断的地方继续，并在 `userApproval` 设置为 `true` 的情况下调用 `docsPublishStep`，我们的文档将被发布。如果使用 `UserRejectedDocument` 事件再次启动，则该过程将在 `docsGenerationStep` 步骤中启动 `ApplySuggestions` 函数，并且该过程将像以前一样继续。

The process is now complete and we have successfully added a human-in-the-loop step to our process. The process can now be used to generate documentation for our product, proofread it, and publish it once it has been approved by a human.
该过程现已完成，我们已成功地在流程中添加了人机交互步骤。该过程现在可用于为我们的产品生成文档、校对并在获得人工批准后发布。