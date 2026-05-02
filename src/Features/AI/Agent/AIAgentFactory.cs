using System.ClientModel;
using ChatAgentic.Entities;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.Text.RegularExpressions;

namespace ChatAgentic.Features.AI.Agent
{
    public partial class AIAgentFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly OpenAIClient _aiClient;
        private readonly string _chatModel;
        private readonly AIAgentToolsFactory _toolsFactory;
        private readonly AIAgentSkillsFactory _skillsFactory;
        private readonly TextSearchProviderFactory _textSearchProviderFactory;

        public AIAgentFactory(AIProviderOptions aiProviderOptions, ILoggerFactory loggerFactory, AIAgentToolsFactory toolsFactory,
            AIAgentSkillsFactory skillsFactory, TextSearchProviderFactory textSearchProviderFactory)
        {
            var apiKey = aiProviderOptions.ApiKey ?? throw new Exception("AIProvider APIKey not defined.");
            var model = aiProviderOptions.ChatModel ?? throw new Exception("AIProvider ChatModel not defined.");
            var endpoint = aiProviderOptions.Endpoint;

            _aiClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
            {
                Endpoint = string.IsNullOrEmpty(endpoint) ? null : new Uri(endpoint),
            });

            _chatModel = model;
            _loggerFactory = loggerFactory;
            _toolsFactory = toolsFactory;
            _skillsFactory = skillsFactory;
            _textSearchProviderFactory = textSearchProviderFactory;
        }

        public async Task<AIAgent> CreateAsync(int workspaceId, WorkflowAgentOptions workflowAgentOptions)
        {
            var instructions = workflowAgentOptions.Instructions;
            if (string.IsNullOrWhiteSpace(instructions))
                throw new InvalidOperationException("Workflow agent instructions are not configured.");

            var logger = _loggerFactory.CreateLogger<AIAgentFactory>();

            logger.LogDebug("Creating AI Agent");

            var tools = await CreateToolsAsync(workflowAgentOptions, logger);
            var contextProviders = CreateContextProviders(workflowAgentOptions, workspaceId, logger);

            var chatOptions = new ChatOptions
            {
                Instructions = instructions,
                MaxOutputTokens = 20_000,
            };

            if (tools.Count > 0)
                chatOptions.Tools = tools;

            var chatAgentOptions = new ChatClientAgentOptions
            {
                Name = "AI Assistent",
                ChatOptions = chatOptions,
            };

            if (contextProviders.Count > 0)
                chatAgentOptions.AIContextProviders = contextProviders;

            var builder = _aiClient
                .GetChatClient(_chatModel)
                .AsAIAgent(chatAgentOptions, loggerFactory: _loggerFactory)
                .AsBuilder();

            if (workflowAgentOptions.EnableAgentMiddleware)
                builder.Use(AIAgentMiddleware.InjectToolArguments);

            var aiAgent = builder.Build();

            logger.LogDebug("AI Agent Created '{aiAgentName}'", aiAgent.Name);

            return aiAgent;
        }

        private async Task<List<AITool>> CreateToolsAsync(WorkflowAgentOptions agentOptions, ILogger logger)
        {
            if (!agentOptions.EnableTools)
            {
                logger.LogInformation("Tools are disabled by WorkflowAgentOptions.EnableTools");
                return [];
            }

            var tools = new List<AITool>();
            tools.AddRange(AIAgentInternalTools.GetTools());
            tools.AddRange(await _toolsFactory.CreateAsync());

            var normalizedTools = NormalizeAndDeduplicateTools(agentOptions, tools, logger);

            logger.LogInformation("Agent tools configured. source={sourceCount} normalized={normalizedCount}", tools.Count, normalizedTools.Count);
            return normalizedTools;
        }

        private List<AIContextProvider> CreateContextProviders(WorkflowAgentOptions agentOptions, int workspaceId, ILogger logger)
        {
            if (!agentOptions.EnableContextProviders)
            {
                logger.LogInformation("Context providers are disabled by WorkflowAgentOptions.EnableContextProviders");
                return [];
            }

            if (!agentOptions.EnableTools)
            {
                logger.LogWarning("Context providers skipped because tools are disabled.");
                return [];
            }

            var providers = new List<AIContextProvider>
            {
                _skillsFactory.Create(),
                _textSearchProviderFactory.Create(new(
                    WorkspaceId: workspaceId,
                    Context: "midesp",
                    ToolName: "midesp_knowledge",
                    ToolDescription: "Base de conhecimento do Midesp. Use para obter informacoes relacionadas ao Midesp",
                    SearchTime: TextSearchProviderOptions.TextSearchBehavior.OnDemandFunctionCalling
                ))
            };

            logger.LogInformation("Agent context providers configured: {providerCount}", providers.Count);
            return providers;
        }

        private List<AITool> NormalizeAndDeduplicateTools(WorkflowAgentOptions agentOptions, IEnumerable<AITool> tools, ILogger logger)
        {
            var output = new List<AITool>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var tool in tools)
            {
                var toolName = tool.Name;
                if (string.IsNullOrWhiteSpace(toolName))
                {
                    logger.LogWarning("Skipping tool with empty name.");
                    continue;
                }

                var candidateName = agentOptions.StrictToolNameValidation ? NormalizeToolName(toolName) : toolName;
                if (string.IsNullOrWhiteSpace(candidateName))
                {
                    logger.LogWarning("Skipping tool '{toolName}' due to invalid normalized name.", toolName);
                    continue;
                }

                var uniqueName = CreateUniqueName(candidateName, seenNames);
                if (!string.Equals(uniqueName, toolName, StringComparison.Ordinal))
                {
                    logger.LogWarning("Skipping tool '{toolName}' because provider-safe normalized name would be '{normalizedName}'. Rename the tool to use [a-zA-Z0-9_-] with max 64 chars.", toolName, uniqueName);
                    continue;
                }

                output.Add(tool);
            }

            return output;
        }

        private static string NormalizeToolName(string toolName)
        {
            var normalized = InvalidToolNameCharsRegex().Replace(toolName, "_");
            normalized = normalized.Trim('_');
            if (normalized.Length == 0)
                return string.Empty;

            if (normalized.Length > 64)
                normalized = normalized[..64];

            return normalized;
        }

        private static string CreateUniqueName(string candidateName, HashSet<string> seenNames)
        {
            if (seenNames.Add(candidateName))
                return candidateName;

            for (var i = 2; i < 1000; i++)
            {
                var suffix = "_" + i;
                var maxBaseLength = Math.Max(1, 64 - suffix.Length);
                var baseName = candidateName.Length > maxBaseLength ? candidateName[..maxBaseLength] : candidateName;
                var uniqueName = baseName + suffix;
                if (seenNames.Add(uniqueName))
                    return uniqueName;
            }

            var fallback = candidateName[..Math.Min(63, candidateName.Length)] + "_x";
            seenNames.Add(fallback);
            return fallback;
        }

        [GeneratedRegex(@"[^a-zA-Z0-9_-]", RegexOptions.Compiled)]
        private static partial Regex InvalidToolNameCharsRegex();
    }
}