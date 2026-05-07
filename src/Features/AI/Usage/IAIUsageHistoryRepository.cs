using ChatAgentic.Entities;

namespace ChatAgentic.Features.AI.Usage;

public interface IAIUsageHistoryRepository
{
    Task AddAsync(AIUsageHistory item, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<AIUsageHistory> items, CancellationToken cancellationToken = default);
}
