using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ChatAgentic.Features.AI.Audio
{
    public sealed class AudioClientOpenRouter : IAudioClient
    {
        public static bool IsOpenRouterEndpoint(string? endpoint) => !string.IsNullOrEmpty(endpoint) &&  endpoint.Contains("openrouter.ai", StringComparison.OrdinalIgnoreCase);

        private readonly HttpClient _httpClient;
        private readonly string _model;

        public AudioClientOpenRouter(AIProviderOptions options, IHttpClientFactory httpClientFactory)
        {
            var apiKey = options.ApiKey ?? throw new InvalidOperationException("AIProvider APIKey not defined.");
            var endpoint = options.Endpoint ?? throw new InvalidOperationException("AIProvider Endpoint is required for OpenRouter STT.");
            _model = options.TranscriptionModel ?? throw new InvalidOperationException("AIProvider TranscriptionModel not defined.");

            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri(endpoint);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream, string mimeType, CancellationToken cancellationToken = default)
        {
            var format = AudioTranscriptionFormats.MapMimeToFormat(mimeType);
            using var ms = new MemoryStream();
            await audioStream.CopyToAsync(ms, cancellationToken);
            var b64 = Convert.ToBase64String(ms.ToArray());

            var payload = new Dictionary<string, object?>
            {
                ["model"] = _model,
                ["language"] = "pt",
                ["input_audio"] = new Dictionary<string, string>
                {
                    ["data"] = b64,
                    ["format"] = format,
                },
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "audio/transcriptions");
            request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            response.EnsureSuccessStatusCode();

            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("text", out var textEl))
                throw new InvalidOperationException("OpenRouter STT response did not contain a \"text\" field.");

            var text = (textEl.GetString() ?? string.Empty).Trim();
            return text;
        }
    }
}
