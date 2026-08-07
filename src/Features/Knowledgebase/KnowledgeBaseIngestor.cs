using ChatAgentic.Features.AI;
using ChatAgentic.Features.AI.Usage;
using ChatAgentic.Persistence;
using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace ChatAgentic.Features.Knowledgebase
{
    public class KnowledgeBaseIngestor
    {
        private readonly DocumentExtractor _docExtractor;
        private readonly EmbeddingService _embedService;
        private readonly AppDbContext _dbContext;
        private readonly IAIUsageHistoryRepository _usageHistoryRepository;
        private readonly ILogger _logger;

        public KnowledgeBaseIngestor(DocumentExtractor docExtractor, EmbeddingService embedService, AppDbContext dbContext,
            IAIUsageHistoryRepository usageHistoryRepository, ILogger<KnowledgeBaseIngestor> logger)
        {
            _docExtractor = docExtractor;
            _embedService = embedService;
            _dbContext = dbContext;
            _usageHistoryRepository = usageHistoryRepository;
            _logger = logger;
        }

        public async Task ExecuteAsync(KnowledgeBaseIngestorInput input)
        {
            _logger.LogDebug("Start knowledge ingestion for {filename}", input.Filename);

            using var fileStream = input.File;

            _logger.LogDebug("Extract document content");
            var fileContent = await _docExtractor.ExtractTextAsync(input.Filename, fileStream);

            if (input.ClearText)
            {
                _logger.LogDebug("Clear document content");
                fileContent = TextCleaner.Clean(fileContent);
            }

            _logger.LogDebug("Chunk document content");
            var chunks = DocumentChunker.Split(fileContent, input.ChunkerType).ToArray();

            fileContent = null;

            var currentDate = DateTime.UtcNow;

            var chunkCount = 0;
            long totalInput = 0;
            long totalOutput = 0;
            string? embedProvider = null;

            foreach (var chunk in chunks)
            {
                chunkCount++;

                _logger.LogDebug("Embed chunk {index}/{count}", chunkCount, chunks.Length);

                var embedResult = await _embedService.EmbedAsync(chunk);
                embedProvider ??= embedResult.Provider;
                totalInput += embedResult.Input;
                totalOutput += embedResult.Output;

                _dbContext.Knowledges.Add(new Knowledge
                {
                    WorkspaceId = input.WorkspaceId,
                    CreatedAt = currentDate,
                    Context = input.Context,
                    Source = input.Filename,
                    Content = chunk,
                    Embedding = new Vector(embedResult.Vector),
                });
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            await _dbContext.Knowledges.Where(x => x.WorkspaceId == input.WorkspaceId && x.Context == input.Context && x.Source == input.Filename).ExecuteDeleteAsync();
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            if (chunks.Length > 0 && embedProvider is not null)
            {
                var report = new EmbeddingAggregateUsageReport(embedProvider, totalInput, totalOutput);
                await _usageHistoryRepository.AddAsync(AIUsageHistoryFactory.Create(input.WorkspaceId, conversationId: null, report));
            }

            _logger.LogDebug("Knowledge ingestion done");
        }
    }

    public record KnowledgeBaseIngestorInput(
        int WorkspaceId,
        string Context,
        string Filename,
        Stream File,
        ChunkerType ChunkerType,
        bool ClearText
    );
}