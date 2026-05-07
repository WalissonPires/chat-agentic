using ChatAgentic.Entities;
using ChatAgentic.Persistence;

namespace ChatAgentic.Features.AI.Usage;

public sealed class AIUsageHistoryRepository(AppDbContext dbContext) : IAIUsageHistoryRepository
{
    public async Task AddAsync(AIUsageHistory item, CancellationToken cancellationToken = default)
    {
        dbContext.AIUsageHistories.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<AIUsageHistory> items, CancellationToken cancellationToken = default)
    {
        dbContext.AIUsageHistories.AddRange(items);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
