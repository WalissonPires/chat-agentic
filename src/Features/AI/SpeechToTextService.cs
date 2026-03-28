using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;

namespace ChatAgentic.Features.AI
{
    public class SpeechToTextService
    {
        private readonly AudioClient _audioClient;

        public SpeechToTextService(IOptions<AIProviderOptions> aiProviderOptions)
        {
            var apiKey = aiProviderOptions.Value.ApiKey ?? throw new Exception("AIProvider APIKey not defined.");
            var model = aiProviderOptions.Value.TranscriptionModel ?? throw new Exception("AIProvider TranscriptionModel not defined.");
            _audioClient = new OpenAIClient(apiKey).GetAudioClient(model);
        }

        public async Task<string> TranscribeAsync(Stream audioStream, string mimeType, CancellationToken ct = default)
        {
            // Map MIME type to file extension so Whisper can detect the codec.
            string extension = mimeType.ToLowerInvariant() switch
            {
                "audio/webm" or "audio/webm;codecs=opus" => "webm",
                "audio/ogg" or "audio/ogg;codecs=opus" => "ogg",
                "audio/mp4" or "audio/m4a" => "m4a",
                "audio/mpeg" or "audio/mp3" => "mp3",
                "audio/wav" or "audio/wave" => "wav",
                "audio/flac" => "flac",
                _ => "webm"
            };

            string fileName = $"audio.{extension}";

            AudioTranscriptionOptions options = new()
            {
                Language = "pt",
                ResponseFormat = AudioTranscriptionFormat.Text,
            };

            var result = await _audioClient.TranscribeAudioAsync(audioStream, fileName, options, ct);
            return result.Value.Text.Trim();
        }
    }
}