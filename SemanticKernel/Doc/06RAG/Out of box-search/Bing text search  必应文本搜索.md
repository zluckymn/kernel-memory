## Overview  概述

The Bing Text Search implementation uses the [Bing Web Search API](https://www.microsoft.com/bing/apis/bing-web-search-api) to retrieve search results. You must provide your own Bing Search Api Key to use this component.
必应文本搜索实现使用[必应 Web 搜索 API](https://www.microsoft.com/bing/apis/bing-web-search-api) 来检索搜索结果。必须提供自己的必应搜索 API 密钥才能使用此组件。



## Limitations  局限性

  展开表

| Feature Area  功能区                      | Support  支持                                                |
| :---------------------------------------- | :----------------------------------------------------------- |
| Search API  搜索 API                      | [Bing Web Search API](https://www.microsoft.com/bing/apis/bing-web-search-api) only. 仅限[必应 Web 搜索 API](https://www.microsoft.com/bing/apis/bing-web-search-api)。 |
| Supported filter clauses 支持的过滤器子句 | Only "equal to" filter clauses are supported. 仅支持“等于”筛选子句。 |
| Supported filter keys  支持的筛选器键     | The [responseFilter](https://learn.microsoft.com/en-us/bing/search-apis/bing-web-search/reference/query-parameters#responsefilter) query parameter and [advanced search keywords](https://support.microsoft.com/topic/advanced-search-keywords-ea595928-5d63-4a0b-9c6b-0b769865e78a) are supported. 支持 [responseFilter](https://learn.microsoft.com/en-us/bing/search-apis/bing-web-search/reference/query-parameters#responsefilter) 查询参数和[高级搜索关键字 ](https://support.microsoft.com/topic/advanced-search-keywords-ea595928-5d63-4a0b-9c6b-0b769865e78a)。 |

 Tip  提示

Follow this link for more information on how to [filter the answers that Bing returns](https://learn.microsoft.com/en-us/bing/search-apis/bing-web-search/filter-answers#getting-results-from-a-specific-site). Follow this link for more information on using [advanced search keywords](https://support.microsoft.com/topic/advanced-search-keywords-ea595928-5d63-4a0b-9c6b-0b769865e78a)
点击此链接，了解有关如何[筛选必应返回的答案](https://learn.microsoft.com/en-us/bing/search-apis/bing-web-search/filter-answers#getting-results-from-a-specific-site)的详细信息。点击此链接了解有关使用[高级搜索关键字](https://support.microsoft.com/topic/advanced-search-keywords-ea595928-5d63-4a0b-9c6b-0b769865e78a)的更多信息



## Getting started  开始

The sample below shows how to create a `BingTextSearch` and use it to perform a text search.
下面的示例演示如何创建 `BingTextSearch` 并使用它来执行文本搜索。

C#Copy  复制

```csharp
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Plugins.Web.Bing;

// Create an ITextSearch instance using Bing search
var textSearch = new BingTextSearch(apiKey: "<Your Bing API Key>");

var query = "What is the Semantic Kernel?";

// Search and return results as a string items
KernelSearchResults<string> stringResults = await textSearch.SearchAsync(query, new() { Top = 4, Skip = 0 });
Console.WriteLine("--- String Results ---\n");
await foreach (string result in stringResults.Results)
{
    Console.WriteLine(result);
}

// Search and return results as TextSearchResult items
KernelSearchResults<TextSearchResult> textResults = await textSearch.GetTextSearchResultsAsync(query, new() { Top = 4, Skip = 4 });
Console.WriteLine("\n--- Text Search Results ---\n");
await foreach (TextSearchResult result in textResults.Results)
{
    Console.WriteLine($"Name:  {result.Name}");
    Console.WriteLine($"Value: {result.Value}");
    Console.WriteLine($"Link:  {result.Link}");
}

// Search and return s results as BingWebPage items
KernelSearchResults<object> fullResults = await textSearch.GetSearchResultsAsync(query, new() { Top = 4, Skip = 8 });
Console.WriteLine("\n--- Bing Web Page Results ---\n");
await foreach (BingWebPage result in fullResults.Results)
{
    Console.WriteLine($"Name:            {result.Name}");
    Console.WriteLine($"Snippet:         {result.Snippet}");
    Console.WriteLine($"Url:             {result.Url}");
    Console.WriteLine($"DisplayUrl:      {result.DisplayUrl}");
    Console.WriteLine($"DateLastCrawled: {result.DateLastCrawled}");
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