using Microsoft.Extensions.Options;

namespace ChatAgentic.Channels
{
    public class EvolutionApiOptions
    {
        public string? ServerUrl { get; set; }
        public string? ApiKey { get; set; }
        public string? Instance { get; set; }
    }

    public class EvolutionApiClient
    {
        private readonly HttpClient _client;
        private readonly string _serverUrl;
        private readonly string _instanceName;

        public EvolutionApiClient(IOptions<EvolutionApiOptions> options, IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            //_client.BaseAddress = new Uri(options.Value.ServerUrl ?? throw new Exception("ApiUrl is empty"));
            _client.DefaultRequestHeaders.Add("apikey", options.Value.ApiKey ?? throw new Exception("ApiKey is empty"));
            _client.Timeout = TimeSpan.FromSeconds(10);
            _serverUrl = options.Value.ServerUrl ?? throw new Exception("ApiUrl is empty");
            _instanceName = options.Value.Instance ?? throw new Exception("Instance is empty");
        }

        public async Task<EvolutionMedia> DownloadMediaAsync(string messageKeyId, CancellationToken ct = default)
        {
            var data = new { message = new { key = new { id = messageKeyId } } };
            var url = $"{_serverUrl}/chat/getBase64FromMediaMessage/{_instanceName}";
            var response = await _client.PostAsJsonAsync(url, data, ct);

            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadFromJsonAsync<EvolutionMediaBase64>(ct);

            if (string.IsNullOrEmpty(result?.Base64))
                throw new Exception("Server returned empty response");

            var media = new MemoryStream(Convert.FromBase64String(result.Base64));

            return new EvolutionMedia(
                MimeType: result.Mimetype ?? "octet/stream",
                Filename: result.FileName ?? string.Empty,
                Media: media
            );
        }
    }

    public record EvolutionMedia(
        string MimeType,
        string Filename,
        Stream Media
    );
}