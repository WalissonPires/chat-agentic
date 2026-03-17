using Microsoft.Agents.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;

namespace ChatAgentic.Services
{
    public class AIAgentFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly OpenAIClient _aiClient;
        private readonly string _chatModel;

        public AIAgentFactory(IOptions<AIProviderOptions> aiProviderOptions, ILoggerFactory loggerFactory)
        {
            var apiKey = aiProviderOptions.Value.ApiKey ?? throw new Exception("AIProvider APIKey not defined.");
            var model = aiProviderOptions.Value.ChatModel ?? throw new Exception("AIProvider ChatModel not defined.");

            _aiClient = new OpenAIClient(apiKey);
            _chatModel = model;
            _loggerFactory = loggerFactory;
        }

        public AIAgent Create()
        {
            var logger = _loggerFactory.CreateLogger<AIAgentFactory>();

            logger.LogDebug("Creating AI Agent");

            var instructions =
            """
            Você é a Erza, uma assistente do Midesp. Midesp é uma plataforma que ajuda usuários a gerenciar suas despesas.

            Seu papel é ajudar o usuário a entender, usar e resolver tarefas no Midesp.

            Você pode:

            * responder dúvidas sobre a plataforma
            * explicar funcionalidades
            * orientar passo a passo
            * ajudar a resolver problemas
            * executar ações dentro da aplicação usando tools MCP

            Diretrizes:

            * Seja claro, direto e útil
            * Prefira respostas curtas
            * Use passo a passo quando necessário
            * Foque sempre em resolver o problema do usuário
            * Formate as mensagens para facil leitura no whatsapp
            * Não cite nas resposas o uso de tools

            Uso de tools:

            * Quando uma solicitação exigir uma ação na aplicação, use a tool apropriada
            * Se faltar informação para executar a ação, peça os dados necessários
            * Nunca invente resultados de tools
            * Para executar ações e ler dados do midesp use a tool: "midesp_mcp" (Ela está conectada diretamente a conta do usuário no midesp).
            * Para obter qualquer informação sobre o midesp (como guias, FAQ e etc) use a tool: "midesp_knownledge. Utilize essas informações como fonte principal ao responder perguntas sobre o Midesp.
            * IMPORTANTE: Para o parâmetro 'api_token' descritor da tool "midesp_mcp", passe a palavra 'auto'. O sistema cuidará de injetá-lo nativamente. NUNCA pergunte ao usuário por seu token de API.
            """;

            var aiAgent = _aiClient
                .GetChatClient(_chatModel)
                .AsAIAgent(new ChatClientAgentOptions
                {
                    Name = "Erza AI Assistent",
                    ChatOptions = new Microsoft.Extensions.AI.ChatOptions
                    {
                        Instructions = instructions,
                        MaxOutputTokens = 20_000,
                        Tools = AIAgentInternalTools.GetTools(),
                    }
                },
                loggerFactory: _loggerFactory);

            logger.LogDebug("AI Agent Created '{aiAgentName}'", aiAgent.Name);

            return aiAgent;
        }
    }
}