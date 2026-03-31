using ChatAgentic.Features.Channels;
using ChatAgentic.Entities;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Workflows
{
    public record WorkflowExecutionContext(
        int WorkspaceId,
        int ConversationId,
        ChannelType Channel,
        string SenderIdentifier,
        string? ChatId,
        List<PersonMetadataItem> ContactMetadata,
        bool ReceiveidAudio,
        List<Message> InputMessages,
        List<ChatMessage> OutputMessages,
        List<ChatMessage> LastMessages,
        List<ChatMessage> OutputAudioMessages
    );
}