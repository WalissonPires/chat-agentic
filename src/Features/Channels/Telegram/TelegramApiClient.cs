using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using ChatAgentic.Utils;
using Microsoft.Extensions.Options;

namespace ChatAgentic.Features.Channels.Telegram
{
    public class TelegramApiClient
    {
        private readonly HttpClient _client;
        private readonly string _fileBaseUrl;

        public TelegramApiClient(IOptions<TelegramApiOptions> options, IHttpClientFactory httpClientFactory)
        {
            var botToken = options.Value.BotToken ?? throw new Exception("BotToken is empty");
            var baseUrl = options.Value.BaseUrl ?? throw new Exception("BaseUrl is empty");
            _fileBaseUrl = options.Value.FileBaseUrl ?? throw new Exception("FileBaseUrl is empty");

            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri($"{baseUrl.TrimEnd('/')}/bot{botToken}/");
            _client.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task SendTextMessageAsync(string chatId, string message, CancellationToken ct = default)
        {
            var response = await _client.PostAsJsonAsync("sendMessage", new
            {
                chat_id = chatId,
                text = message
            }, ct);

            await EnsureTelegramSuccessAsync(response, ct);
        }

        public async Task SendAudioMessageAsync(string chatId, string audioUri, string mimeType, CancellationToken ct = default)
        {
            var streamResult = BuildAudioStream(audioUri, mimeType);
            await SendMultipartMediaAsync("sendVoice", "voice", chatId, streamResult.Stream, streamResult.FileName, streamResult.MimeType, ct);
        }

        public async Task SendPhotoMessageAsync(string chatId, string mediaUri, string mimeType, string? fileName, string? caption, CancellationToken ct = default)
        {
            var stream = await OpenMediaStreamAsync(mediaUri, ct);
            await SendMultipartMediaAsync("sendPhoto", "photo", chatId, stream, fileName, mimeType, caption, ct);
        }

        public async Task SendDocumentMessageAsync(string chatId, string mediaUri, string mimeType, string? fileName, string? caption, CancellationToken ct = default)
        {
            var stream = await OpenMediaStreamAsync(mediaUri, ct);
            await SendMultipartMediaAsync("sendDocument", "document", chatId, stream, fileName, mimeType, caption, ct);
        }

        public async Task<TelegramMedia> DownloadFileAsync(string fileId, CancellationToken ct = default)
        {
            var response = await _client.GetAsync($"getFile?file_id={Uri.EscapeDataString(fileId)}", ct);
            response.EnsureSuccessStatusCode();

            var payload = await response.Content.ReadFromJsonAsync<TelegramGetFileResponse>(ct);
            if (payload?.Ok != true || string.IsNullOrWhiteSpace(payload.Result?.FilePath))
                throw new Exception("Invalid getFile response");

            var filePath = payload.Result.FilePath;
            var url = $"{_fileBaseUrl.TrimEnd('/')}/{GetBotPathSegment()}/{filePath}";
            var fileResponse = await _client.GetAsync(url, ct);
            fileResponse.EnsureSuccessStatusCode();

            var media = await fileResponse.Content.ReadAsStreamAsync(ct);
            var mediaType = fileResponse.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
            var fileName = Path.GetFileName(filePath);
            return new TelegramMedia(mediaType, fileName, media);
        }

        private async Task EnsureTelegramSuccessAsync(HttpResponseMessage response, CancellationToken ct)
        {
            response.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<TelegramApiResponse>(ct);
            if (payload?.Ok != true)
                throw new Exception($"Telegram API error: {payload?.Description ?? "unknown"}");
        }

        private async Task SendMultipartMediaAsync(string endpoint, string fieldName, string chatId, Stream media, string? fileName, string mimeType, CancellationToken ct = default)
        {
            await SendMultipartMediaAsync(endpoint, fieldName, chatId, media, fileName, mimeType, null, ct);
        }

        private async Task SendMultipartMediaAsync(string endpoint, string fieldName, string chatId, Stream media, string? fileName, string mimeType, string? caption, CancellationToken ct = default)
        {
            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(chatId), "chat_id");

            if (!string.IsNullOrWhiteSpace(caption))
                content.Add(new StringContent(caption), "caption");

            var streamContent = new StreamContent(media);
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            content.Add(streamContent, fieldName, fileName ?? "file");

            var response = await _client.PostAsync(endpoint, content, ct);
            await EnsureTelegramSuccessAsync(response, ct);
        }

        private async Task<Stream> OpenMediaStreamAsync(string mediaUri, CancellationToken ct)
        {
            if (mediaUri.StartsWith("file://", StringComparison.InvariantCultureIgnoreCase))
            {
                var filename = new Uri(mediaUri).LocalPath;
                return File.OpenRead(filename);
            }

            var response = await _client.GetAsync(mediaUri, ct);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync(ct);;
        }

        private (Stream Stream, string MimeType, string FileName) BuildAudioStream(string audioUri, string mimeType)
        {
            if (audioUri.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
            {
                var data = new DataUri(audioUri);
                var bytes = Convert.FromBase64String(data.Base64);
                var stream = new MemoryStream(bytes);
                var extension = GetFileExtension(data.MimeType, ".ogg");
                return (stream, data.MimeType, $"audio{extension}");
            }

            if (audioUri.StartsWith("file://", StringComparison.InvariantCultureIgnoreCase))
            {
                var filePath = new Uri(audioUri).LocalPath;
                var stream = File.OpenRead(filePath);
                return (stream, mimeType, Path.GetFileName(filePath));
            }

            throw new Exception("Invalid audio URI");
        }

        private string GetBotPathSegment()
        {
            var path = _client.BaseAddress?.AbsolutePath ?? throw new Exception("Telegram base url is not configured");
            return path.Trim('/');
        }

        private static string GetFileExtension(string mimeType, string fallback)
        {
            return mimeType.ToLowerInvariant() switch
            {
                "audio/ogg" => ".ogg",
                "audio/mpeg" => ".mp3",
                "audio/mp4" => ".m4a",
                "audio/wav" => ".wav",
                _ => fallback
            };
        }
    }

    public record TelegramMedia(
        string MimeType,
        string Filename,
        Stream Media
    );
}
