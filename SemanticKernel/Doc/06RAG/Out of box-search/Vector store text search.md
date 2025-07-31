## Overview  概述

The Vector Store Text Search implementation uses the [Vector Store Connectors](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/) to retrieve search results. This means you can use Vector Store Text Search with any Vector Store which Semantic Kernel supports and any implementation of [Microsoft.Extensions.VectorData.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.VectorData.Abstractions).
矢量存储文本搜索实现使用[矢量存储连接器](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/)来检索搜索结果。这意味着可以将矢量存储文本搜索与Semantic Kernel支持的任何矢量存储以及 [Microsoft.Extensions.VectorData.Abstractions](https://www.nuget.org/packages/Microsoft.Extensions.VectorData.Abstractions) 的任何实现一起使用。



## Limitations  局限性

See the limitations listed for the [Vector Store connector](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/) you are using.
请参阅您正在使用的[矢量存储连接器](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/)列出的限制。



## Getting started  开始

The sample below shows how to use an in-memory vector store to create a `VectorStoreTextSearch` and use it to perform a text search.
下面的示例演示如何使用内存中矢量存储创建 `VectorStoreTextSearch` 并使用它来执行文本搜索。

C#Copy  复制

```csharp
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

// Create an embedding generation service.
var textEmbeddingGeneration = new OpenAITextEmbeddingGenerationService(
        modelId: TestConfiguration.OpenAI.EmbeddingModelId,
        apiKey: TestConfiguration.OpenAI.ApiKey);

// Construct an InMemory vector store.
var vectorStore = new InMemoryVectorStore();
var collectionName = "records";

// Get and create collection if it doesn't exist.
var recordCollection = vectorStore.GetCollection<TKey, TRecord>(collectionName);
await recordCollection.EnsureCollectionExistsAsync().ConfigureAwait(false);

// TODO populate the record collection with your test data
// Example https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/Concepts/Search/VectorStore_TextSearch.cs

// Create a text search instance using the InMemory vector store.
var textSearch = new VectorStoreTextSearch<DataModel>(recordCollection, textEmbeddingGeneration);

// Search and return results as TextSearchResult items
var query = "What is the Semantic Kernel?";
KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = 2, Skip = 0 });
Console.WriteLine("\n--- Text Search Results ---\n");
await foreach (TextSearchResult result in textResults.Results)
{
    Console.WriteLine($"Name:  {result.Name}");
    Console.WriteLine($"Value: {result.Value}");
    Console.WriteLine($"Link:  {result.Link}");
}
```



## Next steps  后续步骤

The following sections of the documentation show you how to:
文档的以下部分向您展示了如何：

1. Create a [plugin](https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-plugins) and use it for Retrieval Augmented Generation (RAG).
   创建一个[插件](https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-plugins)并将其用于检索增强生成 （RAG）。
2. Use text search together with [function calling](https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-function-calling).
   将文本搜索与[函数调用](https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-function-calling)一起使用。
3. Learn more about using [vector stores](https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-vector-stores) for text search.
   了解有关使用[矢量存储](https://learn.microsoft.com/en-us/semantic-kernel/concepts/text-search/text-search-vector-stores)进行文本搜索的更多信息。

   

抽象 