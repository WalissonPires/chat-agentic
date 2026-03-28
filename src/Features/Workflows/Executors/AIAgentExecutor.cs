using ChatAgentic.Features.AI.Agent;
using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Workflows;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Workflows.Executors
{
    public sealed partial class AIAgentExecutor : Executor
    {
        private readonly ILogger _logger;
        private readonly AIAgentFactory _aiAgentFactory;

        public AIAgentExecutor(ILogger<AIAgentExecutor> logger, AIAgentFactory aiAgentFactory) : base("Agent")
        {
            _logger = logger;
            _aiAgentFactory = aiAgentFactory;
        }

        protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
        {
            return protocolBuilder
                .SendsMessage<WorkflowExecutionContext>()
                .ConfigureRoutes(routes =>
                {
                    routes.AddHandler<WorkflowExecutionContext>(HandleAsync);
                });
        }

        private async ValueTask HandleAsync(WorkflowExecutionContext weContexto, IWorkflowContext context, CancellationToken ct)
        {
            var aiAgent = await _aiAgentFactory.CreateAsync(weContexto.WorkspaceId);

            ChatMessage[] messages = [ ..weContexto.LastMessages, ..weContexto.InputMessages.Select(x => x.ToChatMessage()) ];

            _logger.LogDebug("Sending {messageCount} messages to AIAgent", messages.Length);

            weContexto.InputMessages.ForEach(msg => _logger.LogDebug("User message [{contentType}]:\r\n{contentText}", msg.ContentType, msg.ContentType == MessageContentType.Text ? msg.ContentText : msg.MediaUri));

            var runOptions = new AgentRunOptions
            {
                AdditionalProperties = new (weContexto.ContactMetadata.Select(x => new KeyValuePair<string, object?>(x.Name, x.Value)))
            };

            var response = await aiAgent.RunAsync(messages, null, runOptions, ct);

            foreach (var msg in response.Messages)
                weContexto.OutputMessages.Add(new ChatMessage(ChatRole.Assistant, msg.Contents));

            _logger.LogDebug("AIAgent reply {messageCount} messages", weContexto.OutputMessages.Count);

            weContexto.OutputMessages.ForEach(msg => _logger.LogDebug("Assistent message:\r\n{contentText}", msg.Text));

            await context.SendMessageAsync(weContexto, ct);
        }
    }
}