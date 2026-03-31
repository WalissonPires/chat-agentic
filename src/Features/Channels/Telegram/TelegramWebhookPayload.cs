using System.Text.Json.Serialization;

namespace ChatAgentic.Features.Channels.Telegram
{
    public class TelegramWebhookPayload
    {
        [JsonPropertyName("update_id")]
        public long UpdateId { get; set; }
        public TelegramMessageData? Message { get; set; }
    }

    public class TelegramMessageData
    {
        [JsonPropertyName("message_id")]
        public int MessageId { get; set; }
        public TelegramChat? Chat { get; set; }
        public string? Text { get; set; }
        public string? Caption { get; set; }
        public TelegramFileInfo? Voice { get; set; }
        public TelegramFileInfo? Audio { get; set; }
        public TelegramFileInfo? Document { get; set; }
        public List<TelegramPhoto>? Photo { get; set; }
        public TelegramUser? From { get; set; }
    }

    public class TelegramUser
    {
        public string? Username { get; set; }
    }

    public class TelegramChat
    {
        public long Id { get; set; }
    }

    public class TelegramFileInfo
    {
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }
        [JsonPropertyName("file_name")]
        public string? FileName { get; set; }
        [JsonPropertyName("mime_type")]
        public string? MimeType { get; set; }
    }

    public class TelegramPhoto
    {
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        [JsonPropertyName("file_size")]
        public int FileSize { get; set; }
    }

    public class TelegramGetFileResponse
    {
        public bool Ok { get; set; }
        public TelegramFileResult? Result { get; set; }
    }

    public class TelegramFileResult
    {
        [JsonPropertyName("file_id")]
        public string? FileId { get; set; }
        [JsonPropertyName("file_unique_id")]
        public string? FileUniqueId { get; set; }
        [JsonPropertyName("file_size")]
        public int FileSize { get; set; }
        [JsonPropertyName("file_path")]
        public string? FilePath { get; set; }
    }

    public class TelegramApiResponse
    {
        public bool Ok { get; set; }
        public string? Description { get; set; }
    }
}
