# Hybrid search using Semantic Kernel Vector Store connectors (Preview) 使用Semantic Kernel矢量存储连接器的混合搜索（预览版）

- 06/25/2025





Semantic Kernel provides hybrid search capabilities as part of its Vector Store abstractions. This supports filtering and many other options, which this article will explain in more detail.
Semantic Kernel提供混合搜索功能作为其向量存储抽象的一部分。这支持过滤和许多其他选项，本文将更详细地解释。

Currently the type of hybrid search supported is based on a vector search, plus a keyword search, both of which are executed in parallel, after which a union of the two result sets are returned. Sparse vector based hybrid search is not currently supported.
目前支持的混合搜索类型基于向量搜索和关键字搜索，两者并行执行，然后返回两个结果集的并集。当前不支持基于稀疏向量的混合搜索。

To execute a hybrid search, your database schema needs to have a vector field and a string field with full text search capabilities enabled. If you are creating a collection using the Semantic Kernel vector storage connectors, make sure to enable the `IsFullTextIndexed` option on the string field that you want to target for the keyword search.
要执行混合搜索，您的数据库架构需要具有一个矢量字段和一个字符串字段，并启用了全文搜索功能。如果要使用Semantic Kernel矢量存储连接器创建集合，请确保在要作为关键字搜索目标的字符串字段上启用 `IsFullTextIndexed` 选项。

 Tip  提示

For more information on how to enable `IsFullTextIndexed` refer to [VectorStoreDataAttribute parameters](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model#vectorstoredataattribute-parameters) or [VectorStoreDataProperty configuration settings](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/schema-with-record-definition#vectorstoredataproperty-configuration-settings)
有关如何启用 `IsFullTextIndexed` 的更多信息，请参阅 [VectorStoreDataAttribute 参数](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model#vectorstoredataattribute-parameters)或 [VectorStoreDataProperty 配置设置](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/schema-with-record-definition#vectorstoredataproperty-configuration-settings)



## Hybrid Search  混合搜索

The `HybridSearchAsync` method allows searching using a vector and an `ICollection` of string keywords. It also takes an optional `HybridSearchOptions<TRecord>` class as input. This method is available on the following interface:
`HybridSearchAsync` 方法允许使用字符串关键字的向量和 `ICollection` 进行搜索。它还采用可选的 `HybridSearchOptions<TRecord>` 类作为输入。此方法在以下接口上可用：

1. `IKeywordHybridSearchable<TRecord>`

Only connectors for databases that currently support vector plus keyword hybrid search are implementing this interface.
只有当前支持向量加关键字混合搜索的数据库的连接器才能实现此接口。

Assuming you have a collection that already contains data, you can easily do a hybrid search on it. Here is an example using Qdrant.
假设您有一个已包含数据的集合，您可以轻松地对其进行混合搜索。下面是使用 Qdrant 的示例。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;

// Placeholder embedding generation method.
async Task<ReadOnlyMemory<float>> GenerateEmbeddingAsync(string textToVectorize)
{
    // your logic here
}

// Create a Qdrant VectorStore object and choose an existing collection that already contains records.
VectorStore vectorStore = new QdrantVectorStore(new QdrantClient("localhost"), ownsClient: true);
IKeywordHybridSearchable<Hotel> collection = (IKeywordHybridSearchable<Hotel>)vectorStore.GetCollection<ulong, Hotel>("skhotels");

// Generate a vector for your search text, using your chosen embedding generation implementation.
ReadOnlyMemory<float> searchVector = await GenerateEmbeddingAsync("I'm looking for a hotel where customer happiness is the priority.");

// Do the search, passing an options object with a Top value to limit results to the single top match.
var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 1);

// Inspect the returned hotel.
await foreach (var record in searchResult)
{
    Console.WriteLine("Found hotel description: " + record.Record.Description);
    Console.WriteLine("Found record score: " + record.Score);
}
```

 Tip  提示

For more information on how to generate embeddings see [embedding generation](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/embedding-generation).
有关如何生成嵌入的详细信息，请参阅[嵌入生成 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/embedding-generation)。



## Supported Vector Types  支持的矢量类型

`HybridSearchAsync` takes a generic type as the vector parameter. The types of vectors supported by each data store vary. See [the documentation for each connector](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/) for the list of supported vector types.
`HybridSearchAsync` 采用泛型类型作为向量参数。每个数据存储支持的向量类型各不相同。有关支持的矢量类型列表，请参阅[每个连接器的文档 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/)。

It is also important for the search vector type to match the target vector that is being searched, e.g. if you have two vectors on the same record with different vector types, make sure that the search vector you supply matches the type of the specific vector you are targeting. See [VectorProperty and AdditionalProperty](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/hybrid-search?pivots=programming-language-csharp#vectorproperty-and-additionalproperty) for how to pick a target vector if you have more than one per record.
搜索向量类型与正在搜索的目标向量匹配也很重要，例如，如果同一记录上有两个具有不同向量类型的向量，请确保您提供的搜索向量与您所定位的特定向量的类型匹配。请参阅 [VectorProperty 和 AdditionalProperty](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/hybrid-search?pivots=programming-language-csharp#vectorproperty-and-additionalproperty)，了解如何在每条记录有多个目标向量时选择目标向量。



## Hybrid Search Options  混合搜索选项

The following options can be provided using the `HybridSearchOptions<TRecord>` class.
可以使用 `HybridSearchOptions<TRecord>` 类提供以下选项。



### VectorProperty and AdditionalProperty VectorProperty 和 AdditionalProperty

The `VectorProperty` and `AdditionalProperty` options can be used to specify the vector property and full text search property to target during the search.
`VectorProperty` 和 `AdditionalProperty` 选项可用于指定在搜索期间要定位的矢量属性和全文搜索属性。

If no `VectorProperty` is provided and the data model contains only one vector, that vector will be used. If the data model contains no vector or multiple vectors and `VectorProperty` is not provided, the search method will throw.
如果未提供 `VectorProperty`，并且数据模型仅包含一个向量，则将使用该向量。如果数据模型不包含向量或多个向量，并且未提供 `VectorProperty`，则搜索方法将引发。

If no `AdditionalProperty` is provided and the data model contains only one full text search property, that property will be used. If the data model contains no full text search property or multiple full text search properties and `AdditionalProperty` is not provided, the search method will throw.
如果未提供 `AdditionalProperty`，并且数据模型仅包含一个全文搜索属性，则将使用该属性。如果数据模型不包含全文搜索属性或多个全文搜索属性，并且未提供 `AdditionalProperty`，则搜索方法将引发。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.Connectors.Qdrant;
using Microsoft.Extensions.VectorData;
using Qdrant.Client;

var vectorStore = new QdrantVectorStore(new QdrantClient("localhost"), ownsClient: true);
var collection = (IKeywordHybridSearchable<Product>)vectorStore.GetCollection<ulong, Product>("skproducts");

// Create the hybrid search options and indicate that we want
// to search the DescriptionEmbedding vector property and the
// Description full text search property.
var hybridSearchOptions = new HybridSearchOptions<Product>
{
    VectorProperty = r => r.DescriptionEmbedding,
    AdditionalProperty = r => r.Description
};

// This snippet assumes searchVector is already provided, having been created using the embedding model of your choice.
var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 3, hybridSearchOptions);

public sealed class Product
{
    [VectorStoreKey]
    public int Key { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Name { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Description { get; set; }

    [VectorStoreData]
    public List<string> FeatureList { get; set; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> DescriptionEmbedding { get; set; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> FeatureListEmbedding { get; set; }
}
```



### Top and Skip  顶部和跳过

The `Top` and `Skip` options allow you to limit the number of results to the Top n results and to skip a number of results from the top of the resultset. Top and Skip can be used to do paging if you wish to retrieve a large number of results using separate calls.
“` 顶部 `”和 `“跳过”` 选项允许您将结果数限制为“前 n 个”结果，并从结果集顶部跳过多个结果。如果您希望使用单独的调用检索大量结果，则可以使用 Top 和 Skip 进行分页。

C#Copy  复制

```csharp
// Create the vector search options and indicate that we want to skip the first 40 results and then pass 20 to search to get the next 20.
var hybridSearchOptions = new HybridSearchOptions<Product>
{
    Skip = 40
};

// This snippet assumes searchVector is already provided, having been created using the embedding model of your choice.
var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 20, hybridSearchOptions);

// Iterate over the search results.
await foreach (var result in searchResult)
{
    Console.WriteLine(result.Record.Description);
}
```

The default values for `Skip` is 0.
`跳过的`默认值为 0。



### IncludeVectors  包含向量

The `IncludeVectors` option allows you to specify whether you wish to return vectors in the search results. If `false`, the vector properties on the returned model will be left null. Using `false` can significantly reduce the amount of data retrieved from the vector store during search, making searches more efficient.
`IncludeVectors` 选项允许您指定是否希望在搜索结果中返回向量。如果为 `false`，则返回模型上的向量属性将保持为 null。使用 `false` 可以显着减少搜索过程中从向量存储中检索到的数据量，从而使搜索更加高效。

The default value for `IncludeVectors` is `false`.
`IncludeVectors` 的默认值为 `false`。

C#Copy  复制

```csharp
// Create the hybrid search options and indicate that we want to include vectors in the search results.
var hybridSearchOptions = new HybridSearchOptions<Product>
{
    IncludeVectors = true
};

// This snippet assumes searchVector is already provided, having been created using the embedding model of your choice.
var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 3, hybridSearchOptions);

// Iterate over the search results.
await foreach (var result in searchResult)
{
    Console.WriteLine(result.Record.FeatureList);
}
```



### Filter  滤波器

The vector search filter option can be used to provide a filter for filtering the records in the chosen collection before applying the vector search.
矢量搜索筛选器选项可用于提供筛选器，用于在应用矢量搜索之前筛选所选集合中的记录。

This has multiple benefits:
这有多种好处：

- Reduce latency and processing cost, since only records remaining after filtering need to be compared with the search vector and therefore fewer vector comparisons have to be done.
  减少延迟和处理成本，因为只需要将过滤后剩余的记录与搜索向量进行比较，因此需要进行的向量比较更少。
- Limit the resultset for e.g. access control purposes, by excluding data that the user shouldn't have access to.
  通过排除用户不应访问的数据来限制结果集，例如出于访问控制目的。

Note that in order for fields to be used for filtering, many vector stores require those fields to be indexed first. Some vector stores will allow filtering using any field, but may optionally allow indexing to improve filtering performance.
请注意，为了使字段用于筛选，许多向量存储要求首先为这些字段编制索引。某些矢量存储将允许使用任何字段进行筛选，但可以选择允许索引以提高筛选性能。

If creating a collection via the Semantic Kernel vector store abstractions and you wish to enable filtering on a field, set the `IsFilterable` property to true when defining your data model or when creating your record definition.
如果通过Semantic Kernel向量存储抽象创建集合，并且希望在字段上启用筛选，请在定义数据模型或创建记录定义时将 `IsFilterable` 属性设置为 true。

 Tip  提示

For more information on how to set the `IsFilterable` property, refer to [VectorStoreRecordDataAttribute parameters](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model#vectorstorerecorddatafield-parameters) or [VectorStoreRecordDataField configuration settings](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/schema-with-record-definition).
有关如何设置 `IsFilterable` 属性的更多信息，请参阅 [VectorStoreRecordDataAttribute 参数](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model#vectorstorerecorddatafield-parameters)或 [VectorStoreRecordDataField 配置设置 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/schema-with-record-definition)。

Filters are expressed using LINQ expressions based on the type of the data model. The set of LINQ expressions supported will vary depending on the functionality supported by each database, but all databases support a broad base of common expressions, e.g. equals, not equals, and, or, etc.
筛选器根据数据模型的类型使用 LINQ 表达式表示。支持的 LINQ 表达式集将因每个数据库支持的功能而异，但所有数据库都支持广泛的通用表达式，例如 equals、not equals 和 or 等。

C#Copy  复制

```csharp
// Create the hybrid search options and set the filter on the options.
var hybridSearchOptions = new HybridSearchOptions<Glossary>
{
    Filter = r => r.Category == "External Definitions" && r.Tags.Contains("memory")
};

// This snippet assumes searchVector is already provided, having been created using the embedding model of your choice.
var searchResult = collection.HybridSearchAsync(searchVector, ["happiness", "hotel", "customer"], top: 3, hybridSearchOptions);

// Iterate over the search results.
await foreach (var result in searchResult)
{
    Console.WriteLine(result.Record.Definition);
}

sealed class Glossary
{
    [VectorStoreKey]
    public ulong Key { get; set; }

    // Category is marked as indexed, since we want to filter using this property.
    [VectorStoreData(IsIndexed = true)]
    public string Category { get; set; }

    // Tags is marked as indexed, since we want to filter using this property.
    [VectorStoreData(IsIndexed = true)]
    public List<string> Tags { get; set; }

    [VectorStoreData]
    public string Term { get; set; }

    [VectorStoreData(IsFullTextIndexed = true)]
    public string Definition { get; set; }

    [VectorStoreVector(1536)]
    public ReadOnlyMemory<float> DefinitionEmbedding { get; set; }
}
```