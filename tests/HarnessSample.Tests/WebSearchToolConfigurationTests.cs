using HarnessSample;

namespace HarnessSample.Tests;

public sealed class WebSearchToolConfigurationTests
{
    [Fact]
    public void ProviderFlags_ReturnFalseWhenApiKeysAreBlank()
    {
        var configuration = new WebSearchToolConfiguration(
            TavilyApiKey: " ",
            TinyFishApiKey: string.Empty,
            TinyFishLocation: null,
            TinyFishLanguage: null);

        Assert.False(configuration.HasTavily);
        Assert.False(configuration.HasTinyFish);
        Assert.False(configuration.HasAnyProvider);
    }

    [Fact]
    public void ProviderFlags_ReturnTrueWhenAnyApiKeyExists()
    {
        var configuration = new WebSearchToolConfiguration(
            TavilyApiKey: "tvly-demo",
            TinyFishApiKey: null,
            TinyFishLocation: null,
            TinyFishLanguage: null);

        Assert.True(configuration.HasTavily);
        Assert.False(configuration.HasTinyFish);
        Assert.True(configuration.HasAnyProvider);
    }

    [Fact]
    public void LoadFromEnvironment_ReadsConfiguredValues()
    {
        using var scope = new EnvironmentVariableScope(
            ("TAVILY_API_KEY", "tvly-demo"),
            ("TINYFISH_API_KEY", "tinyfish-demo"),
            ("TINYFISH_LOCATION", "JP"),
            ("TINYFISH_LANGUAGE", "ja"));

        WebSearchToolConfiguration configuration = WebSearchToolConfiguration.LoadFromEnvironment();

        Assert.Equal("tvly-demo", configuration.TavilyApiKey);
        Assert.Equal("tinyfish-demo", configuration.TinyFishApiKey);
        Assert.Equal("JP", configuration.TinyFishLocation);
        Assert.Equal("ja", configuration.TinyFishLanguage);
    }

    private sealed class EnvironmentVariableScope : IDisposable
    {
        private readonly IReadOnlyDictionary<string, string?> _originalValues;

        public EnvironmentVariableScope(params (string Name, string? Value)[] variables)
        {
            _originalValues = variables.ToDictionary(
                item => item.Name,
                item => Environment.GetEnvironmentVariable(item.Name));

            foreach ((string name, string? value) in variables)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }

        public void Dispose()
        {
            foreach ((string name, string? value) in _originalValues)
            {
                Environment.SetEnvironmentVariable(name, value);
            }
        }
    }
}