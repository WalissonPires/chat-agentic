using System.ClientModel;
using ChatAgentic.Features.AI.Usage;
using Microsoft.Extensions.AI;
using OpenAI;

namespace ChatAgentic.Features.AI
{
    public class EmbeddingService
    {
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embedGenerator;
        private readonly string _provider;

        public EmbeddingService(AIProviderOptions options)
        {
            var apiKey = options.ApiKey ?? throw new Exception("AIProvider ApiKey is empty");
            var emdedModel = options.EmbedModel ?? throw new Exception("AIProvider EmbedModel is empty");
            var endpoint = options.Endpoint;

            _provider = AIProviderName.FromEndpoint(endpoint);

            _embedGenerator = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
            {
                Endpoint = string.IsNullOrEmpty(endpoint) ? null : new Uri(endpoint),
            }).GetEmbeddingClient(emdedModel).AsIEmbeddingGenerator();
        }

        public async Task<EmbeddingResult> EmbedAsync(string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return new EmbeddingResult
                {
                    Vector = ReadOnlyMemory<float>.Empty,
                    Provider = _provider,
                    Input = 0,
                    Output = 0,
                    Cost = 0m
                };
            }

            var generated = await _embedGenerator.GenerateAsync([text], cancellationToken: cancellationToken);
            var (input, output) = AIUsageTokenMapper.FromUsageDetails(generated.Usage);
            var vector = generated.Count > 0 ? generated[0].Vector : ReadOnlyMemory<float>.Empty;

            return new EmbeddingResult
            {
                Vector = vector,
                Provider = _provider,
                Input = input,
                Output = output,
                Cost = 0m
            };
        }
    }
}