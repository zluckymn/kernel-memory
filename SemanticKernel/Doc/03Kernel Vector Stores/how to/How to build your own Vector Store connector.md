# How to build your own Vector Store connector (Preview) 如何生成自己的矢量存储连接器（预览版）

- 06/25/2025

This article provides guidance for anyone who wishes to build their own Vector Store connector. This article can be used by database providers who wish to build and maintain their own implementation, or for anyone who wishes to build and maintain an unofficial connector for a database that lacks support.
本文为任何希望构建自己的 Vector Store 连接器的人提供指导。本文可供希望构建和维护自己的实现的数据库提供商使用，或者对于希望为缺乏支持的数据库构建和维护非官方连接器的任何人。

If you wish to contribute your connector to the Semantic Kernel code base:
如果您希望将连接器贡献给Semantic Kernel代码库：

1. Create an issue in the [Semantic Kernel Github repository](https://github.com/microsoft/semantic-kernel/issues).
   在[Semantic Kernel Github 存储库](https://github.com/microsoft/semantic-kernel/issues)中创建问题。
2. Review the [Semantic Kernel contribution guidelines](https://github.com/microsoft/semantic-kernel/blob/main/CONTRIBUTING.md).
   查看[Semantic Kernel贡献指南 ](https://github.com/microsoft/semantic-kernel/blob/main/CONTRIBUTING.md)。



## Overview  概述

Vector Store connectors are implementations of the [Vector Store abstraction](https://www.nuget.org/packages/Microsoft.Extensions.VectorData.Abstractions). Some of the decisions that were made when designing the Vector Store abstraction mean that a Vector Store connector requires certain features to provide users with a good experience.
矢量存储连接器是[矢量存储抽象](https://www.nuget.org/packages/Microsoft.Extensions.VectorData.Abstractions)的实现。在设计 Vector Store 抽象时做出的一些决定意味着 Vector Store 连接器需要某些功能才能为用户提供良好的体验。

A key design decision is that the Vector Store abstraction takes a strongly typed approach to working with database records. This means that `UpsertAsync` takes a strongly typed record as input, while `GetAsync` returns a strongly typed record. The design uses C# generics to achieve the strong typing. This means that a connector has to be able to map from this data model to the storage model used by the underlying database. It also means that a connector may need to find out certain information about the record properties in order to know how to map each of these properties. E.g. some vector databases (such as Chroma, Qdrant and Weaviate) require vectors to be stored in a specific structure and non-vectors in a different structure, or require record keys to be stored in a specific field.
一个关键的设计决策是矢量存储抽象采用强类型方法来处理数据库记录。这意味着 `UpsertAsync` 将强类型记录作为输入，而 `GetAsync` 返回强类型记录。该设计使用 C# 泛型来实现强类型。这意味着连接器必须能够从此数据模型映射到底层数据库使用的存储模型。这也意味着连接器可能需要查找有关记录属性的某些信息，以便了解如何映射每个属性。例如，某些向量数据库（如 Chroma、Qdrant 和 Weaviate）要求向量存储在特定结构中，非向量存储在不同的结构中，或者要求记录键存储在特定字段中。

At the same time, the Vector Store abstraction also provides a generic data model that allows a developer to work with a database without needing to create a custom data model.
同时，矢量存储抽象还提供了一个通用数据模型，允许开发人员使用数据库而无需创建自定义数据模型。

It is important for connectors to support different types of model and provide developers with flexibility around how they use the connector. The following section deep dives into each of these requirements.
连接器支持不同类型的模型并为开发人员提供使用连接器的灵活性非常重要。以下部分将深入探讨其中每一项要求。



## Requirements  要求

In order to be considered a full implementation of the Vector Store abstractions, the following set of requirements must be met.
为了被视为矢量存储抽象的完整实现，必须满足以下一组要求。



### 1. Implement the core abstract base clases and interfaces 1. 实现核心抽象基础平台和接口

1.1 The three core abstract base classes and interfaces that need to be implemented are:
1.1 需要实现的三个核心抽象基类和接口分别是：

- Microsoft.Extensions.VectorData.VectorStore
- Microsoft.Extensions.VectorData.VectorStoreCollection<TKey, TRecord>
  Microsoft.Extensions.VectorData.VectorStoreCollection<TKey、TRecord>
- Microsoft.Extensions.VectorData.IVectorSearchable<TRecord>

Note that `VectorStoreCollection<TKey, TRecord>` implements `IVectorSearchable<TRecord>`, so only two inheriting classes are required. The following naming convention should be used:
请注意， `VectorStoreCollection<TKey, TRecord>` 实现 `IVectorSearchable<TRecord>`，因此只需要两个继承类。应使用以下命名约定：

- {database type}VectorStore : VectorStore
  {数据库类型}VectorStore ： VectorStore
- {database type}Collection<TKey, TRecord> : VectorStoreCollection<TKey, TRecord>
  {数据库类型}收藏 <TKey， TRecord> ： VectorStoreCollection<TKey， TRecord>

E.g.  例如

- MyDbVectorStore : VectorStore
  MyDbVectorStore ： VectorStore
- MyDbCollection<TKey, TRecord> : VectorStoreCollection<TKey, TRecord>
  MyDbCollection<TKey，TRecord>：VectorStoreCollection<TKey，TRecord>

The `VectorStoreCollection` implementation should accept the name of the collection as a constructor parameter and each instance of it is therefore tied to a specific collection instance in the database.
`VectorStoreCollection` 实现应接受集合的名称作为构造函数参数，因此它的每个实例都绑定到数据库中的特定集合实例。

Here follows specific requirements for individual methods on these abstract base classes and interfaces.
以下是对这些抽象基类和接口上的各个方法的具体要求。

1.2 *VectorStore.GetCollection* implementations should not do any checks to verify whether a collection exists or not. The method should simply construct a collection object and return it. The user can optionally use the `CollectionExistsAsync` method to check if the collection exists in cases where this is not known. Doing checks on each invocation of `GetCollection` may add unwanted overhead for users when they are working with a collection that they know exists.
1.2 *VectorStore.GetCollection* 实现不应执行任何检查来验证集合是否存在。 该方法应该简单地构造一个集合对象并返回它。用户可以选择使用 `CollectionExistsAsync` 方法来检查集合是否存在，以在未知的情况下。对 `GetCollection` 的每次调用进行检查可能会在用户使用他们知道存在的集合时增加不必要的开销。

1.3 *VectorStoreCollection<TKey, TRecord>.DeleteAsync* that takes a single key as input should succeed if the record does not exist and for any other failures an exception should be thrown. See the [standard exceptions](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions) section for requirements on the exception types to throw.
1.3 *VectorStoreCollection<TKey, TRecord>.DeleteAsync* 如果记录不存在，则采用单个键作为输入的记录应该成功，对于任何其他失败，应抛出异常。有关要引发的异常类型的要求，请参阅[标准异常部分 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions)。

1.4 *VectorStoreCollection<TKey, TRecord>.DeleteAsync* that takes multiple keys as input should succeed if any of the requested records do not exist and for any other failures an exception should be thrown. See the [standard exceptions](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions) section for requirements on the exception types to throw.
1.4 *VectorStoreCollection<TKey, TRecord>.DeleteAsync* 如果请求的任何记录不存在，则将多个键作为输入，则应成功，并且对于任何其他失败，应抛出异常。有关要引发的异常类型的要求，请参阅[标准异常部分 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions)。

1.5 *VectorStoreCollection<TKey, TRecord>.GetAsync* that takes a single key as input should return null and not throw if a record is not found. For any other failures an exception should be thrown. See the [standard exceptions](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions) section for requirements on the exception types to throw.
1.5 将单个键作为输入 *VectorStoreCollection<TKey, TRecord>.GetAsync* 应返回 null，如果找不到记录，则不会抛出。对于任何其他故障，应引发异常。有关要引发的异常类型的要求，请参阅[标准异常部分 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions)。

1.6 *VectorStoreCollection<TKey, TRecord>.GetAsync* that takes multiple keys as input should return the subset of records that were found and not throw if any of the requested records were not found. For any other failures an exception should be thrown. See the [standard exceptions](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions) section for requirements on the exception types to throw.
1.6 *VectorStoreCollection<TKey, TRecord>.GetAsync* 应返回找到的记录子集，如果未找到任何请求的记录，则不会抛出。对于任何其他故障，应引发异常。有关要引发的异常类型的要求，请参阅[标准异常部分 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#10-standard-exceptions)。

1.7 *VectorStoreCollection<TKey, TRecord>.GetAsync* implementations should respect the `IncludeVectors` option provided via `RecordRetrievalOptions` where possible. Vectors are often most useful in the database itself, since that is where vector comparison happens during vector searches and downloading them can be costly due to their size. There may be cases where the database doesn't support excluding vectors in which case returning them is acceptable.
1.7 *VectorStoreCollection<TKey, TRecord>.GetAsync* 实现应尽可能遵循通过 `RecordRetrievalOptions` 提供的 `IncludeVectors` 选项。向量通常在数据库本身中最有用，因为这是在向量搜索期间进行向量比较的地方，并且由于它们的大小，下载它们的成本可能很高。在某些情况下，数据库可能不支持排除向量，在这种情况下，返回它们是可以接受的。

1.8 *IVectorSearchable<TRecord>.SearchAsync<TVector>* implementations should also respect the `IncludeVectors` option provided via `VectorSearchOptions<TRecord>` where possible.
1.8 *IVectorSearchable<TRecord>.SearchAsync<TVector>* 实现还应尽可能遵守通过 `VectorSearchOptions<TRecord>` 提供的 `IncludeVectors` 选项。

1.9 *IVectorSearchable<TRecord>.SearchAsync<TVector>* implementations should simulate the `Top` and `Skip` functionality requested via `VectorSearchOptions<TRecord>` if the database does not support this natively. To simulate this behavior, the implementation should fetch a number of results equal to Top + Skip, and then skip the first Skip number of results before returning the remaining results.
如果数据库本身不支持，1.9 *IVectorSearchable<TRecord>.SearchAsync<TVector>* 实现应模拟通过 `VectorSearchOptions<TRecord>` 请求的 `Top` 和 `Skip` 功能。若要模拟此行为，实现应提取等于 Top + Skip 的结果数，然后跳过第一个 Skip 结果数，然后返回其余结果。

1.10 *IVectorSearchable<TRecord>.SearchAsync<TVector>* implementations should not require `VectorPropertyName` or `VectorProperty` to be specified if only one vector exists on the data model. In this case that single vector should automatically become the search target. If no vector or multiple vectors exists on the data model, and no `VectorPropertyName` or `VectorProperty` is provided the search method should throw.
1.10 *IVectorSearchable<TRecord>.SearchAsync<TVector>* 实现不应要求 `VectorPropertyName` 或 `VectorProperty`，如果数据模型上仅存在一个向量，则要指定。在这种情况下，单个向量应自动成为搜索目标。如果数据模型上不存在向量或多个向量，并且未提供 `VectorPropertyName` 或 `VectorProperty，` 则应引发搜索方法。

When using `VectorPropertyName`, if a user does provide this value, the expected name should be the property name from the data model and not any customized name that the property may be stored under in the database. E.g. let's say the user has a data model property called `TextEmbedding` and they decorated the property with a `JsonPropertyNameAttribute` that indicates that it should be serialized as `text_embedding`. Assuming that the database is json based, it means that the property should be stored in the database with the name `text_embedding`. When specifying the `VectorPropertyName` option, the user should always provide `TextEmbedding` as the value. This is to ensure that where different connectors are used with the same data model, the user can always use the same property names, even though the storage name of the property may be different.
使用 `VectorPropertyName` 时，如果用户确实提供了此值，则预期名称应是数据模型中的属性名称，而不是属性可能存储在数据库中的任何自定义名称。例如，假设用户有一个名为 `TextEmbedding` 的数据模型属性，他们用 `JsonPropertyNameAttribute` 修饰了该属性，该属性指示它应该序列化为 `text_embedding`。假设数据库是基于 json 的，这意味着该属性应存储在数据库中，名称为 `text_embedding`。指定 `VectorPropertyName` 选项时，用户应始终提供 `TextEmbedding` 作为值。这是为了确保在将不同的连接器用于同一数据模型时，即使属性的存储名称可能不同，用户也始终可以使用相同的属性名称。



### 2. Support data model attributes 2. 支持数据模型属性

The Vector Store abstraction allows a user to use attributes to decorate their data model to indicate the type of each property and to configure the type of indexing required for each vector property.
向量存储抽象允许用户使用属性来修饰其数据模型，以指示每个属性的类型，并配置每个向量属性所需的索引类型。

This information is typically required for
通常需要此信息

1. Mapping between a data model and the underlying database's storage model
   数据模型与底层数据库的存储模型之间的映射
2. Creating a collection / index
   创建集合/索引
3. Vector Search  矢量搜索

If the user does not provide a `VectorStoreCollectionDefinition`, this information should be read from the data model attributes using reflection. If the user did provide a `VectorStoreCollectionDefinition`, the data model should not be used as the source of truth.
如果用户未提供 `VectorStoreCollectionDefinition` ，则此信息应 使用反射从数据模型属性中读取。如果用户确实提供了 `VectorStoreCollectionDefinition` ，则不应将数据模型用作事实来源。

 Tip  提示

Refer to [Defining your data model](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model) for a detailed list of all attributes and settings that need to be supported.
请参阅 [定义数据模型 ](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/defining-your-data-model)获取需要支持的所有属性和设置的详细列表。



### 3. Support record definitions 3. 支持记录定义

As mentioned in [Support data model attributes](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#2-support-data-model-attributes) we need information about each property to build out a connector. This information can also be supplied via a `VectorStoreCollectionDefinition` and if supplied, the connector should avoid trying to read this information from the data model or try and validate that the data model matches the definition in any way.
如[支持数据模型属性](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#2-support-data-model-attributes)中所述，我们需要有关每个属性的信息才能构建连接器。此信息也可以通过 提供 `VectorStoreCollectionDefinition` ，如果提供，连接器应避免尝试从数据模型中读取此信息，或尝试验证数据模型是否以任何方式与定义匹配。

The user should be able to provide a `VectorStoreCollectionDefinition` to the `VectorStoreCollection` implementation via options.
用户应该能够向 `VectorStoreCollectionDefinition` `VectorStoreCollection` 通过选项实现。

 Tip  提示

Refer to [Defining your storage schema using a record definition](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/schema-with-record-definition) for a detailed list of all record definition settings that need to be supported.
有关需要支持的所有记录定义设置的详细列表，请参阅[使用记录定义](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/schema-with-record-definition)定义存储架构。



### 4. Collection / Index Creation 4. 集合/索引创建

4.1 A user can optionally choose an index kind and distance function for each vector property. These are specified via string based settings, but where available a connector should expect the strings that are provided as string consts on `Microsoft.Extensions.VectorData.IndexKind` and `Microsoft.Extensions.VectorData.DistanceFunction`. Where the connector requires index kinds and distance functions that are not available on the above mentioned static classes additional custom strings may be accepted.
4.1 用户可以选择为每个向量属性选择索引类型和距离函数。这些是通过基于字符串的设置指定的，但在可用的情况下，连接器应期望作为字符串 consts `Microsoft.Extensions.VectorData.IndexKind` 提供的字符串 和 `Microsoft.Extensions.VectorData.DistanceFunction` 。如果连接器需要上述静态类上不可用的索引类型和距离函数，则可以接受其他自定义字符串。

E.g. the goal is for a user to be able to specify a standard distance function, like `DotProductSimilarity` for any connector that supports this distance function, without needing to use different naming for each connector.
例如，目标是让用户能够指定标准距离函数，例如 `DotProductSimilarity` 对于任何支持此距离功能的连接器，无需使用不同的 每个连接器的命名。

C#Copy  复制

```csharp
    [VectorStoreVector(1536, DistanceFunction = DistanceFunction.DotProductSimilarity]
    public ReadOnlyMemory<float>? Embedding { get; set; }
```

4.2 A user can optionally choose whether each data property should be indexed or full text indexed. In some databases, all properties may already be filterable or full text searchable by default, however in many databases, special indexing is required to achieve this. If special indexing is required this also means that adding this indexing will most likely incur extra cost. The `IsIndexed` and `IsFullTextIndexed` settings allow a user to control whether to enable this additional indexing per property.
4.2 用户可以选择是对每个数据属性进行索引还是对全文进行索引。在某些数据库中，默认情况下，所有属性可能已经是可过滤或全文可搜索的，但是在许多数据库中，需要特殊的索引才能实现此目的。如果需要特殊索引，这也意味着添加此索引很可能会产生额外的成本。`IsIndexed` 和 `IsFullTextIndexed` 设置允许用户控制是否为每个属性启用此附加索引。



### 5. Data model validation  5. 数据模型验证

Every database doesn't support every data type. To improve the user experience it's important to validate the data types of any record properties and to do so early, e.g. when an `VectorStoreCollection` instance is constructed. This way the user will be notified of any potential failures before starting to use the database.
并非每个数据库都支持每种数据类型。为了改善用户体验，验证任何记录属性的数据类型并尽早执行此作非常重要，例如，当 `VectorStoreCollection` 实例被构造。这样，用户将在开始使用数据库之前收到任何潜在故障的通知。



### 6. Storage property naming 6. 存储属性命名

The naming conventions used for properties in code doesn't always match the preferred naming for matching fields in a database. It is therefore valuable to support customized storage names for properties. Some databases may support storage formats that already have their own mechanism for specifying storage names, e.g. when using JSON as the storage format you can use a `JsonPropertyNameAttribute` to provide a custom name.
用于代码中属性的命名约定并不总是与数据库中匹配字段的首选命名匹配。因此，支持属性的自定义存储名称非常有价值。某些数据库可能支持已经具有自己的存储名称指定机制的存储格式，例如，当使用 JSON 作为存储格式时，您可以使用 `JsonPropertyNameAttribute` 来提供自定义名称。

6.1 Where the database has a storage format that supports its own mechanism for specifying storage names, the connector should preferably use that mechanism.
6.1 如果数据库具有支持其自己的存储名称指定机制的存储格式，则连接器最好使用该机制。

6.2 Where the database does not use a storage format that supports its own mechanism for specifying storage names, the connector must support the `StorageName` settings from the data model attributes or the `VectorStoreCollectionDefinition`.
6.2 如果数据库不使用支持其自己的存储名称指定机制的存储格式，则连接器必须支持数据模型属性或 .`` `VectorStoreCollectionDefinition`



### 7. Mapper support  7. 映射器支持

Connectors should provide the ability to map between the user supplied data model and the storage model that the database requires, but should also provide some flexibility in how that mapping is done. Most connectors would typically need to support the following two mappers.
连接器应提供在用户提供的数据模型和数据库所需的存储模型之间进行映射的功能，但也应在映射方式方面提供一定的灵活性。大多数连接器通常需要支持以下两个映射器。

7.1 All connectors should come with a built in mapper that can map between the user supplied data model and the storage model required by the underlying database.
7.1 所有连接器都应附带一个内置的映射器，该映射器可以在用户提供的数据模型和底层数据库所需的存储模型之间进行映射。

7.2. All connectors should have a built in mapper that works with the `VectorStoreGenericDataModel`. See [Support GenericDataModel](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#8-support-genericdatamodel) for more information.
7.2. 所有连接器都应具有与 `VectorStoreGenericDataModel` 配合使用的内置映射器。有关详细信息，请参阅[支持 GenericDataModel](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/how-to/build-your-own-connector?pivots=programming-language-csharp#8-support-genericdatamodel)。



### 8. Support GenericDataModel 8. 支持 GenericDataModel

While it is very useful for users to be able to define their own data model, in some cases it may not be desirable, e.g. when the database schema is not known at coding time and driven by configuration.
虽然能够定义自己的数据模型对用户来说非常有用，但在某些情况下，这可能并不可取，例如，当数据库模式在编码时未知并由配置驱动时。

To support this scenario, connectors should have out of the box support for the generic data model supplied by the abstraction package: `Microsoft.Extensions.VectorData.VectorStoreGenericDataModel<TKey>`.
若要支持此方案，连接器应对抽象包提供的泛型数据模型提供现成支持： `Microsoft.Extensions.VectorData.VectorStoreGenericDataModel<TKey>` 。

In practice this means that the connector must implement a special mapper to support the generic data model. The connector should automatically use this mapper if the user specified the generic data model as their data model.
在实践中，这意味着连接器必须实现一个特殊的映射器来支持通用数据模型。如果用户将通用数据模型指定为其数据模型，则连接器应自动使用此映射器。



### 9. Divergent data model and database schema 9. 发散数据模型和数据库模式

The only divergence required to be supported by connector implementations are customizing storage property names for any properties.
连接器实现需要支持的唯一分歧是自定义任何属性的存储属性名称。

Any more complex divergence is not supported, since this causes additional complexity for filtering. E.g. if the user has a filter expression that references the data model, but the underlying schema is different to the data model, the filter expression cannot be used against the underlying schema.
不支持任何更复杂的发散，因为这会导致过滤更加复杂。例如，如果用户具有引用数据模型的筛选表达式，但基础架构与数据模型不同，则不能将筛选表达式用于基础架构。



### 10. Standard Exceptions  10. 标准例外情况

The database operation methods provided by the connector should throw a set of standard exceptions so that users of the abstraction know what exceptions they need to handle, instead of having to catch a different set for each provider. E.g. if the underlying database client throws a `MyDBClientException` when a call to the database fails, this should be caught and wrapped in a `VectorStoreOperationException`, preferably preserving the original exception as an inner exception.
连接器提供的数据库作方法应抛出一组标准异常，以便抽象的用户知道他们需要处理哪些异常，而不必为每个提供程序捕获不同的异常集。例如，如果底层数据库客户端在对数据库的调用失败时抛出 `MyDBClientException`，则应捕获此异常并将其包装在 `VectorStoreOperationException` 中，最好将原始异常保留为内部异常。

11.1 For failures relating to service call or database failures the connector should throw: `Microsoft.Extensions.VectorData.VectorStoreOperationException`
11.1 对于与服务调用或数据库故障相关的故障，连接器应抛出： `Microsoft.Extensions.VectorData.VectorStoreOperationException`

11.2 For mapping failures, the connector should throw: `Microsoft.Extensions.VectorData.VectorStoreRecordMappingException`
11.2 对于映射失败，连接器应抛出： `Microsoft.Extensions.VectorData.VectorStoreRecordMappingException`

11.3 For cases where a certain setting or feature is not supported, e.g. an unsupported index type, use: `System.NotSupportedException`.
11.3 对于不支持某些设置或功能的情况，例如不受支持的索引类型，请使用： `System.NotSupportedException` 中。

11.4 In addition, use `System.ArgumentException`, `System.ArgumentNullException` for argument validation.
11.4 此外，使用 `System.ArgumentException`、`System.ArgumentNullException` 进行参数验证。



### 11. Batching  11. 批处理

The `VectorStoreCollection` abstract base class includes batching overloads for Get, Upsert and Delete. Not all underlying database clients may have the same level of support for batching.
`VectorStoreCollection` 抽象基类包括 Get、Upsert 和 Delete 的批处理重载。并非所有基础数据库客户端都对批处理具有相同级别的支持。

The base batch method implementations on `VectorStoreCollection` calls the abstract non-batch implementations in serial. If the database supports batching natively, these base batch implementations should be overridden and implemented using the native database support.
`VectorStoreCollection` 上的基本批处理方法实现以串行方式调用抽象非批处理实现。如果数据库支持本机批处理，则应使用本机数据库支持重写和实现这些基本批处理实现。



## Recommended common patterns and practices 建议的常见模式和做法

1. Keep `VectorStore` and `VectorStoreCollection` implementations sealed. It is recommended to use a decorator pattern to override a default vector store behaviour.
   保持 `VectorStore` 和 `VectorStoreCollection` 实现密封。建议使用装饰器模式来覆盖默认的向量存储行为。
2. Always use options classes for optional settings with smart defaults.
   始终将选项类用于具有智能默认值的可选设置。
3. Keep required parameters on the main signature and move optional parameters to options.
   将必需参数保留在主签名上，并将可选参数移至选项。

Here is an example of an `VectorStoreCollection` constructor following this pattern.
下面是遵循此模式的 `VectorStoreCollection` 构造函数的示例。

C#Copy  复制

```csharp
public sealed class MyDBCollection<TRecord> : VectorStoreCollection<string, TRecord>
{
    public MyDBCollection(MyDBClient myDBClient, string collectionName, MyDBCollectionOptions<TRecord>? options = default)
    {
    }

    ...
}

public class MyDBCollectionOptions<TRecord> : VectorStoreCollectionOptions
{
}
```



## SDK Changes  SDK 更改

Please also see the following articles for a history of changes to the SDK and therefore implementation requirements:
另请参阅以下文章，了解 SDK 更改的历史记录以及实现要求：

1. [Vector Store Changes March 2025
   矢量商店变更 2025 年 3 月](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/vectorstore-march-2025)
2. [Vector Store Changes April 2025
   矢量商店 2025 年 4 月更改](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/vectorstore-april-2025)
3. [Vector Store Changes May 2025
   矢量商店 2025 年 5 月更改](https://learn.microsoft.com/en-us/semantic-kernel/support/migration/vectorstore-may-2025)



## Documentation  文档

To share the features and limitations of your implementation, you can contribute a documentation page to this Microsoft Learn website. See [here](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/) for the documentation on the existing connectors.
若要共享实现的功能和限制，可以向此 Microsoft Learn 网站提供文档页。看[这里](https://learn.microsoft.com/en-us/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors/) 有关现有连接器的文档。

To create your page, create a pull request on the [Semantic Kernel docs Github repository](https://github.com/MicrosoftDocs/semantic-kernel-docs). Use the pages in the following folder as examples: [Out-of-the-box connectors](https://github.com/MicrosoftDocs/semantic-kernel-docs/tree/main/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors)
要创建页面，请在 [Semantic Kernel docs Github 存储库](https://github.com/MicrosoftDocs/semantic-kernel-docs)上创建拉取请求。使用以下文件夹中的页面作为示例：现[成连接器](https://github.com/MicrosoftDocs/semantic-kernel-docs/tree/main/semantic-kernel/concepts/vector-store-connectors/out-of-the-box-connectors)

Areas to cover:  涵盖领域：

1. An `Overview` with a standard table describing the main features of the connector.
   `概述 `，其中包含描述连接器主要功能的标准表格。
2. An optional `Limitations` section with any limitations for your connector.
   可选的 `“限制”` 部分，其中包含连接器的任何限制。
3. A `Getting started` section that describes how to import your nuget and construct your `VectorStore` and `VectorStoreCollection`
   介绍如何导入 nuget 和构造 `VectorStore` 和 `VectorStoreCollection` 的`入门`部分
4. A `Data mapping` section showing the connector's default data mapping mechanism to the database storage model, including any property renaming it may support.
   数据`映射`部分显示连接器与数据库存储模型的默认数据映射机制，包括它可能支持的任何属性重命名。
5. Information about additional features your connector supports.
   有关连接器支持的其他功能的信息。