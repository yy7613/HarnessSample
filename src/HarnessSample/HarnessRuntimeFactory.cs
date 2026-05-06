using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace HarnessSample;

public static class HarnessRuntimeFactory
{
    public static HarnessRuntime Create(IChatClient chatClient, string agentFilesRoot, HarnessSampleConfiguration configuration)
    {
        string workingFolder = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";
        string sessionFolderPath = Path.Combine(agentFilesRoot, workingFolder);

        var chatHistoryProvider = new InMemoryChatHistoryProvider();
        var harnessReferenceProvider = new HarnessReferenceProvider(configuration.HarnessReference);
        var todoProvider = new TodoProvider();
        var agentModeProvider = new AgentModeProvider(new AgentModeProviderOptions
        {
            DefaultMode = "plan",
            Modes =
            [
                new AgentModeProviderOptions.AgentMode("plan", "作業を分解し、TODO とサブエージェント戦略を決めるモード"),
                new AgentModeProviderOptions.AgentMode("execute", "サブエージェントや Web検索を使って調査し、成果物を仕上げるモード")
            ]
        });

        var overviewSubAgent = CreateSubAgent(
            chatClient,
            "OverviewResearchAgent",
            "テーマの概要、背景、重要性を整理するサブエージェント",
            $$"""
            あなたは HarnessSample 用の概要整理サブエージェントです。
            {{configuration.HarnessReference}}

            ユーザーのテーマについて、背景、目的、重要ポイントを日本語で簡潔に整理してください。箇条書きを優先してください。
            """);

        var implementationSubAgent = CreateSubAgent(
            chatClient,
            "ImplementationAdviceAgent",
            "テーマを実装または利用する際の実践ポイントを整理するサブエージェント",
            $$"""
            あなたは HarnessSample 用の実装観点整理サブエージェントです。
            {{configuration.HarnessReference}}

            ユーザーのテーマについて、実装手順、コード設計上の観点、試し方を日本語で整理してください。実務で役立つ観点を優先してください。
            """);

        var reviewSubAgent = CreateSubAgent(
            chatClient,
            "ReviewChecklistAgent",
            "注意点、落とし穴、確認項目を整理するサブエージェント",
            $$"""
            あなたは HarnessSample 用のレビュー観点整理サブエージェントです。
            {{configuration.HarnessReference}}

            ユーザーのテーマについて、注意点、落とし穴、動作確認チェックリストを日本語で整理してください。短く明確にまとめてください。
            """);

        var subAgentsProvider = new SubAgentsProvider(
            [overviewSubAgent, implementationSubAgent, reviewSubAgent],
            new SubAgentsProviderOptions
            {
                Instructions =
                    "サブエージェントを使うときは、まず必要なタスクを並列に開始してください。" +
                    configuration.HarnessReference +
                    "回答は必ず提供された参照情報の範囲に限定し、確認できない情報は書かないでください。" +
                    "結果を集約してから最終回答を作ってください。利用可能なサブエージェント一覧: {sub_agents}"
            });

        var fileMemoryProvider = new FileMemoryProvider(
            new FileSystemAgentFileStore(agentFilesRoot),
            _ => new FileMemoryState { WorkingFolder = workingFolder });

        WebSearchService webSearchService = WebSearchToolFactory.CreateService(configuration.WebSearch);
        IReadOnlyList<AITool> webSearchTools = new WebSearchToolAdapter(webSearchService).CreateTools();
        string webSearchInstructions = webSearchService.BuildUsageInstructions();
        StructuredWebSearchAggregator structuredAggregator = new(chatClient);

        ChatClientAgentOptions parentOptions = new()
        {
            Name = "HarnessSampleAgent",
            Description = "Harness の Todo / Mode / SubAgents / FileMemory / WebSearch を LM Studio で確認する親エージェントです。",
            ChatHistoryProvider = chatHistoryProvider,
            AIContextProviders = [harnessReferenceProvider, todoProvider, agentModeProvider, subAgentsProvider, fileMemoryProvider],
            ChatOptions = new ChatOptions
            {
                Instructions =
                    "あなたは HarnessSample の親エージェントです。" +
                    configuration.HarnessReference +
                    webSearchInstructions +
                    "Web検索 Tool を使った場合は、結果をそのまま列挙するだけでなく、必要に応じて構造化して整理してください。" +
                    "提供された参照情報から確認できる内容だけを述べてください。確認できない情報は『このサンプル実装からは確認できません』と述べてください。" +
                    "plan モードでは TodoList を作成し、どのサブエージェントへ依頼するか決めてください。" +
                    "execute モードでは必ず SubAgentsProvider を使って複数のサブエージェントへ並列に依頼し、必要なら Web検索 Tool も使って結果を統合してください。" +
                    "作業完了時は Todo を更新し、最終結果を SaveFile で Markdown として保存してください。" +
                    "日時が必要な場合のみ {{GetDateTime}} を使ってください。応答は日本語で行ってください。",
                Tools = [AIFunctionFactory.Create(HarnessTools.GetDateTime), AIFunctionFactory.Create(structuredAggregator.AggregateAsync), .. webSearchTools],
                MaxOutputTokens = 5000
            }
        };

        return new HarnessRuntime(
            chatClient.AsAIAgent(parentOptions),
            chatHistoryProvider,
            todoProvider,
            agentModeProvider,
            fileMemoryProvider,
            sessionFolderPath);
    }

    private static AIAgent CreateSubAgent(IChatClient chatClient, string name, string description, string instructions)
    {
        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = name,
            Description = description,
            ChatOptions = new ChatOptions
            {
                Instructions = instructions,
                MaxOutputTokens = 4000
            }
        });
    }
}
