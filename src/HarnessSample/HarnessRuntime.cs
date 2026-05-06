using Microsoft.Agents.AI;

namespace HarnessSample;

public sealed class HarnessRuntime : IDisposable
{
    public HarnessRuntime(
        AIAgent agent,
        InMemoryChatHistoryProvider chatHistoryProvider,
        TodoProvider todoProvider,
        AgentModeProvider agentModeProvider,
        FileMemoryProvider fileMemoryProvider,
        string sessionFolderPath)
    {
        Agent = agent;
        ChatHistoryProvider = chatHistoryProvider;
        TodoProvider = todoProvider;
        AgentModeProvider = agentModeProvider;
        FileMemoryProvider = fileMemoryProvider;
        SessionFolderPath = sessionFolderPath;
    }

    public AIAgent Agent { get; }

    public InMemoryChatHistoryProvider ChatHistoryProvider { get; }

    public TodoProvider TodoProvider { get; }

    public AgentModeProvider AgentModeProvider { get; }

    public FileMemoryProvider FileMemoryProvider { get; }

    public string SessionFolderPath { get; }

    public void Dispose()
    {
        FileMemoryProvider.Dispose();
    }
}
