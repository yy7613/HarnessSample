namespace HarnessSample;

public sealed record HarnessSampleConfiguration(
    string LmStudioUrl,
    string ModelName,
    string DummyApiKey,
    IReadOnlyList<string> SampleTopics,
    string HarnessReference,
    WebSearchToolConfiguration WebSearch)
{
    public static HarnessSampleConfiguration Load()
    {
        DotEnvLoader.LoadIfPresent();

        string[] sampleTopics =
        [
            "この HarnessSample 実装の使い方",
            "この HarnessSample における SubAgents の役割",
            "この HarnessSample における TodoProvider / AgentModeProvider / SubAgentsProvider / FileMemoryProvider の役割整理",
            "Web検索 Tool を使った HarnessSample の調査フロー"
        ];

        const string harnessReference = """
        この会話で対象にしている Harness は、旧 Microsoft Agent / COM 技術ではなく、Microsoft Agent Framework の .NET Harness サンプル群です。

        公式 Harness サンプルの要点:
        - Step01: TodoProvider と AgentModeProvider を使って、plan / execute モードで調査を進める
        - Step02: SubAgentsProvider を使って、親エージェントから複数のサブエージェントへ委譲する
        - Step03: FileAccess / FileMemory 系の仕組みで、ファイルを読み書きしながら作業結果を保存する

        このローカル HarnessSample 実装の要点:
        - LM Studio の OpenAI 互換 endpoint は http://localhost:1234/v1
        - model は openai/gpt-oss-20b
        - 親エージェントは HarnessSampleAgent
        - AIContextProviders として TodoProvider / AgentModeProvider / SubAgentsProvider / FileMemoryProvider を使う
        - オプションで Web検索 Tool を使える
        - plan モードでは TODO と実行方針を作る
        - execute モードでは SubAgentsProvider を使ってサブエージェントへ依頼し、必要なら Web検索 Tool も使って結果を統合する
        - サブエージェントは OverviewResearchAgent / ImplementationAdviceAgent / ReviewChecklistAgent の 3 つ
        - 最終成果物は harness-result.md として SaveFile する

        この前提から外れる一般論や架空のセットアップ手順は書かず、このサンプル実装に即して説明すること。
        """;

        return new HarnessSampleConfiguration(
            LmStudioUrl: "http://localhost:1234/v1",
            ModelName: "openai/gpt-oss-20b",
            DummyApiKey: "sk-dummy",
            SampleTopics: sampleTopics,
            HarnessReference: harnessReference,
            WebSearch: WebSearchToolConfiguration.LoadFromEnvironment());
    }
}
