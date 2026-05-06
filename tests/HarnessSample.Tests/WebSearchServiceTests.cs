using HarnessSample;

namespace HarnessSample.Tests;

public sealed class WebSearchServiceTests
{
    [Fact]
    public async Task SearchAsync_PrefersTavilyWhenAutoProviderIsRequested()
    {
        var tavilyClient = new RecordingWebSearchClient("tavily");
        var tinyFishClient = new RecordingWebSearchClient("tinyfish");
        var service = new WebSearchService(tavilyClient, tinyFishClient);

        WebSearchResponse response = await service.SearchAsync(new WebSearchRequest("copilot", "auto", 3));

        Assert.Equal("tavily", response.Provider);
        Assert.NotNull(tavilyClient.LastRequest);
        Assert.Null(tinyFishClient.LastRequest);
    }

    [Fact]
    public async Task SearchAsync_ClampsRequestedMaxResultsToTen()
    {
        var tavilyClient = new RecordingWebSearchClient("tavily");
        var service = new WebSearchService(tavilyClient, tinyFishClient: null);

        await service.SearchAsync(new WebSearchRequest("copilot", "tavily", 50));

        Assert.NotNull(tavilyClient.LastRequest);
        Assert.Equal(10, tavilyClient.LastRequest.MaxResults);
    }

    [Fact]
    public async Task SearchAsync_ThrowsWhenRequestedProviderIsUnavailable()
    {
        var service = new WebSearchService(tavilyClient: null, tinyFishClient: null);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.SearchAsync(new WebSearchRequest("copilot", "tavily", 5)));

        Assert.Contains("not available", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildUsageInstructions_ReturnsDisabledMessageWhenNoProviderIsConfigured()
    {
        var service = new WebSearchService(tavilyClient: null, tinyFishClient: null);

        string instructions = service.BuildUsageInstructions();

        Assert.Contains("未設定", instructions, StringComparison.Ordinal);
        Assert.DoesNotContain("tavily", instructions, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildUsageInstructions_ListsConfiguredProviders()
    {
        var service = new WebSearchService(
            new RecordingWebSearchClient("tavily"),
            new RecordingWebSearchClient("tinyfish"));

        string instructions = service.BuildUsageInstructions();

        Assert.Contains("tavily", instructions, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("tinyfish", instructions, StringComparison.OrdinalIgnoreCase);
    }

    private sealed class RecordingWebSearchClient(string providerName) : IWebSearchClient
    {
        public string ProviderName { get; } = providerName;

        public WebSearchRequest? LastRequest { get; private set; }

        public Task<WebSearchResponse> SearchAsync(WebSearchRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;

            return Task.FromResult(new WebSearchResponse(
                Provider: ProviderName,
                Query: request.Query,
                Results:
                [
                    new WebSearchResultItem(1, "Example", "https://example.com", "snippet")
                ],
                Summary: $"{ProviderName} summary"));
        }
    }
}