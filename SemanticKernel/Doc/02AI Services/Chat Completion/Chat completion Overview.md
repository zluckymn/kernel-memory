# Chat completion  聊天完成 Overview

- 05/29/2025

With chat completion, you can simulate a back-and-forth conversation with an AI agent. This is of course useful for creating chat bots, but it can also be used for creating autonomous agents that can complete business processes, generate code, and more. As the primary model type provided by OpenAI, Google, Mistral, Facebook, and others, chat completion is the most common AI service that you will add to your Semantic Kernel project.
通过聊天完成，您可以模拟与 AI 代理的来回对话。这当然对于创建聊天机器人很有用，但它也可用于创建可以完成业务流程、生成代码等的自主代理。作为 OpenAI、Google、Mistral、Facebook 等提供的主要模型类型，聊天完成是您将添加到Kernel项目中最常见的 AI 服务。

When picking out a chat completion model, you will need to consider the following:
在选择聊天完成模型时，您需要考虑以下因素：

- What modalities does the model support (e.g., text, image, audio, etc.)?
  模型支持哪些模态（例如文本、图像、音频等）？
- Does it support function calling?
  它是否支持函数调用？
- How fast does it receive and generate tokens?
  它接收和生成代币的速度有多快？
- How much does each token cost?
  每个代币的价格是多少？

 Important  重要

Of all the above questions, the most important is whether the model supports function calling. If it does not, you will not be able to use the model to call your existing code. Most of the latest models from OpenAI, Google, Mistral, and Amazon all support function calling. Support from small language models, however, is still limited.
以上所有问题中，最重要的是模型是否支持函数调用。如果没有，您将无法使用该模型调用现有代码。OpenAI、谷歌、Mistral 和亚马逊的大多数最新模型都支持函数调用。然而，小型语言模型的支持仍然有限。



## Setting up your local environment 设置本地环境

Some of the AI Services can be hosted locally and may require some setup. Below are instructions for those that support this.
某些 AI 服务可以托管在本地，可能需要一些设置。以下是支持此功能的说明。

- [Azure OpenAI](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-AzureOpenAI)
- [OpenAI  开放人工智能](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-OpenAI)
- [Mistral  米斯特拉尔](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-Mistral)
- [Google  谷歌](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-Google)
- [Hugging Face  拥抱脸](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-HuggingFace)
- [Azure AI Inference
  Azure AI 推理](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-AzureAIInference)
- [Ollama  奥拉玛](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-Ollama)
- [Anthropic  人为的](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-Anthropic)
- [Amazon Bedrock  亚马逊基岩](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-AmazonBedrock)
- [ONNX](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-ONNX)
- [Other  其他](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_1_csharp-other)

No local setup.  没有本地设置。



## Installing the necessary packages 安装必要的软件包

Before adding chat completion to your kernel, you will need to install the necessary packages. Below are the packages you will need to install for each AI service provider.
在将聊天完成添加到内核之前，您需要安装必要的软件包。以下是您需要为每个 AI 服务提供商安装的软件包。

- [Azure OpenAI](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-AzureOpenAI)
- [OpenAI  开放人工智能](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-OpenAI)
- [Mistral  米斯特拉尔](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-Mistral)
- [Google  谷歌](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-Google)
- [Hugging Face  拥抱脸](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-HuggingFace)
- [Azure AI Inference
  Azure AI 推理](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-AzureAIInference)
- [Ollama  奥拉玛](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-Ollama)
- [Anthropic  人为的](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-Anthropic)
- [Amazon Bedrock  亚马逊基岩](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-AmazonBedrock)
- [ONNX](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-ONNX)
- [Other  其他](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_2_csharp-other)

Bash  猛击Copy  复制

```bash
dotnet add package Microsoft.SemanticKernel.Connectors.AzureOpenAI
```



## Creating chat completion services 创建聊天完成服务

Now that you've installed the necessary packages, you can create chat completion services. Below are the several ways you can create chat completion services using Semantic Kernel.
现在，您已安装必要的包，您可以创建聊天完成服务。以下是使用Kernel创建聊天完成服务的几种方法。



### Adding directly to the kernel 直接添加到内核

To add a chat completion service, you can use the following code to add it to the kernel's inner service provider.
要添加聊天完成服务，您可以使用以下代码将其添加到内核的内部服务提供程序中。

- [Azure OpenAI](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-AzureOpenAI)
- [OpenAI  开放人工智能](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-OpenAI)
- [Mistral  米斯特拉尔](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-Mistral)
- [Google  谷歌](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-Google)
- [Hugging Face  拥抱脸](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-HuggingFace)
- [Azure AI Inference
  Azure AI 推理](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-AzureAIInference)
- [Ollama  奥拉玛](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-Ollama)
- [Anthropic  人为的](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-Anthropic)
- [Amazon Bedrock  亚马逊基岩](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-AmazonBedrock)
- [ONNX](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-ONNX)
- [Other  其他](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_3_csharp-other)

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
kernelBuilder.AddAzureOpenAIChatCompletion(
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
    apiKey: "YOUR_API_KEY",
    endpoint: "YOUR_AZURE_ENDPOINT",
    modelId: "gpt-4", // Optional name of the underlying model if the deployment name doesn't match the model name
    serviceId: "YOUR_SERVICE_ID", // Optional; for targeting specific services within Semantic Kernel
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);
Kernel kernel = kernelBuilder.Build();
```



### Using dependency injection 使用依赖注入

If you're using dependency injection, you'll likely want to add your AI services directly to the service provider. This is helpful if you want to create singletons of your AI services and reuse them in transient kernels.
如果您使用的是依赖项注入，您可能希望将 AI 服务直接添加到服务提供商。如果您想创建 AI 服务的单例并在瞬态内核中重用它们，这将非常有用。

- [Azure OpenAI](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-AzureOpenAI)
- [OpenAI  开放人工智能](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-OpenAI)
- [Mistral  米斯特拉尔](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-Mistral)
- [Google  谷歌](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-Google)
- [Hugging Face  拥抱脸](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-HuggingFace)
- [Azure AI Inference
  Azure AI 推理](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-AzureAIInference)
- [Ollama  奥拉玛](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-Ollama)
- [Anthropic  人为的](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-Anthropic)
- [Amazon Bedrock  亚马逊基岩](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-AmazonBedrock)
- [ONNX](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-ONNX)
- [Other  其他](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_4_csharp-other)

C#Copy  复制

```csharp
using Microsoft.SemanticKernel;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddAzureOpenAIChatCompletion(
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
    apiKey: "YOUR_API_KEY",
    endpoint: "YOUR_AZURE_ENDPOINT",
    modelId: "gpt-4", // Optional name of the underlying model if the deployment name doesn't match the model name
    serviceId: "YOUR_SERVICE_ID" // Optional; for targeting specific services within Semantic Kernel
);

builder.Services.AddTransient((serviceProvider)=> {
    return new Kernel(serviceProvider);
});
```



### Creating standalone instances 创建独立实例

Lastly, you can create instances of the service directly so that you can either add them to a kernel later or use them directly in your code without ever injecting them into the kernel or in a service provider.
最后，您可以直接创建服务的实例，以便稍后可以将它们添加到内核中，也可以直接在代码中使用它们，而无需将它们注入内核或服务提供商中。

- [Azure OpenAI](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-AzureOpenAI)
- [OpenAI  开放人工智能](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-OpenAI)
- [Mistral  米斯特拉尔](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-Mistral)
- [Google  谷歌](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-Google)
- [Hugging Face  拥抱脸](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-HuggingFace)
- [Azure AI Inference
  Azure AI 推理](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-AzureAIInference)
- [Ollama  奥拉玛](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-Ollama)
- [Anthropic  人为的](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-Anthropic)
- [Amazon Bedrock  亚马逊基岩](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-AmazonBedrock)
- [ONNX](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-ONNX)
- [Other  其他](https://learn.microsoft.com/en-us/semantic-kernel/concepts/ai-services/chat-completion/?tabs=csharp-AzureOpenAI%2Cpython-AzureOpenAI%2Cjava-AzureOpenAI&pivots=programming-language-csharp#tabpanel_5_csharp-other)

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;

AzureOpenAIChatCompletionService chatCompletionService = new (
    deploymentName: "NAME_OF_YOUR_DEPLOYMENT",
    apiKey: "YOUR_API_KEY",
    endpoint: "YOUR_AZURE_ENDPOINT",
    modelId: "gpt-4", // Optional name of the underlying model if the deployment name doesn't match the model name
    httpClient: new HttpClient() // Optional; if not provided, the HttpClient from the kernel will be used
);
```



## Retrieving chat completion services 检索聊天完成服务

Once you've added chat completion services to your kernel, you can retrieve them using the get service method. Below is an example of how you can retrieve a chat completion service from the kernel.
将聊天完成服务添加到内核后，可以使用 get service 方法检索它们。下面是如何从内核检索聊天完成服务的示例。

C#Copy  复制

```csharp
var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
```

 Tip  提示

Adding the chat completion service to the kernel is not required if you don't need to use other services in the kernel. You can use the chat completion service directly in your code.
如果您不需要在内核中使用其他服务，则不需要将聊天完成服务添加到内核。可以直接在代码中使用聊天完成服务。



## Using chat completion services 使用聊天完成服务

Now that you have a chat completion service, you can use it to generate responses from an AI agent. There are two main ways to use a chat completion service:
现在您拥有聊天完成服务，您可以使用它来生成来自 AI 代理的响应。使用聊天完成服务主要有两种方式：

- **Non-streaming**: You wait for the service to generate an entire response before returning it to the user.
  **非流式处理** ：等待服务生成整个响应，然后再将其返回给用户。
- **Streaming**: Individual chunks of the response are generated and returned to the user as they are created.
  **流式处理** ：生成响应的各个块，并在创建时返回给用户。

Below are the two ways you can use a chat completion service to generate responses.
下面是使用聊天完成服务生成响应的两种方法。



### Non-streaming chat completion 非流式聊天完成

To use non-streaming chat completion, you can use the following code to generate a response from the AI agent.
若要使用非流式聊天完成，可以使用以下代码从 AI 代理生成响应。

C#Copy  复制

```csharp
ChatHistory history = [];
history.AddUserMessage("Hello, how are you?");

var response = await chatCompletionService.GetChatMessageContentAsync(
    history,
    kernel: kernel
);
```



### Streaming chat completion 流式聊天完成

To use streaming chat completion, you can use the following code to generate a response from the AI agent.
要使用流式聊天完成，您可以使用以下代码从 AI 代理生成响应。

C#Copy  复制

```csharp
ChatHistory history = [];
history.AddUserMessage("Hello, how are you?");

var response = chatCompletionService.GetStreamingChatMessageContentsAsync(
    chatHistory: history,
    kernel: kernel
);

await foreach (var chunk in response)
{
    Console.Write(chunk);
}
```



## Next steps  后续步骤

Now that you've added chat completion services to your Semantic Kernel project, you can start creating conversations with your AI agent. To learn more about using a chat completion service, check out the following articles:
现在，您已将聊天完成服务添加到Kernel项目中，您可以开始与 AI 代理创建对话。若要详细了解如何使用聊天完成服务，请查看以下文章：