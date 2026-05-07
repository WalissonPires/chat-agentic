namespace ChatAgentic.Features.AI.Usage;

public static class AIProviderName
{
    public static string FromEndpoint(string? endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
            return "openai";

        return endpoint.Contains("openrouter", StringComparison.OrdinalIgnoreCase)
            ? "openrouter"
            : "custom";
    }
}
