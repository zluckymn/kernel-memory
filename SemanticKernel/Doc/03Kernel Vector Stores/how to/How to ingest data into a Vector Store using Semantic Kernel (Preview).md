# How to ingest data into a Vector Store using Semantic Kernel (Preview) 如何使用Semantic Kernel将数据引入矢量存储（预览版）

- 05/20/2025

This article will demonstrate how to create an application to
本文将演示如何创建应用程序

1. Take text from each paragraph in a Microsoft Word document
   从 Microsoft Word 文档中的每个段落中获取文本
2. Generate an embedding for each paragraph
   为每个段落生成嵌入
3. Upsert the text, embedding and a reference to the original location into a Redis instance.
   将文本、嵌入和对原始位置的引用更新插入到 Redis 实例中。



## Prerequisites  先决条件

For this sample you will need
对于此示例，您将需要

1. An embedding generation model hosted in Azure or another provider of your choice.
   托管在 Azure 或所选的其他提供商中的嵌入生成模型。
2. An instance of Redis or Docker Desktop so that you can run Redis locally.
   Redis 或 Docker Desktop 的实例，以便您可以在本地运行 Redis。
3. A Word document to parse and load. Here is a zip containing a sample Word document you can download and use: [vector-store-data-ingestion-input.zip](https://learn.microsoft.com/en-us/semantic-kernel/media/vector-store-data-ingestion-input.zip).
   要解析和加载的 Word 文档。这是一个 zip，其中包含您可以下载和使用的示例 Word 文档：[vector-store-data-ingestion-input.zip](https://learn.microsoft.com/en-us/semantic-kernel/media/vector-store-data-ingestion-input.zip)。



## Setup Redis  设置 Redis

If you already have a Redis instance you can use that. If you prefer to test your project locally you can easily start a Redis container using docker.
如果您已经有一个 Redis 实例，则可以使用它。如果您更喜欢在本地测试您的项目，您可以使用 docker 轻松启动 Redis 容器。

Copy  复制

```
docker run -d --name redis-stack -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

To verify that it is running successfully, visit http://localhost:8001/redis-stack/browser in your browser.
要验证它是否成功运行，请访问浏览器中的 http://localhost:8001/redis-stack/browser。

The rest of these instructions will assume that you are using this container using the above settings.
这些说明的其余部分将假设您使用上述设置使用此容器。



## Create your project  创建项目

Create a new project and add nuget package references for the Redis connector from Semantic Kernel, the open xml package to read the word document with and the OpenAI connector from Semantic Kernel for generating embeddings.
创建一个新项目，并为Semantic Kernel中的 Redis 连接器添加 nuget 包引用、用于读取 word 文档的开放 xml 包以及用于生成嵌入的Semantic Kernel中的 OpenAI 连接器。

.NET CLICopy  复制

```dotnetcli
dotnet new console --framework net8.0 --name SKVectorIngest
cd SKVectorIngest
dotnet add package Microsoft.SemanticKernel.Connectors.AzureOpenAI
dotnet add package Microsoft.SemanticKernel.Connectors.Redis --prerelease
dotnet add package DocumentFormat.OpenXml
```



## Add a data model  添加数据模型

To upload data we need to first describe what format the data should have in the database. We can do this by creating a data model with attributes that describe the function of each property.
要上传数据，我们需要首先描述数据在数据库中应该具有什么格式。我们可以通过创建一个具有描述每个属性功能的属性的数据模型来做到这一点。

Add a new file to the project called `TextParagraph.cs` and add the following model to it.
向项目添加一个名为 `TextParagraph.cs` 的新文件，并向其添加以下模型。

C#Copy  复制

```csharp
using Microsoft.Extensions.VectorData;

namespace SKVectorIngest;

internal class TextParagraph
{
    /// <summary>A unique key for the text paragraph.</summary>
    [VectorStoreKey]
    public required string Key { get; init; }

    /// <summary>A uri that points at the original location of the document containing the text.</summary>
    [VectorStoreData]
    public required string DocumentUri { get; init; }

    /// <summary>The id of the paragraph from the document containing the text.</summary>
    [VectorStoreData]
    public required string ParagraphId { get; init; }

    /// <summary>The text of the paragraph.</summary>
    [VectorStoreData]
    public required string Text { get; init; }

    /// <summary>The embedding generated from the Text.</summary>
    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> TextEmbedding { get; set; }
}
```

Note that we are passing the value `1536` to the `VectorStoreVectorAttribute`. This is the dimension size of the vector and has to match the size of vector that your chosen embedding generator produces.
请注意，我们将值 `1536` 传递给 `VectorStoreVectorAttribute`。这是向量的尺寸大小，并且必须与所选嵌入生成器生成的向量大小相匹配。

 Tip  提示

For more information on how to annotate your data model and what additional options are available for each attribute, refer to [defining your data model](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model).
有关如何注释数据模型以及每个属性可用的其他选项的更多信息，请参阅 [定义数据模型 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model).



## Read the paragraphs in the document 阅读文档中的段落

We need some code to read the word document and find the text of each paragraph in it.
我们需要一些代码来阅读 word 文档并找到其中每个段落的文本。

Add a new file to the project called `DocumentReader.cs` and add the following class to read the paragraphs from a document.
向项目添加一个名为 `DocumentReader.cs` 的新文件，并添加以下类以读取文档中的段落。

C#Copy  复制

```csharp
using System.Text;
using System.Xml;
using DocumentFormat.OpenXml.Packaging;

namespace SKVectorIngest;

internal class DocumentReader
{
    public static IEnumerable<TextParagraph> ReadParagraphs(Stream documentContents, string documentUri)
    {
        // Open the document.
        using WordprocessingDocument wordDoc = WordprocessingDocument.Open(documentContents, false);
        if (wordDoc.MainDocumentPart == null)
        {
            yield break;
        }

        // Create an XmlDocument to hold the document contents and load the document contents into the XmlDocument.
        XmlDocument xmlDoc = new XmlDocument();
        XmlNamespaceManager nsManager = new XmlNamespaceManager(xmlDoc.NameTable);
        nsManager.AddNamespace("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
        nsManager.AddNamespace("w14", "http://schemas.microsoft.com/office/word/2010/wordml");

        xmlDoc.Load(wordDoc.MainDocumentPart.GetStream());

        // Select all paragraphs in the document and break if none found.
        XmlNodeList? paragraphs = xmlDoc.SelectNodes("//w:p", nsManager);
        if (paragraphs == null)
        {
            yield break;
        }

        // Iterate over each paragraph.
        foreach (XmlNode paragraph in paragraphs)
        {
            // Select all text nodes in the paragraph and continue if none found.
            XmlNodeList? texts = paragraph.SelectNodes(".//w:t", nsManager);
            if (texts == null)
            {
                continue;
            }

            // Combine all non-empty text nodes into a single string.
            var textBuilder = new StringBuilder();
            foreach (XmlNode text in texts)
            {
                if (!string.IsNullOrWhiteSpace(text.InnerText))
                {
                    textBuilder.Append(text.InnerText);
                }
            }

            // Yield a new TextParagraph if the combined text is not empty.
            var combinedText = textBuilder.ToString();
            if (!string.IsNullOrWhiteSpace(combinedText))
            {
                Console.WriteLine("Found paragraph:");
                Console.WriteLine(combinedText);
                Console.WriteLine();

                yield return new TextParagraph
                {
                    Key = Guid.NewGuid().ToString(),
                    DocumentUri = documentUri,
                    ParagraphId = paragraph.Attributes?["w14:paraId"]?.Value ?? string.Empty,
                    Text = combinedText
                };
            }
        }
    }
}
```



## Generate embeddings and upload the data 生成嵌入并上传数据

We will need some code to generate embeddings and upload the paragraphs to Redis. Let's do this in a separate class.
我们将需要一些代码来生成嵌入并将段落上传到 Redis。让我们在单独的类中执行此作。

Add a new file called `DataUploader.cs` and add the following class to it.
添加一个名为 `DataUploader.cs` 的新文件，并向其中添加以下类。

C#Copy  复制

```csharp
#pragma warning disable SKEXP0001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace SKVectorIngest;

internal class DataUploader(VectorStore vectorStore, ITextEmbeddingGenerationService textEmbeddingGenerationService)
{
    /// <summary>
    /// Generate an embedding for each text paragraph and upload it to the specified collection.
    /// </summary>
    /// <param name="collectionName">The name of the collection to upload the text paragraphs to.</param>
    /// <param name="textParagraphs">The text paragraphs to upload.</param>
    /// <returns>An async task.</returns>
    public async Task GenerateEmbeddingsAndUpload(string collectionName, IEnumerable<TextParagraph> textParagraphs)
    {
        var collection = vectorStore.GetCollection<string, TextParagraph>(collectionName);
        await collection.EnsureCollectionExistsAsync();

        foreach (var paragraph in textParagraphs)
        {
            // Generate the text embedding.
            Console.WriteLine($"Generating embedding for paragraph: {paragraph.ParagraphId}");
            paragraph.TextEmbedding = await textEmbeddingGenerationService.GenerateEmbeddingAsync(paragraph.Text);

            // Upload the text paragraph.
            Console.WriteLine($"Upserting paragraph: {paragraph.ParagraphId}");
            await collection.UpsertAsync(paragraph);

            Console.WriteLine();
        }
    }
}
```



## Put it all together  把它们放在一起

Finally, we need to put together the different pieces. In this example, we will use the Semantic Kernel dependency injection container but it is also possible to use any `IServiceCollection` based container.
最后，我们需要将不同的部分放在一起。在此示例中，我们将使用Semantic Kernel依赖项注入容器，但也可以使用任何基于 `IServiceCollection` 的容器。

Add the following code to your `Program.cs` file to create the container, register the Redis vector store and register the embedding service. Make sure to replace the text embedding generation settings with your own values.
将以下代码添加到 `Program.cs` 文件以创建容器、注册 Redis 向量存储并注册嵌入服务。确保将文本嵌入生成设置替换为您自己的值。

C#Copy  复制

```csharp
#pragma warning disable SKEXP0010 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
#pragma warning disable SKEXP0020 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using SKVectorIngest;

// Replace with your values.
var deploymentName = "text-embedding-ada-002";
var endpoint = "https://sksample.openai.azure.com/";
var apiKey = "your-api-key";

// Register Azure OpenAI text embedding generation service and Redis vector store.
var builder = Kernel.CreateBuilder()
    .AddAzureOpenAITextEmbeddingGeneration(deploymentName, endpoint, apiKey)

builder.Services
    .AddRedisVectorStore("localhost:6379");

// Register the data uploader.
builder.Services.AddSingleton<DataUploader>();

// Build the kernel and get the data uploader.
var kernel = builder.Build();
var dataUploader = kernel.Services.GetRequiredService<DataUploader>();
```

As a last step, we want to read the paragraphs from our word document, and call the data uploader to generate the embeddings and upload the paragraphs.
最后一步，我们想从 word 文档中读取段落，并调用数据上传器来生成嵌入并上传段落。

C#Copy  复制

```csharp
// Load the data.
var textParagraphs = DocumentReader.ReadParagraphs(
    new FileStream(
        "vector-store-data-ingestion-input.docx",
        FileMode.Open),
    "file:///c:/vector-store-data-ingestion-input.docx");

await dataUploader.GenerateEmbeddingsAndUpload(
    "sk-documentation",
    textParagraphs);
```



## See your data in Redis 在 Redis 中查看数据

Navigate to the Redis stack browser, e.g. http://localhost:8001/redis-stack/browser where you should now be able to see your uploaded paragraphs. Here is an example of what you should see for one of the uploaded paragraphs.
导航到 Redis 堆栈浏览器，例如 http://localhost:8001/redis-stack/browser 您现在应该能够看到上传的段落。以下是您应该看到的上传段落之一的示例。

JSONCopy  复制

```json
{
    "DocumentUri" : "file:///c:/vector-store-data-ingestion-input.docx",
    "ParagraphId" : "14CA7304",
    "Text" : "Version 1.0+ support across C#, Python, and Java means it’s reliable, committed to non breaking changes. Any existing chat-based APIs are easily expanded to support additional modalities like voice and video.",
    "TextEmbedding" : [...]
}
```