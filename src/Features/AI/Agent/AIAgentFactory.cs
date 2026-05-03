using System.ClientModel;
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

        public async Task<AIAgent> CreateAsync(int workspaceId, AgentOptions options)
        {
            var instructions = BuildAgentInstructions(options);
            if (string.IsNullOrWhiteSpace(instructions))
                throw new InvalidOperationException("Agent definition instructions are not configured.");

            var logger = _loggerFactory.CreateLogger<AIAgentFactory>();

            logger.LogDebug("Creating AI Agent");

            var tools = await CreateToolsAsync(options, logger);
            var contextProviders = CreateContextProviders(options, workspaceId, logger);

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

            if (options.EnableAgentMiddleware)
                builder.Use(AIAgentMiddleware.InjectToolArguments);

            var aiAgent = builder.Build();

            logger.LogDebug("AI Agent Created '{aiAgentName}'", aiAgent.Name);

            return aiAgent;
        }

        private static string BuildAgentInstructions(AgentOptions options)
        {
            var instructions = options.Instructions?.Trim();
            if (string.IsNullOrWhiteSpace(instructions))
                return string.Empty;

            if (!options.UseStructuredOutput)
                return instructions;

            return $"{instructions}\n\n{StructuredOutputInstructions}";
        }

        private const string StructuredOutputInstructions = """
Sempre responda no formato estruturado configurado pela aplicacao.
Regras obrigatorias:
- speakableText: TODA a resposta em linguagem natural (perguntas, listas faladas, passo a passo, observacoes, ofertas de ajuda). Sem URLs. Nada de "veja no link" sem dizer o que fazer em palavras; o usuario pode ouvir isso em voz alta.
- textSegments: SOMENTE strings que sao URLs literais (comecam com http:// ou https://). Um item = um link. Nada de frases, nada de numeracao, nada de pergunta, nada de "Se for Android...". Se nao existir URL na resposta, use exatamente [].
- Proibido colocar URL em speakableText. Proibido usar textSegments como "mensagem extra" ou complemento: qualquer texto que nao seja URL vai em speakableText.

Exemplo ERRADO (nao faca):
{"speakableText":"Baixe o app pelo link...","textSegments":["Voce usa Android ou iPhone?","1) Abra as configuracoes 2) ..."]}

Exemplo CERTO quando ha download em https://exemplo.com/app.apk:
{"speakableText":"Para Android: baixe o aplicativo, permita origem desconhecida e instale. Quer que eu detalhe para celular ou tablet?","textSegments":["https://exemplo.com/app.apk"]}

Exemplo CERTO quando NAO ha URL:
{"speakableText":"Toda a sua resposta aqui, completa.","textSegments":[]}
O audio de resposta e decidido pela aplicacao quando o usuario enviou mensagem em audio; nao inclua campos extras para isso.
""";

        private async Task<List<AITool>> CreateToolsAsync(AgentOptions agentOptions, ILogger logger)
        {
            if (!agentOptions.EnableTools)
            {
                logger.LogInformation("Tools are disabled by AgentDefinitionAgentOptions.EnableTools");
                return [];
            }

            var tools = new List<AITool>();
            tools.AddRange(AIAgentInternalTools.GetTools());
            tools.AddRange(await _toolsFactory.CreateAsync());

            var normalizedTools = NormalizeAndDeduplicateTools(agentOptions, tools, logger);

            logger.LogInformation("Agent tools configured. source={sourceCount} normalized={normalizedCount}", tools.Count, normalizedTools.Count);
            return normalizedTools;
        }

        private List<AIContextProvider> CreateContextProviders(AgentOptions agentOptions, int workspaceId, ILogger logger)
        {
            if (!agentOptions.EnableContextProviders)
            {
                logger.LogInformation("Context providers are disabled by AgentDefinitionAgentOptions.EnableContextProviders");
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

        private List<AITool> NormalizeAndDeduplicateTools(AgentOptions agentOptions, IEnumerable<AITool> tools, ILogger logger)
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