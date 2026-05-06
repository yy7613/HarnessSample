using System.ComponentModel;
using System.Text.Json.Serialization;

namespace HarnessSample;

public sealed record WebSearchRequest(
    [property: Description("検索したいキーワードや質問。具体的に指定する。")]
    string Query,

    [property: Description("検索対象の provider。'tavily'、'tinyfish'、'auto' のいずれか。省略時は auto。")]
    string Provider = "auto",

    [property: Description("取得する最大件数。1 から 10 の範囲を推奨。")]
    int MaxResults = 5,

    [property: Description("必要に応じて付ける追加制約。日本語サイト優先、公式情報優先など。")]
    string? Notes = null);

public sealed record WebSearchResponse(
    string Provider,
    string Query,
    IReadOnlyList<WebSearchResultItem> Results,
    string Summary);

public sealed record WebSearchResultItem(
    int Position,
    string Title,
    string Url,
    string Snippet,
    string? SiteName = null);

internal sealed record TinyFishSearchResponse(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("results")] IReadOnlyList<TinyFishSearchItem> Results,
    [property: JsonPropertyName("total_results")] int TotalResults);

internal sealed record TinyFishSearchItem(
    [property: JsonPropertyName("position")] int Position,
    [property: JsonPropertyName("site_name")] string SiteName,
    [property: JsonPropertyName("snippet")] string Snippet,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("url")] string Url);

internal sealed record TavilySearchRequest(
    [property: JsonPropertyName("api_key")] string ApiKey,
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("search_depth")] string SearchDepth,
    [property: JsonPropertyName("topic")] string Topic,
    [property: JsonPropertyName("max_results")] int MaxResults,
    [property: JsonPropertyName("include_answer")] bool IncludeAnswer,
    [property: JsonPropertyName("include_raw_content")] bool IncludeRawContent,
    [property: JsonPropertyName("include_images")] bool IncludeImages);

internal sealed record TavilySearchResponse(
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("answer")] string? Answer,
    [property: JsonPropertyName("results")] IReadOnlyList<TavilySearchResult> Results);

internal sealed record TavilySearchResult(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("content")] string? Content,
    [property: JsonPropertyName("score")] double? Score,
    [property: JsonPropertyName("raw_content")] string? RawContent = null);
