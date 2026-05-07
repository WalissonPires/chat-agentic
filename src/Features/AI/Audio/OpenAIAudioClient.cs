using System.ClientModel;
using ChatAgentic.Features.AI.Usage;
using OpenAI;
using OpenAI.Audio;

namespace ChatAgentic.Features.AI.Audio
{
    public sealed class OpenAIAudioClient : IAudioClient
    {
        private readonly AudioClient _audioClient;
        private readonly string _provider;

        public OpenAIAudioClient(AIProviderOptions options)
        {
            var apiKey = options.ApiKey ?? throw new InvalidOperationException("AIProvider APIKey not defined.");
            var model = options.TranscriptionModel ?? throw new InvalidOperationException("AIProvider TranscriptionModel not defined.");
            var endpoint = options.Endpoint;

            _provider = AIProviderName.FromEndpoint(endpoint);

            _audioClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
            {
                Endpoint = string.IsNullOrEmpty(endpoint) ? null : new Uri(endpoint),
            }).GetAudioClient(model);
        }

        public async Task<SpeechToTextResult> TranscribeAudioAsync(Stream audioStream, string mimeType, CancellationToken cancellationToken = default)
        {
            var extension = AudioTranscriptionFormats.MapMimeToFormat(mimeType);

            AudioTranscriptionOptions options = new()
            {
                Language = "pt",
                ResponseFormat = AudioTranscriptionFormat.Text,
            };

            var result = await _audioClient.TranscribeAudioAsync(audioStream, $"audio.{extension}", options, cancellationToken).ConfigureAwait(false);
            var text = result.Value.Text.Trim();

#pragma warning disable OPENAI001 // AudioTranscription.Usage is experimental in OpenAI .NET SDK
            object? usageObj = result.Value.Usage;
#pragma warning restore OPENAI001
            var (input, output) = AIUsageTokenMapper.FromOpenAiSdkUsageObject(usageObj);

            return new SpeechToTextResult
            {
                Text = text,
                Provider = _provider,
                Input = input,
                Output = output,
                Cost = 0m
            };
        }
    }
}
