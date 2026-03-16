
namespace ChatAgentic.Services
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
                int commaIdx = mediaUri.IndexOf(',');
                if (commaIdx < 0)
                    throw new ArgumentException("Invalid data-URI: missing comma separator.", nameof(mediaUri));

                // Extract MIME from "data:<mime>;base64"
                //string meta = mediaUri[5..commaIdx];                     // e.g. "audio/webm;base64"
                //var mimeType = meta.Split(';')[0];                       // e.g. "audio/webm"
                var base64Data = mediaUri[(commaIdx + 1)..];               // raw Base64 bytes
                byte[] bytes = Convert.FromBase64String(base64Data);
                return new MemoryStream(bytes); ;
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