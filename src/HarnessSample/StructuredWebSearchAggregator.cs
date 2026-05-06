using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace HarnessSample;

public sealed class StructuredWebSearchAggregator
{
    public static readonly JsonSerializerOptions SerializerOptions = JsonResponseParser.SerializerOptions;

    private readonly AIAgent _agent;

    public StructuredWebSearchAggregator(IChatClient chatClient)
    {
        JsonElement schema = AIJsonUtilities.CreateJsonSchema(
            typeof(StructuredResearchDigest),
            description: "検索結果を要約した構造化データです。",
            hasDefaultValue: false,
            defaultValue: null,
            serializerOptions: SerializerOptions,
            inferenceOptions: AIJsonSchemaCreateOptions.Default);

        _agent = chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "StructuredWebSearchAggregator",
            Description = "検索結果を構造化要約へ変換する補助エージェントです。",
            ChatOptions = new ChatOptions
            {
                Instructions = "与えられた検索結果だけを使って構造化要約を返してください。事実の追加や推測は行わないでください。language は ja を優先してください。",
                ResponseFormat = ChatResponseFormat.ForJsonSchema(
                    schema,
                    schemaName: "structured_research_digest",
                    schemaDescription: "検索結果から summary、keyFindings、references を抽出する JSON schema")
            }
        });
    }

    public async Task<StructuredResearchDigest> AggregateAsync(string topic, WebSearchResponse searchResponse)
    {
        AgentSession session = await _agent.CreateSessionAsync().ConfigureAwait(false);

        string input = $$"""
        次のテーマに関する検索結果を構造化してください。
        テーマ: {{topic}}
        Provider: {{searchResponse.Provider}}
        Query: {{searchResponse.Query}}
        Summary: {{searchResponse.Summary}}
        Results:
        {{JsonSerializer.Serialize(searchResponse.Results, SerializerOptions)}}
        """;

        AgentResponse response = await _agent.RunAsync(input, session).ConfigureAwait(false);
        return JsonResponseParser.Parse<StructuredResearchDigest>(response);
    }
}

public sealed record StructuredResearchDigest(
    [property: Description("検索結果全体の短い要約。")]
    string Summary,

    [property: Description("重要ポイントの一覧。")]
    IReadOnlyList<string> KeyFindings,

    [property: Description("参照URLとタイトルの一覧。")]
    IReadOnlyList<StructuredReference> References,

    [property: Description("出力言語。通常は ja。")]
    string Language);

public sealed record StructuredReference(
    [property: Description("参照タイトル。")]
    string Title,

    [property: Description("参照 URL。")]
    string Url);
