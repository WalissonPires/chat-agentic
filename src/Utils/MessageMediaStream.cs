namespace ChatAgentic.Utils
{
    public class MessageMediaStream
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public MessageMediaStream(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<Stream> GetMediaStream(string mediaUri)
        {
            if (string.IsNullOrEmpty(mediaUri))
                throw new ArgumentException("MediaUri is empty");

            if (mediaUri.StartsWith("file://", StringComparison.InvariantCultureIgnoreCase))
            {
                return File.OpenRead(mediaUri[7..]);
            }

            if (mediaUri.StartsWith("data:", StringComparison.InvariantCultureIgnoreCase))
            {
                var bytes = Convert.FromBase64String(new DataUri(mediaUri).Base64);
                return new MemoryStream(bytes);
            }

            if (mediaUri.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
            {
                var client = _httpClientFactory.CreateClient();
                var request = new HttpRequestMessage(HttpMethod.Get, mediaUri);
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadAsStreamAsync();
            }

            throw new InvalidOperationException("Invalid MediaUri");
        }
    }
}