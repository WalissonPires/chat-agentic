using ChatAgentic.Features.AI.Agent;

namespace ChatAgentic.Entities
{
    public class AgentDefinition
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string WebhookToken { get; set; } = string.Empty;
        public AgentDefinitionMetadata? Metadata { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = default!;

        public int? WhatsappChannelId { get; set; }
        public Channel? WhatsappChannel { get; set; }

        public int? TelegramChannelId { get; set; }
        public Channel? TelegramChannel { get; set; }
    }

    public class AgentDefinitionMetadata
    {
        public AgentOptions? Agent { get; set; }
    }
}
