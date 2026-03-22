using ChatAgentic.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace ChatAgentic.Services
{
    public class KnowledgeBaseIngestor
    {
        private readonly DocumentExtractor _docExtractor;
        private readonly EmbeddingService _embedService;
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public KnowledgeBaseIngestor(DocumentExtractor docExtractor, EmbeddingService embedService,
            AppDbContext dbContext, ILogger<KnowledgeBaseIngestor> logger)
        {
            _docExtractor = docExtractor;
            _embedService = embedService;
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task ExecuteAsync(KnowledgeBaseIngestorInput input)
        {
            _logger.LogDebug("Start knowledge ingestion for {filename}", input.Filename);

            using var fileStream = input.File;

            if (string.IsNullOrEmpty(input.Token))
            {
                _logger.LogDebug("Integration token is empty");
                return;
            }

            var workspaceId = await _dbContext.Workspaces.Where(x => x.IntegrationToken == input.Token).Select(x => x.Id).FirstOrDefaultAsync();
            if (workspaceId == default)
            {
                _logger.LogError("Integration token not found");
                return;
            }

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
            foreach (var chunk in chunks)
            {
                chunkCount++;

                _logger.LogDebug("Embed chunk {index}/{count}", chunkCount, chunks.Length);

                var embed = await _embedService.EmbedAsync(chunk);
                _dbContext.Knowledges.Add(new Models.Knowledge
                {
                    WorkspaceId = workspaceId,
                    CreatedAt = currentDate,
                    Context = input.Context,
                    Source = input.Filename,
                    Content = chunk,
                    Embedding = new Vector(embed),
                });
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            await _dbContext.Knowledges.Where(x => x.WorkspaceId == workspaceId && x.Context == input.Context && x.Source == input.Filename).ExecuteDeleteAsync();
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogDebug("Knowledge ingestion done");
        }
    }

    public record KnowledgeBaseIngestorInput(
        string Token,
        string Context,
        string Filename,
        Stream File,
        ChunkerType ChunkerType,
        bool ClearText
    );
}