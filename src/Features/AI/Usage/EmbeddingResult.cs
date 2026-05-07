namespace ChatAgentic.Features.AI.Usage;

public sealed class EmbeddingResult : IAIUsageReport
{
    public required ReadOnlyMemory<float> Vector { get; init; }
    public required string Provider { get; init; }
    public AIUsageService Service => AIUsageService.Embedding;
    public long Input { get; init; }
    public long Output { get; init; }
    public decimal Cost { get; init; }
}
