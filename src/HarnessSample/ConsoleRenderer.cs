using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace HarnessSample;

public static class ConsoleRenderer
{
    public static void PrintStartup(HarnessSampleConfiguration configuration)
    {
        Console.WriteLine();
        Console.WriteLine("HarnessSample を開始します。");
        Console.WriteLine($"LM Studio Endpoint : {configuration.LmStudioUrl}");
        Console.WriteLine($"Model              : {configuration.ModelName}");

        string webSearchStatus = configuration.WebSearch.HasAnyProvider
            ? $"enabled ({string.Join(", ", GetEnabledProviders(configuration.WebSearch))})"
            : "disabled";
        Console.WriteLine($"Web Search Tool    : {webSearchStatus}");
    }

    public static void PrintSection(string title)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 20) + $" {title} " + new string('=', 20));
    }

    public static string? ReadInteractiveTopic(IReadOnlyList<string> sampleTopics)
    {
        PrintSection("Input Examples");
        for (int i = 0; i < sampleTopics.Count; i++)
        {
            Console.WriteLine($"{i + 1}. {sampleTopics[i]}");
        }

        Console.WriteLine("exit を入力すると終了します。");
        Console.Write("番号またはテーマを入力してください: ");
        string? input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            return sampleTopics[0];
        }

        string trimmed = input.Trim();
        if (string.Equals(trimmed, "exit", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (int.TryParse(trimmed, out int index) && index >= 1 && index <= sampleTopics.Count)
        {
            return sampleTopics[index - 1];
        }

        return trimmed;
    }

    public static void PrintTodos(TodoProvider todoProvider, AgentSession session)
    {
        PrintSection("TodoList");

        IReadOnlyList<TodoItem> todos = todoProvider.GetAllTodos(session);
        if (todos.Count == 0)
        {
            Console.WriteLine("TODO はまだ作成されていません。");
            return;
        }

        foreach (TodoItem todo in todos)
        {
            string status = todo.IsComplete ? "x" : " ";
            Console.WriteLine($"- [{status}] #{todo.Id} {todo.Title}");
            if (!string.IsNullOrWhiteSpace(todo.Description))
            {
                Console.WriteLine($"  description: {todo.Description}");
            }
        }
    }

    public static void PrintChatHistory(InMemoryChatHistoryProvider chatHistoryProvider, AgentSession session)
    {
        PrintSection("ChatHistory");

        IReadOnlyList<ChatMessage> messages = chatHistoryProvider.GetMessages(session);
        if (messages.Count == 0)
        {
            Console.WriteLine("チャット履歴は空です。");
            return;
        }

        foreach (ChatMessage message in messages)
        {
            string text = string.Concat(message.Contents.OfType<TextContent>().Select(content => content.Text)).Trim();
            Console.WriteLine($"{message.Role}: {text}");
        }
    }

    public static void PrintStateBag(AgentSession session)
    {
        PrintSection("StateBag");

        JsonElement stateBagJson = session.StateBag.Serialize();
        foreach (JsonProperty entry in stateBagJson.EnumerateObject())
        {
            Console.WriteLine($"{entry.Name}: {entry.Value}");
        }
    }

    public static void PrintSavedFiles(string sessionFolderPath)
    {
        PrintSection("SavedFiles");

        if (!Directory.Exists(sessionFolderPath))
        {
            Console.WriteLine($"保存フォルダーはまだ作成されていません: {sessionFolderPath}");
            return;
        }

        string[] files = Directory.GetFiles(sessionFolderPath, "*", SearchOption.AllDirectories)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (files.Length == 0)
        {
            Console.WriteLine("保存されたファイルはありません。");
            return;
        }

        foreach (string file in files)
        {
            Console.WriteLine($"- {Path.GetFileName(file)}");
            Console.WriteLine(File.ReadAllText(file));
            Console.WriteLine();
        }
    }

    public static void PrintError(Exception ex, HarnessSampleConfiguration configuration)
    {
        PrintSection("Error");
        Console.WriteLine(ex.Message);
        Console.WriteLine();
        Console.WriteLine($"LM Studio endpoint: {configuration.LmStudioUrl}");
        Console.WriteLine($"Model name       : {configuration.ModelName}");
        Console.WriteLine("LM Studio が起動していること、モデル名が一致していること、OpenAI 互換 API が有効なことを確認してください。");
    }

    private static IEnumerable<string> GetEnabledProviders(WebSearchToolConfiguration configuration)
    {
        if (configuration.HasTavily)
        {
            yield return "Tavily";
        }

        if (configuration.HasTinyFish)
        {
            yield return "TinyFish";
        }
    }
}
