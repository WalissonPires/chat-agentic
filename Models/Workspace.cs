namespace ChatAgentic.Models
{
    public class Workspace
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? WebhookToken { get; set; }
        public WorkspaceMetadata? Metadata { get; set; }
    }

    public class WorkspaceMetadata
    {
        public int SchemaVersion { get; set; }
        public string? ProviderUrl { get; set; }
        public string? ProviderApiKey { get; set; }
        public string? ChatModel { get; set; }
        public string? EmbedModel { get; set; }
        public string? TranscriptionModel { get; set; }
        public string? TtsModel { get; set; }
        public string? TtsVoice { get; set; }
        public string[]? Tools { get; set; }
    }
}