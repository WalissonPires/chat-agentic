using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;

namespace ChatAgentic.Entities
{
    public class Workflow
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string WebhookToken { get; set; } = string.Empty;
        public WorkflowMetadata? Metadata { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = default!;

    }

    public class WorkflowMetadata
    {
        public WorkflowAgentOptions? Agent { get; set; }
        public EvolutionApiOptions? EvolutionApi { get; set; }
        public TelegramApiOptions? Telegram { get; set; }
    }

    public class WorkflowAgentOptions
    {
        public string? Instructions { get; set; }
        public bool UseStructuredOutput { get; set; } = true;
        public bool EnableTools { get; set; } = true;
        public bool EnableContextProviders { get; set; } = true;
        public bool EnableAgentMiddleware { get; set; } = true;
        public bool StrictToolNameValidation { get; set; } = true;
    }
}
