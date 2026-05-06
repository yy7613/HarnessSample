using Microsoft.Extensions.AI;

namespace HarnessSample;

public sealed class WebSearchToolAdapter(WebSearchService service)
{
    private readonly WebSearchService _service = service;

    public IReadOnlyList<AITool> CreateTools()
    {
        if (!_service.HasAnyProvider())
        {
            return [];
        }

        return [AIFunctionFactory.Create(_service.SearchWebAsync)];
    }
}
