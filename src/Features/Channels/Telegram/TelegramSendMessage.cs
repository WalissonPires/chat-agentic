using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Channels.Telegram
{
    public class TelegramSendMessage : IChannelSendMessage
    {
        private readonly TelegramApiClient _telegramClient;
        private readonly ILogger _logger;

        public TelegramSendMessage(TelegramApiClient telegramClient, ILogger<TelegramSendMessage> logger)
        {
            _telegramClient = telegramClient;
            _logger = logger;
        }

        public async Task ExecuteAsync(ChannelSendMessageInput input, CancellationToken ct = default)
        {
            if (string.IsNullOrEmpty(input.ChatId))
                throw new Exception("ChatId is required");

            foreach (var content in input.Message.Contents)
            {
                switch (content)
                {
                    case TextContent textContent:
                        _logger.LogDebug("Sending text message");
                        await _telegramClient.SendTextMessageAsync(input.ChatId, textContent.Text, ct);
                        break;
                    case UriContent uriContent:
                        if (uriContent.MediaType.StartsWith("audio", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _logger.LogDebug("Sending audio message");
                            await _telegramClient.SendAudioMessageAsync(input.ChatId, uriContent.Uri.ToString(), uriContent.MediaType, ct);
                        }
                        else if (uriContent.MediaType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _logger.LogDebug("Sending image message");
                            await _telegramClient.SendPhotoMessageAsync(input.ChatId, uriContent.Uri.ToString(), uriContent.MediaType, null, null, ct);
                        }
                        else
                        {
                            _logger.LogDebug("Sending document message");
                            await _telegramClient.SendDocumentMessageAsync(input.ChatId, uriContent.Uri.ToString(), uriContent.MediaType, null, null, ct);
                        }
                        break;
                }
            }
        }
    }
}
