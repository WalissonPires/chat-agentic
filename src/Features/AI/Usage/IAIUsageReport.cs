namespace ChatAgentic.Features.AI.Usage;

public interface IAIUsageReport
{
    string Provider { get; }
    AIUsageService Service { get; }
    long Input { get; }
    long Output { get; }
    decimal Cost { get; }
}
