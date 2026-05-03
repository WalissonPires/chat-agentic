using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;
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
    }

    public class AgentDefinitionMetadata
    {
        public AgentOptions? Agent { get; set; }
        public EvolutionApiOptions? EvolutionApi { get; set; }
        public TelegramApiOptions? Telegram { get; set; }
    }
}
