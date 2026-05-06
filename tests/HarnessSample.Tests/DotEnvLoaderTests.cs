using System.Text;

namespace HarnessSample.Tests;

public sealed class DotEnvLoaderTests
{
    [Fact]
    public void LoadIfPresent_LoadsRepositoryRootDotEnv()
    {
        using var scope = new EnvironmentVariableScope(
            ("TAVILY_API_KEY", null),
            ("TINYFISH_API_KEY", null),
            ("TINYFISH_LOCATION", null),
            ("TINYFISH_LANGUAGE", null));
        using var tempDirectory = new TemporaryDirectoryScope();

        string envFilePath = Path.Combine(tempDirectory.Path, ".env");
        File.WriteAllText(envFilePath, "TAVILY_API_KEY=tvly-from-dotenv", Encoding.UTF8);

        string? loadedPath = DotEnvLoader.LoadIfPresent(new[] { tempDirectory.Path });
        WebSearchToolConfiguration configuration = WebSearchToolConfiguration.LoadFromEnvironment();

        Assert.Equal(envFilePath, loadedPath);
        Assert.Equal("tvly-from-dotenv", configuration.TavilyApiKey);
    }

    [Fact]
    public void LoadIfPresent_UsesScriptsDotEnvWithoutOverwritingExistingEnvironmentVariables()
    {
        using var scope = new EnvironmentVariableScope(
            ("TAVILY_API_KEY", null),
            ("TINYFISH_API_KEY", "tinyfish-from-env"),
            ("TINYFISH_LOCATION", "JP"),
            ("TINYFISH_LANGUAGE", "ja"));
        using var tempDirectory = new TemporaryDirectoryScope();

        string scriptsDirectory = Directory.CreateDirectory(Path.Combine(tempDirectory.Path, "scripts")).FullName;
        string envFilePath = Path.Combine(scriptsDirectory, ".env");
        File.WriteAllText(
            envFilePath,
            "TINYFISH_API_KEY=tinyfish-from-dotenv\nTINYFISH_LOCATION=US\nTINYFISH_LANGUAGE=en",
            Encoding.UTF8);

        string? loadedPath = DotEnvLoader.LoadIfPresent(new[] { tempDirectory.Path });
        WebSearchToolConfiguration configuration = WebSearchToolConfiguration.LoadFromEnvironment();

        Assert.Equal(envFilePath, loadedPath);
        Assert.Equal("tinyfish-from-env", configuration.TinyFishApiKey);
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

    private sealed class TemporaryDirectoryScope : IDisposable
    {
        public TemporaryDirectoryScope()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"HarnessSample.Tests.{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}