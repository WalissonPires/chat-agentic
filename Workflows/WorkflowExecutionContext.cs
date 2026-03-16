using ChatAgentic.Channels;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Workflows
{
    public record WorkflowExecutionContext(
        int WorkspaceId,
        int ConversationId,
        ChannelType Channel,
        string SenderIdentifier,
        int LastMessagesCount,
        List<ChatMessage> Messages
    );
}