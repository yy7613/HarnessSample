using System.Net.Http.Headers;
using System.Text.Json;

namespace HarnessSample;

public interface IWebSearchClient
{
    string ProviderName { get; }

    Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken = default);
}

public sealed class TinyFishSearchClient(HttpClient httpClient, WebSearchToolConfiguration configuration) : IWebSearchClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly WebSearchToolConfiguration _configuration = configuration;

    public string ProviderName => "tinyfish";

    public async Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken = default)
    {
        using var message = new HttpRequestMessage(HttpMethod.Get, BuildQueryUri(request));
        message.Headers.Add("X-API-Key", _configuration.TinyFishApiKey);

        using HttpResponseMessage response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        string responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        TinyFishSearchResponse? result = JsonSerializer.Deserialize<TinyFishSearchResponse>(responseText, SerializerOptions);
        if (result is null)
        {
            throw new InvalidOperationException("TinyFish search response could not be deserialized.");
        }

        IReadOnlyList<WebSearchResultItem> items = result.Results
            .Take(Math.Clamp(request.MaxResults, 1, 10))
            .Select(item => new WebSearchResultItem(item.Position, item.Title, item.Url, item.Snippet, item.SiteName))
            .ToArray();

        return new WebSearchResponse(
            Provider: ProviderName,
            Query: result.Query,
            Results: items,
            Summary: $"TinyFish で {items.Count} 件の検索結果を取得しました。");
    }

    private Uri BuildQueryUri(WebSearchRequest request)
    {
        var query = new List<string>
        {
            $"query={Uri.EscapeDataString(request.Query)}"
        };

        if (!string.IsNullOrWhiteSpace(_configuration.TinyFishLocation))
        {
            query.Add($"location={Uri.EscapeDataString(_configuration.TinyFishLocation)}");
        }

        if (!string.IsNullOrWhiteSpace(_configuration.TinyFishLanguage))
        {
            query.Add($"language={Uri.EscapeDataString(_configuration.TinyFishLanguage)}");
        }

        return new Uri($"?{string.Join("&", query)}", UriKind.Relative);
    }
}

public sealed class TavilySearchClient(HttpClient httpClient, WebSearchToolConfiguration configuration) : IWebSearchClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly HttpClient _httpClient = httpClient;
    private readonly WebSearchToolConfiguration _configuration = configuration;

    public string ProviderName => "tavily";

    public async Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken = default)
    {
        TavilySearchRequest payload = new(
            ApiKey: _configuration.TavilyApiKey!,
            Query: request.Query,
            SearchDepth: "advanced",
            Topic: "general",
            MaxResults: Math.Clamp(request.MaxResults, 1, 10),
            IncludeAnswer: true,
            IncludeRawContent: false,
            IncludeImages: false);

        using HttpRequestMessage message = JsonHttp.CreatePost("search", payload, SerializerOptions);
        using HttpResponseMessage response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        string responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        TavilySearchResponse? result = JsonSerializer.Deserialize<TavilySearchResponse>(responseText, SerializerOptions);
        if (result is null)
        {
            throw new InvalidOperationException("Tavily search response could not be deserialized.");
        }

        IReadOnlyList<WebSearchResultItem> items = result.Results
            .Select((item, index) => new WebSearchResultItem(index + 1, item.Title, item.Url, item.Content ?? string.Empty))
            .ToArray();

        string summary = string.IsNullOrWhiteSpace(result.Answer)
            ? $"Tavily で {items.Count} 件の検索結果を取得しました。"
            : result.Answer!;

        return new WebSearchResponse(
            Provider: ProviderName,
            Query: result.Query,
            Results: items,
            Summary: summary);
    }
}

internal static class JsonHttp
{
    public static HttpRequestMessage CreatePost<T>(string relativeUrl, T payload, JsonSerializerOptions serializerOptions)
    {
        string json = JsonSerializer.Serialize(payload, serializerOptions);
        return new HttpRequestMessage(HttpMethod.Post, relativeUrl)
        {
            Content = new StringContent(json)
            {
                Headers =
                {
                    ContentType = new MediaTypeHeaderValue("application/json")
                }
            }
        };
    }
}
