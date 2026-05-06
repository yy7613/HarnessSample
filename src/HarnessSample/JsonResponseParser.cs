using Microsoft.Agents.AI;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace HarnessSample;

public static class JsonResponseParser
{
    public static JsonSerializerOptions SerializerOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static T Parse<T>(AgentResponse response)
    {
        string json = ExtractJson(GetRawText(response));

        T? result = JsonSerializer.Deserialize<T>(json, SerializerOptions);
        if (result is null)
        {
            throw new InvalidOperationException("Structured output could not be deserialized.");
        }

        return result;
    }

    public static string GetRawText(AgentResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        string text = response.ToString()?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException("Agent response did not contain text.");
        }

        return text;
    }

    private static string ExtractJson(string text)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        string normalized = StripCodeFence(text).Trim();
        int start = normalized.IndexOf('{');
        int end = normalized.LastIndexOf('}');

        if (start < 0 || end < start)
        {
            throw new InvalidOperationException("JSON object was not found in the agent response.");
        }

        return normalized[start..(end + 1)];
    }

    private static string StripCodeFence(string text)
    {
        if (!text.StartsWith("```", StringComparison.Ordinal))
        {
            return text;
        }

        string[] lines = text.Split('\n');
        if (lines.Length <= 2)
        {
            return text;
        }

        return string.Join('\n', lines.Skip(1).SkipLast(1));
    }
}
