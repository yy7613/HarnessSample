namespace HarnessSample;

public sealed record WebSearchToolConfiguration(
    string? TavilyApiKey,
    string? TinyFishApiKey,
    string? TinyFishLocation,
    string? TinyFishLanguage)
{
    public bool HasAnyProvider => HasTavily || HasTinyFish;

    public bool HasTavily => !string.IsNullOrWhiteSpace(TavilyApiKey);

    public bool HasTinyFish => !string.IsNullOrWhiteSpace(TinyFishApiKey);

    public static WebSearchToolConfiguration LoadFromEnvironment()
    {
        return new WebSearchToolConfiguration(
            TavilyApiKey: Environment.GetEnvironmentVariable("TAVILY_API_KEY"),
            TinyFishApiKey: Environment.GetEnvironmentVariable("TINYFISH_API_KEY"),
            TinyFishLocation: Environment.GetEnvironmentVariable("TINYFISH_LOCATION"),
            TinyFishLanguage: Environment.GetEnvironmentVariable("TINYFISH_LANGUAGE"));
    }
}
