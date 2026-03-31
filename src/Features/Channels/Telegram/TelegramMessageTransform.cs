using System.Text.Json;
using ChatAgentic.Persistence;

namespace ChatAgentic.Features.Channels.Telegram
{
    public class TelegramMessageTransform : IChannelMessageTransform
    {
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
        };

        private readonly ILogger _logger;
        private readonly TelegramApiClient _telegramClient;

        public TelegramMessageTransform(ILogger<TelegramMessageTransform> logger, TelegramApiClient telegramClient)
        {
            _logger = logger;
            _telegramClient = telegramClient;
        }

        public async Task<ChannelMessageTransformResult> Execute(ChannelMessageTransformInput input)
        {
            _logger.LogDebug("Process telegram message");
            _logger.LogDebug(input.JsonPayload);

            if (string.IsNullOrEmpty(input.JsonPayload))
                throw new ArgumentException("JSON payload is empty");

            var payload = JsonSerializer.Deserialize<TelegramWebhookPayload>(input.JsonPayload, _jsonOptions);
            var data = payload?.Message ?? throw new Exception("Invalid message payload");
            var chatId = data.Chat?.Id.ToString() ?? throw new Exception("Telegram chat id is empty");
            var username = data.From?.Username ?? throw new Exception("Telegram username is empty");

            MessageContentType contentType;
            string contentText;
            string? fileId = null;
            string? fileName = null;
            string? mimeType = null;

            if (!string.IsNullOrWhiteSpace(data.Text))
            {
                contentType = MessageContentType.Text;
                contentText = data.Text;
            }
            else if (data.Voice?.FileId is not null || data.Audio?.FileId is not null)
            {
                contentType = MessageContentType.Audio;
                contentText = data.Caption ?? string.Empty;
                fileId = data.Voice?.FileId ?? data.Audio?.FileId;
                fileName = data.Audio?.FileName;
                mimeType = data.Voice?.MimeType ?? data.Audio?.MimeType ?? "audio/ogg";
            }
            else if (data.Photo?.Count > 0)
            {
                contentType = MessageContentType.Image;
                contentText = data.Caption ?? string.Empty;
                fileId = data.Photo.OrderByDescending(x => x.FileSize).FirstOrDefault()?.FileId;
                mimeType = "image/jpeg";
            }
            else if (data.Document?.FileId is not null)
            {
                contentType = MessageContentType.Document;
                contentText = data.Caption ?? string.Empty;
                fileId = data.Document.FileId;
                fileName = data.Document.FileName;
                mimeType = data.Document.MimeType ?? "application/octet-stream";
            }
            else
            {
                throw new Exception("Invalid message type");
            }

            string? mediaUri = null;
            if (!string.IsNullOrWhiteSpace(fileId))
            {
                var mediaResult = await _telegramClient.DownloadFileAsync(fileId);
                using var mediaStream = mediaResult.Media;

                var filename = Path.GetTempFileName();
                using var fileStream = File.Open(filename, FileMode.Create);
                await mediaStream.CopyToAsync(fileStream);
                mediaUri = "file://" + filename;
            }

            var message = new Message(
                WorkspaceId: input.WorkspaceId,
                Channel: ChannelType.Telegram,
                SenderIdentifier: username,
                ChatId: chatId,
                ContentType: contentType,
                ContentText: contentText,
                MediaUri: mediaUri,
                FileName: fileName,
                MimeType: mimeType
            );

            return new ChannelMessageTransformResult(false, message);
        }
    }
}
