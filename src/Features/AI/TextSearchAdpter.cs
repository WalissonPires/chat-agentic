using ChatAgentic.Features.AI.Usage;
using ChatAgentic.Persistence;
using Microsoft.Agents.AI;
using Pgvector;
using Pgvector.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Features.AI
{
    public class TextSearchAdpter
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;
        private readonly EmbeddingService _embedService;
        private readonly IAIUsageHistoryRepository _usageHistoryRepository;

        public TextSearchAdpter(ILogger<TextSearchAdpter> logger, AppDbContext dbContext, EmbeddingService embedService,
            IAIUsageHistoryRepository usageHistoryRepository)
        {
            _dbContext = dbContext;
            _logger = logger;
            _embedService = embedService;
            _usageHistoryRepository = usageHistoryRepository;
        }

        public async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAsync(int workspaceId, string query, string? context, CancellationToken ct = default)
        {
            _logger.LogInformation("RAG Search called with query '{query}'", query);

            if (string.IsNullOrEmpty(query))
                return [];

            var embedResult = await _embedService.EmbedAsync(query, ct);
            var queryVector = new Vector(embedResult.Vector);

            await _usageHistoryRepository.AddAsync(AIUsageHistoryFactory.Create(workspaceId, conversationId: null, embedResult), ct);
            var maxDistance = 0.7f;
            var topK = 5;

            var knowledgeQuery = _dbContext.Knowledges.Where(x => x.WorkspaceId == workspaceId);

            if (!string.IsNullOrEmpty(context))
                knowledgeQuery = knowledgeQuery.Where(x => x.Context == context);

            var results = await knowledgeQuery
                .OrderBy(x => x.Embedding.CosineDistance(queryVector))
                .Where(x => x.Embedding.CosineDistance(queryVector) < maxDistance)
                .Take(topK)
                .Select(x => new TextSearchProvider.TextSearchResult
                {
                    SourceName = x.Context,
                    SourceLink = x.Source,
                    Text = x.Content
                })
                .ToListAsync(ct);

            _logger.LogDebug("{resultCount} result(s) found.", results.Count);

            foreach (var r in results)
            {
                _logger.LogDebug("{resultContent}", r.Text[..Math.Min(r.Text.Length, 100)]);
            }

            return results;
        }
    }
}