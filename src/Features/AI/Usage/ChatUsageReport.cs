namespace ChatAgentic.Features.AI.Usage;

public sealed class ChatUsageReport : IAIUsageReport
{
    public required string Provider { get; init; }
    public AIUsageService Service => AIUsageService.Chat;
    public long Input { get; init; }
    public long Output { get; init; }
    public decimal Cost { get; init; }
}
