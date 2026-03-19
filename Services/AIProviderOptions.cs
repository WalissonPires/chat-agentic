namespace ChatAgentic.Services
{
    public class AIProviderOptions
    {
        public string? ApiKey { get; set; }
        public string? TranscriptionModel { get; set; }
        public string? ChatModel { get; set; }
        public string? TtsModel { get; set; }
        public string? TtsVoice { get; set; }
    }
}