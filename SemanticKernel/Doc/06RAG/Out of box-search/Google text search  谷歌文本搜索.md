 Warning  警告

The Semantic Kernel Text Search functionality is in preview, and improvements that require breaking changes may still occur in limited circumstances before release.
Semantic Kernel文本搜索功能处于预览阶段，在发布前的有限情况下，需要重大更改的改进仍可能发生。



## Overview  概述

The Google Text Search implementation uses [Google Custom Search](https://developers.google.com/custom-search) to retrieve search results. You must provide your own Google Search Api Key and Search Engine Id to use this component.
Google 文本搜索实现使用 [Google 自定义搜索](https://developers.google.com/custom-search)来检索搜索结果。您必须提供自己的 Google 搜索 API 密钥和搜索引擎 ID 才能使用此组件。



## Limitations  局限性

  展开表

| Feature Area  功能区                      | Support  支持                                                |
| :---------------------------------------- | :----------------------------------------------------------- |
| Search API  搜索 API                      | [Google Custom Search API](https://developers.google.com/custom-search/v1/reference/rest/v1/cse) only. 仅限 [Google 自定义搜索 API](https://developers.google.com/custom-search/v1/reference/rest/v1/cse)。 |
| Supported filter clauses 支持的过滤器子句 | Only "equal to" filter clauses are supported. 仅支持“等于”筛选子句。 |
| Supported filter keys  支持的筛选器键     | Following parameters are supported: "cr", "dateRestrict", "exactTerms", "excludeTerms", "filter", "gl", "hl", "linkSite", "lr", "orTerms", "rights", "siteSearch". For more information see [parameters](https://developers.google.com/custom-search/v1/reference/rest/v1/cse/list). 支持以下参数：“cr”、“dateRestrict”、“exactTerms”、“excludeTerms”、“filter”、“gl”、“hl”、“linkSite”、“lr”、“orTerms”、“rights”、“siteSearch”。有关详细信息，请参阅[参数 ](https://developers.google.com/custom-search/v1/reference/rest/v1/cse/list)。 |

 Tip  提示

Follow this link for more information on how [search is performed](https://developers.google.com/custom-search/v1/reference/rest/v1/cse/list)
点击此链接，了解有关如何[执行搜索](https://developers.google.com/custom-search/v1/reference/rest/v1/cse/list)的更多信息



## Getting started  开始

The sample below shows how to create a `GoogleTextSearch` and use it to perform a text search.
下面的示例展示了如何创建 `GoogleTextSearch` 并使用它来执行文本搜索。

C#Copy  复制

```csharp
using Google.Apis.Http;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Google;

// Create an ITextSearch instance using Google search
var textSearch = new GoogleTextSearch(
    initializer: new() { ApiKey = "<Your Google API Key>", HttpClientFactory = new CustomHttpClientFactory(this.Output) },
    searchEngineId: "<Your Google Search Engine Id>");

var query = "What is the Semantic Kernel?";

// Search and return results as string items
KernelSearchResults<string> stringResults = await textSearch.SearchAsync(query, new() { Top = 4, Skip = 0 });
Console.WriteLine("——— String Results ———\n");
await foreach (string result in stringResults.Results)
{
    Console.WriteLine(result);
}

// Search and return results as TextSearchResult items
KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = 4, Skip = 4 });
Console.WriteLine("\n——— Text Search Results ———\n");
await foreach (TextSearchResult result in textResults.Results)
{
    Console.WriteLine($"Name:  {result.Name}");
    Console.WriteLine($"Value: {result.Value}");
    Console.WriteLine($"Link:  {result.Link}");
}

// Search and return results as Google.Apis.CustomSearchAPI.v1.Data.Result items
KernelSearchResults<object> fullResults = await textSearch.GetSearchResultsAsync(query, new() { Top = 4, Skip = 8 });
Console.WriteLine("\n——— Google Web Page Results ———\n");
await foreach (Google.Apis.CustomSearchAPI.v1.Data.Result result in fullResults.Results)
{
    Console.WriteLine($"Title:       {result.Title}");
    Console.WriteLine($"Snippet:     {result.Snippet}");
    Console.WriteLine($"Link:        {result.Link}");
    Console.WriteLine($"DisplayLink: {result.DisplayLink}");
    Console.WriteLine($"Kind:        {result.Kind}");
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