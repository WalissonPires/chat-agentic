using ChatAgentic.Features.AI;
using ChatAgentic.Features.AI.Agent;
using ChatAgentic.Features.AI.Usage;
using ChatAgentic.Features.Channels;
using ChatAgentic.src.Features.AI;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using System.ClientModel;
using System.Text.Json;

namespace ChatAgentic.Features.Workflows.Executors
{
    public sealed partial class AIAgentExecutor : Executor
    {
        private readonly ILogger _logger;
        private readonly AIAgentFactory _aiAgentFactory;
        private readonly AIProviderOptions _aiProviderOptions;
        private readonly IAIUsageHistoryRepository _usageHistoryRepository;

        public AIAgentExecutor(ILogger<AIAgentExecutor> logger, AIAgentFactory aiAgentFactory,
            AIProviderOptions aiProviderOptions, IAIUsageHistoryRepository usageHistoryRepository) : base("Agent")
        {
            _logger = logger;
            _aiAgentFactory = aiAgentFactory;
            _aiProviderOptions = aiProviderOptions;
            _usageHistoryRepository = usageHistoryRepository;
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
            var aiAgent = await _aiAgentFactory.CreateAsync(weContexto.WorkspaceId, weContexto.AgentOptions);

            ChatMessage[] messages = [ ..weContexto.LastMessages, ..weContexto.InputMessages.Select(x =>
            {
                var msg = x.ToChatMessage();
                foreach(var c in msg.Contents)
                    (c as UriContent)?.LoadFileToBase64();
                return msg;
            }) ];

            _logger.LogDebug("Sending {messageCount} messages to AIAgent", messages.Length);

            weContexto.InputMessages.ForEach(msg => _logger.LogDebug("User message [{contentType}]:\r\n{contentText}", msg.ContentType, msg.ContentType == MessageContentType.Text ? msg.ContentText : msg.MediaUri));

            var runOptions = new AgentRunOptions
            {
                AdditionalProperties = new (weContexto.ContactMetadata.Select(x => new KeyValuePair<string, object?>(x.Name, x.Value)))
            };

            if (weContexto.AgentOptions.UseStructuredOutput)
                runOptions.ResponseFormat = ChatResponseFormat.ForJsonSchema<AgentStructuredResponse>();

            try
            {
                var response = await aiAgent.RunAsync(messages, null, runOptions, ct);
                var structuredResponse = ParseStructuredResponse(response);
                weContexto.OutputStructuredResponses.Add(structuredResponse);

                var (input, output) = AIUsageTokenMapper.FromUsageDetails(response.Usage);
                var provider = AIProviderName.FromEndpoint(_aiProviderOptions.Endpoint);
                var chatReport = new ChatUsageReport
                {
                    Provider = provider,
                    Input = input,
                    Output = output,
                    Cost = 0m
                };
                await _usageHistoryRepository.AddAsync(
                    AIUsageHistoryFactory.Create(weContexto.WorkspaceId, weContexto.ConversationId, chatReport),
                    ct);

                _logger.LogDebug(
                    "AI usage recorded workspace={workspaceId} conversation={conversationId} provider={provider} input={inputTokens} output={outputTokens}",
                    weContexto.WorkspaceId,
                    weContexto.ConversationId,
                    provider,
                    input,
                    output);
            }
            catch (ClientResultException ex)
            {
                var errorMetadata = ex.Data.Count == 0
                    ? string.Empty
                    : string.Join("; ", ex.Data.Cast<System.Collections.DictionaryEntry>().Select(x => $"{x.Key}={x.Value}"));

                _logger.LogError(ex, "AI provider request failed. Status={status}. Message={message}. Metadata={metadata}",
                    ex.Status,
                    ex.Message,
                    errorMetadata);
                throw;
            }

            _logger.LogDebug("AIAgent reply {messageCount} messages", weContexto.OutputStructuredResponses.Count);

            foreach (var r in weContexto.OutputStructuredResponses)
                _logger.LogDebug(
                    "Assistant speakable:\r\n{speakable}\r\nAdditional segments: {segments}",
                    r.SpeakableText,
                    string.Join(", ", r.TextSegments.Select(static segment => $"{segment.Type}: {segment.Label} = {segment.Value}")));

            await context.SendMessageAsync(weContexto, ct);
        }

        private AgentStructuredResponse ParseStructuredResponse(AgentResponse response)
        {
            var payloadCandidates = new List<string>();

            if (!string.IsNullOrWhiteSpace(response.Text))
                payloadCandidates.Add(response.Text);

            var textContents = response.Messages
                .SelectMany(static message => message.Contents)
                .OfType<TextContent>()
                .Select(static content => content.Text)
                .Where(static text => !string.IsNullOrWhiteSpace(text));

            payloadCandidates.AddRange(textContents!);

            foreach (var candidate in payloadCandidates)
            {
                try
                {
                    var typedResponse = JsonSerializer.Deserialize<AgentStructuredResponse>(candidate);
                    if (typedResponse is not null)
                        return typedResponse.Normalize();
                }
                catch (JsonException)
                {
                    // fallback below
                }
            }

            var plainText = payloadCandidates.FirstOrDefault() ?? string.Empty;
            _logger.LogWarning("Agent response did not match structured schema. Applying plain-text fallback.");
            return AgentStructuredResponse.FromPlainText(plainText);
        }
    }
}