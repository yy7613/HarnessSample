using System.ComponentModel;
using System.Text.Json;

namespace HarnessSample;

public sealed class WebSearchService
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    private readonly IWebSearchClient? _tavilyClient;
    private readonly IWebSearchClient? _tinyFishClient;

    public WebSearchService(IWebSearchClient? tavilyClient, IWebSearchClient? tinyFishClient)
    {
        _tavilyClient = tavilyClient;
        _tinyFishClient = tinyFishClient;
    }

    [Description("Web検索を実行して、要約と上位結果を返します。provider は auto / tavily / tinyfish を指定できます。APIキー未設定の provider は使えません。")]
    public async Task<string> SearchWebAsync(string query, string provider = "auto", int maxResults = 5, string? notes = null)
    {
        WebSearchRequest request = new(query, provider, maxResults, notes);
        WebSearchResponse response = await SearchAsync(request).ConfigureAwait(false);
        return JsonSerializer.Serialize(response, SerializerOptions);
    }

    public async Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken = default)
    {
        IWebSearchClient client = ResolveClient(request.Provider);
        return await client.SearchAsync(request with { MaxResults = Math.Clamp(request.MaxResults, 1, 10) }, cancellationToken).ConfigureAwait(false);
    }

    public string BuildUsageInstructions()
    {
        if (!HasAnyProvider())
        {
            return "Web検索 Tool は未設定です。外部Web検索は行わず、提供された参照情報のみで回答してください。";
        }

        List<string> providers = [];
        if (_tavilyClient is not null)
        {
            providers.Add("tavily");
        }

        if (_tinyFishClient is not null)
        {
            providers.Add("tinyfish");
        }

        return "Web検索 Tool が利用可能です。事実確認が必要な場合は SearchWebAsync を使ってください。" +
               $"利用可能 provider: {string.Join(", ", providers)}。" +
               "provider に auto を指定した場合は tavily を優先し、未設定なら tinyfish を使用してください。";
    }

    private IWebSearchClient ResolveClient(string? provider)
    {
        string normalized = string.IsNullOrWhiteSpace(provider) ? "auto" : provider.Trim().ToLowerInvariant();
        return normalized switch
        {
            "tavily" when _tavilyClient is not null => _tavilyClient,
            "tinyfish" when _tinyFishClient is not null => _tinyFishClient,
            "auto" when _tavilyClient is not null => _tavilyClient,
            "auto" when _tinyFishClient is not null => _tinyFishClient,
            _ => throw new InvalidOperationException($"Web search provider '{provider}' is not available. APIキー設定を確認してください。")
        };
    }

    public bool HasAnyProvider() => _tavilyClient is not null || _tinyFishClient is not null;
}
