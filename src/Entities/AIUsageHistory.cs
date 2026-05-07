using ChatAgentic.Features.AI.Usage;

namespace ChatAgentic.Entities;

public class AIUsageHistory
{
    public int Id { get; set; }
    public int WorkspaceId { get; set; }
    public int? ConversationId { get; set; }
    public string Provider { get; set; } = string.Empty;
    public AIUsageService Service { get; set; }
    public long Input { get; set; }
    public long Output { get; set; }
    public decimal Cost { get; set; }
    public DateTime CreatedAt { get; set; }
}
