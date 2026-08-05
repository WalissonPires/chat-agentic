using ChatAgentic.Features.AI;

namespace ChatAgentic.Entities
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? IntegrationToken { get; set; }
        public WorkspaceMetadata? Metadata { get; set; }
        public List<Channel> Channels { get; set; } = [];
    }

    public class WorkspaceMetadata
    {
        public AIProviderOptions? AIProvider { get; set; }
        public int? NotificationWhatsappChannelId { get; set; }
        public int? NotificationTelegramChannelId { get; set; }
    }
}