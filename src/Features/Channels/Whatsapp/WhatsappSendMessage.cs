using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Channels.Whatsapp
{
    public class WhatsappSendMessage : IChannelSendMessage
    {
        private readonly EvolutionApiClient _evolutionClient;
        private readonly ILogger _logger;

        public WhatsappSendMessage(EvolutionApiClient evolutionClient, ILogger<WhatsappSendMessage> logger)
        {
            _evolutionClient = evolutionClient;
            _logger = logger;
        }

        public async Task ExecuteAsync(ChannelSendMessageInput input, CancellationToken ct = default)
        {
            foreach (var content in input.Message.Contents)
            {
                switch (content)
                {
                    case TextContent textContent:
                        _logger.LogDebug("Sending text message");
                        await _evolutionClient.SendTextMessageAsync(input.SenderIdentifier, textContent.Text, ct);
                        break;
                    case UriContent uriContent:
                        if (uriContent.MediaType.StartsWith("audio", StringComparison.InvariantCultureIgnoreCase))
                        {
                            _logger.LogDebug("Sending audio message");
                            await _evolutionClient.SendAudioMessageAsync(input.SenderIdentifier, uriContent.Uri.ToString(), ct);
                        }
                        else
                        {
                            MediaType mediaType = MediaType.Document;

                            if (uriContent.MediaType.StartsWith("image", StringComparison.InvariantCultureIgnoreCase))
                                mediaType = MediaType.Image;
                            else if (uriContent.MediaType.StartsWith("video", StringComparison.InvariantCultureIgnoreCase))
                                mediaType = MediaType.Video;

                            _logger.LogDebug("Sending {mediaType} message", mediaType);
                            await _evolutionClient.SendMediaMessageAsync(input.SenderIdentifier, mediaType, uriContent.Uri.ToString(), uriContent.MediaType, null, null, ct);
                        }
                        break;
                }
            }
        }
    }
}