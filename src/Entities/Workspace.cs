using ChatAgentic.Features.AI;
using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;

namespace ChatAgentic.Entities
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? WebhookToken { get; set; }
        public string? IntegrationToken { get; set; }
        public WorkspaceMetadata? Metadata { get; set; }
    }

    public class WorkspaceMetadata
    {
        public AIProviderOptions? AIProvider { get; set; }
        public EvolutionApiOptions? EvolutionApi { get; set; }
        public TelegramApiOptions? Telegram { get; set; }
    }
}