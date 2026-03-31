using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Channels
{
    public interface IChannelSendMessage
    {
        Task ExecuteAsync(ChannelSendMessageInput input,  CancellationToken ct = default);
    }

    public record ChannelSendMessageInput(
        string SenderIdentifier,
        string? ChatId,
        ChatMessage Message
    );
}