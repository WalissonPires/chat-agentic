using ChatAgentic.Channels;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Workflows
{
    public record WorkflowExecutionContext(
        int WorkspaceId,
        int ConversationId,
        ChannelType Channel,
        string SenderIdentifier,
        bool ReceiveidAudio,
        List<Message> InputMessages,
        List<ChatMessage> OutputMessages,
        List<ChatMessage> LastMessages
    );
}