using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Audio;

namespace ChatAgentic.Services;

public sealed class TextToSpeechService
{
    private readonly AudioClient _audioClient;
    private readonly GeneratedSpeechVoice _voice;

    public TextToSpeechService(IOptions<AIProviderOptions> aiProviderOptions)
    {
        var apiKey = aiProviderOptions.Value.ApiKey ?? throw new Exception("AIProvider APIKey not defined.");
        var model = aiProviderOptions.Value.TtsModel ?? throw new Exception("AIProvider TtsModel not defined.");
        var voice = aiProviderOptions.Value.TtsVoice ?? throw new Exception("AIProvider TtsVoice not defined.");

        _audioClient = new OpenAIClient(apiKey).GetAudioClient(model);

        _voice = voice.ToLowerInvariant() switch
        {
            "alloy" => GeneratedSpeechVoice.Alloy,
            "echo" => GeneratedSpeechVoice.Echo,
            "fable" => GeneratedSpeechVoice.Fable,
            "onyx" => GeneratedSpeechVoice.Onyx,
            "nova" => GeneratedSpeechVoice.Nova,
            "shimmer" => GeneratedSpeechVoice.Shimmer,
            _ => GeneratedSpeechVoice.Nova
        };
    }

    public async Task<SynthesizedAudio> SynthesizeAsync(string text, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Text is empty");

        var options = new SpeechGenerationOptions()
        {
            ResponseFormat = GeneratedSpeechFormat.Mp3,
            SpeedRatio = 1.0f,
        };

        var audio = await _audioClient.GenerateSpeechAsync(text, _voice, options, ct);
        return new SynthesizedAudio("audio/mp3", audio);
    }

    public record SynthesizedAudio(
        string MimeType,
        BinaryData Audio
    );
}
