using Microsoft.Agents.AI;

namespace ChatAgentic.Features.AI
{
    public class TextSearchProviderFactory
    {
        private readonly TextSearchAdpter _adapter;

        public TextSearchProviderFactory(TextSearchAdpter adapter)
        {
            _adapter = adapter;
        }

        public TextSearchProvider Create(CreateTextSearchProviderOptions options)
        {
            var providerOptions = new TextSearchProviderOptions
            {
                FunctionToolName = options.ToolName,
                FunctionToolDescription = options.ToolDescription,
                SearchTime = options.SearchTime,
            };

            return new TextSearchProvider((query, ct) => _adapter.SearchAsync(options.WorkspaceId, query, options.Context, ct), providerOptions);
        }
    }

    public record CreateTextSearchProviderOptions(
        int WorkspaceId,
        string? Context,
        string ToolName,
        string ToolDescription,
        TextSearchProviderOptions.TextSearchBehavior SearchTime
    );
}