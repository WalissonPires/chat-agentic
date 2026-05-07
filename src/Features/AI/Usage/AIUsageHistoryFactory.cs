using ChatAgentic.Entities;

namespace ChatAgentic.Features.AI.Usage;

internal static class AIUsageHistoryFactory
{
    public static AIUsageHistory Create(int workspaceId, int? conversationId, IAIUsageReport report)
    {
        return new AIUsageHistory
        {
            WorkspaceId = workspaceId,
            ConversationId = conversationId,
            Provider = report.Provider,
            Service = report.Service,
            Input = report.Input,
            Output = report.Output,
            Cost = report.Cost,
            CreatedAt = DateTime.UtcNow
        };
    }
}
