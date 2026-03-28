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

        public TextSearchAdpter(ILogger<TextSearchAdpter> logger, AppDbContext dbContext, EmbeddingService embedService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _embedService = embedService;
        }

        public async Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchAsync(int workspaceId, string query, string? context, CancellationToken ct = default)
        {
            _logger.LogInformation("RAG Search called with query '{query}'", query);

            if (string.IsNullOrEmpty(query))
                return [];

            var queryEmbed = await _embedService.EmbedAsync(query);
            var queryVector = new Vector(queryEmbed);
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