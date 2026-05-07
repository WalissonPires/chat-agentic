using System.ClientModel;
using ChatAgentic.Features.AI.Usage;
using OpenAI;
using OpenAI.Audio;

namespace ChatAgentic.Features.AI;

public sealed class TextToSpeechService
{
    private readonly AudioClient _audioClient;
    private readonly GeneratedSpeechVoice _voice;
    private readonly string _provider;

    public TextToSpeechService(AIProviderOptions aiProviderOptions)
    {
        var apiKey = aiProviderOptions.ApiKey ?? throw new Exception("AIProvider APIKey not defined.");
        var model = aiProviderOptions.TtsModel ?? throw new Exception("AIProvider TtsModel not defined.");
        var voice = aiProviderOptions.TtsVoice ?? throw new Exception("AIProvider TtsVoice not defined.");
        var endpoint = aiProviderOptions.Endpoint;

        _provider = AIProviderName.FromEndpoint(endpoint);

        _audioClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = string.IsNullOrEmpty(endpoint) ? null : new Uri(endpoint),
        }).GetAudioClient(model);

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
        return new SynthesizedAudio("audio/mp3", audio, _provider, text.Length, 0, 0m);
    }

    public sealed record SynthesizedAudio(
        string MimeType,
        BinaryData Audio,
        string Provider,
        long Input,
        long Output,
        decimal Cost
    ) : IAIUsageReport
    {
        public AIUsageService Service => AIUsageService.TTS;
    }
}
