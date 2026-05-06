using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace HarnessSample;

public sealed class HarnessReferenceProvider(string referenceText) : AIContextProvider
{
    protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = default)
    {
        return new ValueTask<AIContext>(new AIContext
        {
            Instructions = "以下の参照情報を最優先し、この HarnessSample 実装の事実だけを使って回答してください。確認できない情報は推測しないでください。",
            Messages = [new ChatMessage(ChatRole.System, referenceText)]
        });
    }
}
