namespace ChatAgentic.Features.AI.Usage;

public sealed class EmbeddingAggregateUsageReport : IAIUsageReport
{
    public EmbeddingAggregateUsageReport(string provider, long inputTokens, long outputTokens)
    {
        Provider = provider;
        Input = inputTokens;
        Output = outputTokens;
    }

    public string Provider { get; }
    public AIUsageService Service => AIUsageService.Embedding;
    public long Input { get; }
    public long Output { get; }
    public decimal Cost => 0m;
}
