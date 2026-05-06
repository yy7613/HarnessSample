using DotNetEnv;

namespace HarnessSample;

internal static class DotEnvLoader
{
    public static void LoadIfPresent()
    {
        LoadIfPresent(GetStartDirectories());
    }

    internal static string? LoadIfPresent(IEnumerable<string> startDirectories)
    {
        string? envFilePath = FindDotEnvFile(startDirectories);
        if (envFilePath is null)
        {
            return null;
        }

        Env.NoClobber().Load(envFilePath);

        return envFilePath;
    }

    private static string? FindDotEnvFile(IEnumerable<string> startDirectories)
    {
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (string startDirectory in startDirectories)
        {
            foreach (string candidate in EnumerateCandidateFiles(startDirectory))
            {
                if (!visited.Add(candidate))
                {
                    continue;
                }

                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }
        }

        return null;
    }

    private static IEnumerable<string> GetStartDirectories()
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        if (!string.IsNullOrWhiteSpace(currentDirectory))
        {
            yield return currentDirectory;
        }

        string baseDirectory = AppContext.BaseDirectory;
        if (!string.IsNullOrWhiteSpace(baseDirectory))
        {
            yield return baseDirectory;
        }
    }

    private static IEnumerable<string> EnumerateCandidateFiles(string startDirectory)
    {
        DirectoryInfo? directory = new(startDirectory);
        while (directory is not null)
        {
            yield return Path.Combine(directory.FullName, ".env");
            yield return Path.Combine(directory.FullName, "scripts", ".env");
            directory = directory.Parent;
        }
    }
}