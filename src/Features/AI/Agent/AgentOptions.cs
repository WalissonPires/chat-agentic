namespace ChatAgentic.Features.AI.Agent
{
    public class AgentOptions
    {
        public string? Instructions { get; set; }
        public bool UseStructuredOutput { get; set; } = true;
        public bool EnableTools { get; set; } = true;
        public bool EnableContextProviders { get; set; } = true;
        public bool EnableAgentMiddleware { get; set; } = true;
        public bool StrictToolNameValidation { get; set; } = true;
    }
}