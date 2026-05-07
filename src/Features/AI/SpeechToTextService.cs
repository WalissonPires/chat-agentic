using ChatAgentic.Features.AI.Audio;
using ChatAgentic.Features.AI.Usage;

namespace ChatAgentic.Features.AI
{
    public class SpeechToTextService
    {
        private readonly IAudioClient _audioClient;
        private readonly ILogger<SpeechToTextService> _logger;

        public SpeechToTextService(AIProviderOptions aiProviderOptions, IHttpClientFactory httpClientFactory, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SpeechToTextService>();

            _audioClient = AudioClientOpenRouter.IsOpenRouterEndpoint(aiProviderOptions.Endpoint)
                ? new AudioClientOpenRouter(aiProviderOptions, httpClientFactory)
                : new OpenAIAudioClient(aiProviderOptions);

            _logger.LogDebug("SpeechToTextService using client: {Client}, model: {Model}", _audioClient.GetType().Name, aiProviderOptions.TranscriptionModel);
        }

        public Task<SpeechToTextResult> TranscribeAsync(Stream audioStream, string mimeType, CancellationToken ct = default)
        {
            _logger.LogDebug("Transcribing audio stream with MIME type: {MimeType}", mimeType);
            return _audioClient.TranscribeAudioAsync(audioStream, mimeType, ct);
        }
    }
}
