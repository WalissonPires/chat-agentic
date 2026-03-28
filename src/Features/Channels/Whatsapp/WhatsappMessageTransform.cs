using System.Text.Json;
using ChatAgentic.Persistence;

namespace ChatAgentic.Features.Channels.Whatsapp
{
    public class WhatsappMessageTransform : IChannelMessageTransform
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;
        private readonly EvolutionApiClient _evolutionClient;

        public WhatsappMessageTransform(ILogger<WhatsappMessageTransform> logger, AppDbContext dbContext, EvolutionApiClient evolutionClient)
        {
            _logger = logger;
            _dbContext = dbContext;
            _evolutionClient = evolutionClient;
        }

        public async Task<ChannelMessageTransformResult> Execute(ChannelMessageTransformInput input)
        {
            _logger.LogDebug("Process whatsapp message");
            _logger.LogDebug(input.JsonPayload);

            if (string.IsNullOrEmpty(input.JsonPayload))
                throw new ArgumentException("JSON payload is empty");

            var payload = JsonSerializer.Deserialize<EvolutionWebhookPayload>(input.JsonPayload, _jsonOptions);
            var data = payload?.Data ?? throw new Exception("Invalid message payload");

            // Extract phone "5511988001122@s.whatsapp.net" => "5511988001122"
            string phone = data.Key?.RemoteJid ?? string.Empty;
            if (phone.Contains('@'))
                phone = phone[..phone.IndexOf('@')];

            string messageType = (data.MessageType ?? "conversation").ToLowerInvariant();
            MessageContentType contentType =
                messageType is "conversation" ? MessageContentType.Text :
                messageType is "audiomessage" or "audiosmessage" or "pttmessage" ? MessageContentType.Audio :
                messageType is "imagemessage" ? MessageContentType.Image :
                messageType is "documentmessage" ? MessageContentType.Document :
                throw new Exception("Invalid message type " + messageType);

            string contentText =
                data.Message?.Conversation ??
                data.Message?.ImageMessage?.Caption ??
                data.Message?.DocumentMessage?.Caption ?? string.Empty;

            string? fileName = data.Message?.DocumentMessage?.FileName;
            string? mimeType = data.Message?.DocumentMessage?.Mimetype;

            // image/png is sended as document
            if (mimeType?.StartsWith("image") == true)
                contentType = MessageContentType.Image;

            string? mediaUri = null;
            if (contentType is  MessageContentType.Audio or MessageContentType.Image or MessageContentType.Document)
            {
                var mediaResult = await _evolutionClient.DownloadMediaAsync(data.Key?.Id ?? string.Empty);

                var filename = Path.GetTempFileName();
                using var fileStream = File.Open(filename, FileMode.Create);
                await mediaResult.Media.CopyToAsync(fileStream);
                mediaUri = "file://" + filename;
            }

            var message = new Message(
                WorkspaceId: input.WorkspaceId,
                Channel: ChannelType.Whatsapp,
                SenderIdentifier: phone,
                ContentType: contentType,
                ContentText: contentText,
                MediaUri: mediaUri,
                FileName: fileName,
                MimeType: mimeType
            );

            return new ChannelMessageTransformResult(
                SelfMessage: data.Key?.FromMe == true,
                message
            );
        }
    }
}