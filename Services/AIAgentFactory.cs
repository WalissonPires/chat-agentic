using ChatAgentic.Workflows;
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
        private readonly AIAgentToolsFactory _toolsFactory;
        private readonly TextSearchProviderFactory _textSearchProviderFactory;

        public AIAgentFactory(IOptions<AIProviderOptions> aiProviderOptions, ILoggerFactory loggerFactory, AIAgentToolsFactory toolsFactory, TextSearchProviderFactory textSearchProviderFactory)
        {
            var apiKey = aiProviderOptions.Value.ApiKey ?? throw new Exception("AIProvider APIKey not defined.");
            var model = aiProviderOptions.Value.ChatModel ?? throw new Exception("AIProvider ChatModel not defined.");

            _aiClient = new OpenAIClient(apiKey);
            _chatModel = model;
            _loggerFactory = loggerFactory;
            _toolsFactory = toolsFactory;
            _textSearchProviderFactory = textSearchProviderFactory;
        }

        public async Task<AIAgent> CreateAsync(int workspaceId)
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
            * Prefira respostas curtas (Max. 4000 caracteres)
            * Use passo a passo quando necessário
            * Foque sempre em resolver o problema do usuário
            * Formate as mensagens para facil leitura no whatsapp
            * Não cite nas resposas o uso de tools
            * Use os seguintes cards da dashbord na tool:
                - card_categories: Valor total despesas de um mês/ano especifico
                - card_tags: Valor total despesas de um mês/ano especifico
                - card_payments: Valor total despesas de um mês/ano especifico
                - card_participants: Valor total despesas de um mês/ano especifico
                - card_future_expenses: Valor total todas as despesas futuras registradas
                - card_expenses_in_year: Valor total despesas dos ultimos 12 meses
                - card_categories_in_year: Valor total despesas dos ultimos 12 meses
                - card_tags_in_year: Valor total despesas dos ultimos 12 meses
                - card_expenses_daily_average
                - card_expenses_forecast: Previsão do Valor total das despesas futuras
            * Para saber qual o participante_id do usuário chame a tool account_me_get. Na propriedade config tem uma string JSON que é um objeto com: { "participant_id": valor }
            * Para listar despesas formate cada despesa em duas linhas assim:
                *Descrição index/installment_count* - R$ 100,00
                MetodoPag, Categoria, Tags

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
                        Tools = [.. AIAgentInternalTools.GetTools(), .. await _toolsFactory.CreateAsync()],
                    },
                    AIContextProviders =
                    [
                        _textSearchProviderFactory.Create(new (
                            WorkspaceId: workspaceId,
                            Context: "midesp",
                            ToolName: "midesp_knowledge",
                            ToolDescription: "Base de conhecimento do Midesp. Use para obter informações relacionadas ao Midesp",
                            SearchTime: TextSearchProviderOptions.TextSearchBehavior.OnDemandFunctionCalling
                        ))
                    ]
                },
                loggerFactory: _loggerFactory)
                .AsBuilder()
                .Use(AIAgentMiddleware.InjectToolArguments)
                .Build();

            logger.LogDebug("AI Agent Created '{aiAgentName}'", aiAgent.Name);

            return aiAgent;
        }
    }
}