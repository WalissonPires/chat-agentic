using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Channels
{
    public record Message(
        int WorkspaceId,
        ChannelType Channel,
        string SenderIdentifier,
        string? ChatId,
        MessageContentType ContentType,
        string ContentText,
        string? MediaUri = null,
        string? FileName = null,
        string? MimeType = null
    );

    public enum MessageContentType
    {
        Text = 1,
        Audio = 2,
        Image = 3,
        Video = 4,
        Document = 5
    }

    public static class MessageExtensions
    {
        public static ChatMessage ToChatMessage(this Message message)
        {
            var chatMessage = new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                Role = ChatRole.User,
                CreatedAt = DateTime.UtcNow,
                Contents = []
            };

            switch (message.ContentType)
            {
                case MessageContentType.Text:
                    chatMessage.Contents.Add(new TextContent(message.ContentText));
                    break;
                case MessageContentType.Audio:
                case MessageContentType.Image:
                case MessageContentType.Video:
                case MessageContentType.Document:
                    if (!string.IsNullOrEmpty(message.ContentText))
                        chatMessage.Contents.Add(new TextContent(message.ContentText));
                    if (!string.IsNullOrEmpty(message.MediaUri))
                        chatMessage.Contents.Add(new UriContent(message.MediaUri, message.MimeType ?? "application/octet-stream"));
                    break;
            }

            return chatMessage;
        }
    }
}