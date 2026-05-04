namespace ChatAgentic.Features.AI.Audio
{
    public interface IAudioClient
    {
        Task<string> TranscribeAudioAsync(Stream audioStream, string mimeType, CancellationToken cancellationToken = default);
    }
}
