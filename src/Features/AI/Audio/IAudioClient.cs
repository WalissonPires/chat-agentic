using ChatAgentic.Features.AI.Usage;

namespace ChatAgentic.Features.AI.Audio
{
    public interface IAudioClient
    {
        Task<SpeechToTextResult> TranscribeAudioAsync(Stream audioStream, string mimeType, CancellationToken cancellationToken = default);
    }
}
