namespace HarnessSample;

public static class WebSearchToolFactory
{
    public static WebSearchService CreateService(WebSearchToolConfiguration configuration)
    {
        IWebSearchClient? tavilyClient = null;
        IWebSearchClient? tinyFishClient = null;

        if (configuration.HasTavily)
        {
            tavilyClient = new TavilySearchClient(
                new HttpClient
                {
                    BaseAddress = new Uri("https://api.tavily.com/"),
                    Timeout = TimeSpan.FromSeconds(60)
                },
                configuration);
        }

        if (configuration.HasTinyFish)
        {
            tinyFishClient = new TinyFishSearchClient(
                new HttpClient
                {
                    BaseAddress = new Uri("https://api.search.tinyfish.ai/"),
                    Timeout = TimeSpan.FromSeconds(60)
                },
                configuration);
        }

        return new WebSearchService(tavilyClient, tinyFishClient);
    }
}
