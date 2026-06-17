using Microsoft.Agents.AI;

namespace HarnessSample;

public static class HarnessScenarioRunner
{
    public static async Task RunScenarioAsync(HarnessRuntime runtime, string topic)
    {
        AgentSession session = await runtime.Agent.CreateSessionAsync();

        Console.WriteLine();
        Console.WriteLine($"Topic              : {topic}");
        Console.WriteLine($"Initial Mode       : {runtime.AgentModeProvider.GetMode(session)}");
        Console.WriteLine($"Working Folder     : {runtime.SessionFolderPath}");

        string planPrompt = $$"""
        次のテーマについて plan モードで作業を始めてください。
        テーマ: {{topic}}

        要件:
        - 回答は必ず、この HarnessSample 実装と提供された参照情報の範囲に限定する
        - todos_add を使って 3 件から 5 件の TODO を作成する
        - execute モードでどのサブエージェントへ何を依頼するかも簡潔に示す
        - Web検索 Tool が有効なら、どのタイミングで使うかも方針に含める
        - まだ最終回答を作り込みすぎず、実行方針を日本語で整理する
        """;

        ConsoleRenderer.PrintSection("Plan Response");
        AgentResponse planResponse = await runtime.Agent.RunAsync(planPrompt, session);
        Console.WriteLine(planResponse);

        await ConsoleRenderer.PrintTodosAsync(runtime.TodoProvider, session);
        ConsoleRenderer.PrintChatHistory(runtime.ChatHistoryProvider, session);
        ConsoleRenderer.PrintStateBag(session);

        runtime.AgentModeProvider.SetMode(session, "execute");
        Console.WriteLine();
        Console.WriteLine($"Mode switched to: {runtime.AgentModeProvider.GetMode(session)}");

        string executePrompt = $$"""
        次のテーマについて execute モードで作業を完了してください。
        テーマ: {{topic}}

        実行ルール:
        - 回答と保存内容は必ず、この HarnessSample 実装と提供された参照情報の範囲に限定する
        - 確認できないセットアップ手順、存在しない設定ファイル、存在しないコマンドは書かない
        - まず BackgroundAgents_StartTask を使って、少なくとも 2 つ以上のサブエージェントへ並列に依頼する
        - BackgroundAgents_WaitForFirstCompletion と BackgroundAgents_GetTaskResults を使って各結果を回収する
        - 開始した BackgroundAgents の taskId はすべて結果回収するまで最終回答に進まない
        - 一部のタスクだけ完了した場合は BackgroundAgents_WaitForFirstCompletion を繰り返す
        - Web検索 Tool が利用可能なら、必要な確認事項について積極的に使う
        - TODO の進捗を更新し、完了した項目は todos_complete する
        - 完了した TODO はすべて todos_complete するまで最終回答に進まない
        - 最終結果を `harness-result.md` という Markdown ファイル名で SaveFile する
        - SaveFile が成功するまで最終回答に進まない
        - 回答本文では、サブエージェントの要点、Web検索の要点、統合結果、保存したファイル内容の要約を返す
        """;

        ConsoleRenderer.PrintSection("Execute Response");
        AgentResponse executeResponse = await runtime.Agent.RunAsync(executePrompt, session);
        Console.WriteLine(executeResponse);

        await EnsureResultFileSavedAsync(runtime.Agent, session, topic, runtime.SessionFolderPath);
        await RetryIncompleteTodosAsync(runtime.Agent, runtime.TodoProvider, session);

        await ConsoleRenderer.PrintTodosAsync(runtime.TodoProvider, session);
        ConsoleRenderer.PrintChatHistory(runtime.ChatHistoryProvider, session);
        ConsoleRenderer.PrintStateBag(session);
        ConsoleRenderer.PrintSavedFiles(runtime.SessionFolderPath);
    }

    private static async Task EnsureResultFileSavedAsync(AIAgent agent, AgentSession session, string topic, string sessionFolderPath)
    {
        string resultFilePath = Path.Combine(sessionFolderPath, "harness-result.md");
        if (File.Exists(resultFilePath))
        {
            return;
        }

        ConsoleRenderer.PrintSection("SaveFile Retry");
        AgentResponse retryResponse = await agent.RunAsync(
            $"直前の作業結果を整理し、`harness-result.md` というファイル名で Markdown を SaveFile してください。テーマは「{topic}」です。",
            session);
        Console.WriteLine(retryResponse);
    }

    private static async Task RetryIncompleteTodosAsync(AIAgent agent, TodoProvider todoProvider, AgentSession session)
    {
        for (int attempt = 0; attempt < 2; attempt++)
        {
            TodoItem[] incompleteTodos = [.. (await todoProvider.GetAllTodosAsync(session)).Where(todo => !todo.IsComplete)];
            if (incompleteTodos.Length == 0)
            {
                return;
            }

            string todoList = string.Join(
                Environment.NewLine,
                incompleteTodos.Select(todo => $"- #{todo.Id} {todo.Title}"));

            ConsoleRenderer.PrintSection("Todo Complete Retry");
            AgentResponse retryResponse = await agent.RunAsync(
                $$"""
                次の TODO が未完了です。完了済みの作業として todos_complete で更新してください。

                ルール:
                - 関数名は必ず todos_complete を使う
                - todors_complete は存在しないため使わない
                - items には各 TODO の id と reason を指定する
                - TODO 更新が終わるまで最終回答に進まない

                未完了 TODO:
                {{todoList}}
                """,
                session);
            Console.WriteLine(retryResponse);
        }
    }
}
