using ChatAgentic.Utils;

namespace ChatAgentic.Features.Channels.Whatsapp
{

    public class EvolutionApiClient
    {
        private readonly HttpClient _client;
        private readonly string _instanceName;

        public EvolutionApiClient(EvolutionApiOptions options, IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri(options.ServerUrl ?? throw new Exception("ApiUrl is empty"));
            _client.DefaultRequestHeaders.Add("apikey", options.ApiKey ?? throw new Exception("ApiKey is empty"));
            _client.Timeout = TimeSpan.FromSeconds(10);

            _instanceName = options.Instance ?? throw new Exception("Instance is empty");
        }

        public async Task<EvolutionMedia> DownloadMediaAsync(string messageKeyId, CancellationToken ct = default)
        {
            var data = new { message = new { key = new { id = messageKeyId } } };
            var url = $"chat/getBase64FromMediaMessage/{_instanceName}";

            var response = await _client.PostAsJsonAsync(url, data, ct);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EvolutionMediaBase64>(ct);

            if (string.IsNullOrEmpty(result?.Base64))
                throw new Exception("Server returned empty response");

            var media = new MemoryStream(Convert.FromBase64String(result.Base64));

            return new EvolutionMedia(
                MimeType: result.Mimetype ?? "application/octet-stream",
                Filename: result.FileName ?? string.Empty,
                Media: media
            );
        }

        public async Task SendTextMessageAsync(string phone, string message, CancellationToken ct = default)
        {
            var data = new { number = phone, text = message, delay = 800 };
            var url = $"message/sendText/{_instanceName}";

            var response = await _client.PostAsJsonAsync(url, data, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendAudioMessageAsync(string phone, string audioUri, CancellationToken ct = default)
        {
            var uriOrBase64 = audioUri.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase) ? new DataUri(audioUri).Base64 : audioUri;
            var data = new { number = phone, audio = uriOrBase64, delay = 800 };
            var url = $"message/sendWhatsAppAudio/{_instanceName}";

            var response = await _client.PostAsJsonAsync(url, data, ct);
            response.EnsureSuccessStatusCode();
        }

        public async Task SendMediaMessageAsync(string phone, MediaType mediaType, string mediaUri, string mimeType, string? caption, string? filename, CancellationToken ct = default)
        {
            var data = new
            {
                number = phone,
                mediatype = mediaType.ToString().ToLower(),
                mimetype = mimeType,
                caption = caption,
                media = mediaUri,
                fileName = filename,
                delay = 800,
            };

            var url = $"message/sendMedia/{_instanceName}";

            var response = await _client.PostAsJsonAsync(url, data, ct);
            response.EnsureSuccessStatusCode();
        }
    }

    public record EvolutionMedia(
        string MimeType,
        string Filename,
        Stream Media
    );

    public enum MediaType
    {
        Image = 1,
        Video = 2,
        Document = 3
    }
}