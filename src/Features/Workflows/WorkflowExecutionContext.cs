using ChatAgentic.Features.Channels;
using ChatAgentic.Features.AI.Agent;
using ChatAgentic.Entities;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Workflows
{
    public record WorkflowExecutionContext(
        int WorkspaceId,
        int AgentDefinitionId,
        int ConversationId,
        ChannelType Channel,
        string SenderIdentifier,
        string? ChatId,
        List<PersonMetadataItem> ContactMetadata,
        bool ReceiveidAudio,
        List<Message> InputMessages,
        List<AgentStructuredResponse> OutputStructuredResponses,
        List<ChatMessage> LastMessages,
        List<ChatMessage> OutputAudioMessages,
        AgentOptions AgentOptions
    );
}