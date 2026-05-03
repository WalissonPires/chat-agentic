namespace ChatAgentic.Features.Channels
{
    public interface IChannelMessageTransform
    {
        Task<ChannelMessageTransformResult> Execute(ChannelMessageTransformInput input);
    }

    public record ChannelMessageTransformInput(
        int WorkspaceId,
        int AgentDefinitionId,
        string JsonPayload
    );

    public record ChannelMessageTransformResult(
        bool SelfMessage,
        Message Message
    );
}