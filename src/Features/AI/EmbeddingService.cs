using Microsoft.Extensions.AI;
using OpenAI;

namespace ChatAgentic.Features.AI
{
    public class EmbeddingService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embedGenerator;

        public EmbeddingService(AIProviderOptions options)
        {
            var apiKey = options.ApiKey ?? throw new Exception("AIProvider ApiKey is empty");
            var emdedModel = options.EmbedModel ?? throw new Exception("AIProvider EmbedModel is empty");
            _embedGenerator = new OpenAIClient(apiKey).GetEmbeddingClient(emdedModel).AsIEmbeddingGenerator();
        }

        public async Task<ReadOnlyMemory<float>> EmbedAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new ReadOnlyMemory<float>();

            var result = await _embedGenerator.GenerateAsync(text);
            return result.Vector;
        }
    }
}