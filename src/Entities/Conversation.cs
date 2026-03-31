using ChatAgentic.Features.Channels;

namespace ChatAgentic.Entities
{
    public class Conversation
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpireAt { get; set; }
        public ChannelType Channel { get; set; }
        public string SenderIdentifier { get; set; } = string.Empty;
        public string? ChatId { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = default!;
        public List<ConversationMessage> Messages { get; set; } = default!;
    }
}