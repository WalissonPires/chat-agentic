using ChatAgentic.Channels;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Models
{
    public class ConversationMessage
    {
        public int Id { get; set; }
        /// <summary>
        /// Format:  MessageId:ContentIndex
        /// ContentIndex has fixed 2 digits
        /// Eg: aG21a1c81fc2:00
        /// </summary>
        public string MessageId { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public string Role { get; set; } = default!;
        public MessageContentType ContentType { get; set; }
        public string ContentText { get; set; } = string.Empty;
        public string? MediaUri { get; set; }
        public string? FileName { get; set; }
        public string? MimeType { get; set; }

        public int ConversationId { get; set; }
        public Conversation Conversation { get; set; } = default!;
    }

    public static class ConversationMessageExtesions
    {
        public static ChatMessage MapToChatMessage(this ConversationMessage message)
        {
            var messageId = message.MessageId[..^3];
            var chatMessage = new ChatMessage
            {
                Role = new ChatRole(message.Role),
                MessageId = messageId,
                CreatedAt = message.CreatedAt,
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
                    if (!string.IsNullOrEmpty(message.MediaUri))
                        chatMessage.Contents.Add(new UriContent(message.MediaUri, message.MimeType ?? "application/octet-stream"));
                    break;
            }

            return chatMessage;
        }

        public static ConversationMessage[] MapToConversationMessage(this ChatMessage chatMessage)
        {
            var messages = new List<ConversationMessage>();

            for (int i = 0; i < chatMessage.Contents.Count; i++)
            {
                var msg = new ConversationMessage
                {
                    Role = chatMessage.Role.ToString(),
                    MessageId = (chatMessage.MessageId ?? Guid.NewGuid().ToString("N")) + ":" + (i.ToString().PadLeft(2, '0')),
                    CreatedAt = chatMessage.CreatedAt?.UtcDateTime ?? DateTime.UtcNow
                };

                var content = chatMessage.Contents[i];

                switch(content)
                {
                    case TextContent c:
                        msg.ContentType = MessageContentType.Text;
                        msg.ContentText = c.Text;
                        break;
                    case FunctionCallContent c:
                        msg.Role = ChatRole.Tool.ToString();
                        msg.ContentType = MessageContentType.Text;
                        msg.ContentText = $"Tool '{c.Name}' called. CallId: {c.CallId}";
                        break;
                    case FunctionResultContent c:
                        msg.Role = ChatRole.Tool.ToString();
                        msg.ContentType = MessageContentType.Text;
                        msg.ContentText = $"Result for call tool '{c.CallId}': " + c.Result;
                        break;
                    default:
                        throw new NotSupportedException("Content type not supported: " + content.GetType().Name);
                }

                messages.Add(msg);
            }

            return messages.ToArray();
        }
    }
}