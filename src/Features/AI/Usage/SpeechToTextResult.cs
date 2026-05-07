namespace ChatAgentic.Features.AI.Usage;

public sealed class SpeechToTextResult : IAIUsageReport
{
    public required string Text { get; init; }
    public required string Provider { get; init; }
    public AIUsageService Service => AIUsageService.STT;
    public long Input { get; init; }
    public long Output { get; init; }
    public decimal Cost { get; init; }
}
