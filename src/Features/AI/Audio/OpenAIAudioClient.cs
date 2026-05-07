using System.ClientModel;
using OpenAI;
using OpenAI.Audio;

namespace ChatAgentic.Features.AI.Audio
{
    public sealed class OpenAIAudioClient : IAudioClient
    {
        private readonly AudioClient _audioClient;

        public OpenAIAudioClient(AIProviderOptions options)
        {
            var apiKey = options.ApiKey ?? throw new InvalidOperationException("AIProvider APIKey not defined.");
            var model = options.TranscriptionModel ?? throw new InvalidOperationException("AIProvider TranscriptionModel not defined.");
            var endpoint = options.Endpoint;

            _audioClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
            {
                Endpoint = string.IsNullOrEmpty(endpoint) ? null : new Uri(endpoint),
            }).GetAudioClient(model);
        }

        public async Task<string> TranscribeAudioAsync(Stream audioStream, string mimeType, CancellationToken cancellationToken = default)
        {
            var extension = AudioTranscriptionFormats.MapMimeToFormat(mimeType);

            AudioTranscriptionOptions options = new()
            {
                Language = "pt",
                ResponseFormat = AudioTranscriptionFormat.Text,
            };

            var result = await _audioClient.TranscribeAudioAsync(audioStream, $"audio.{extension}", options, cancellationToken).ConfigureAwait(false);
            var text = result.Value.Text.Trim();

            Console.WriteLine(result.Value.Usage.ToString());
            return text;
        }
    }
}
