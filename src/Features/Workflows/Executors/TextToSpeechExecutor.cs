using ChatAgentic.Features.AI;
using ChatAgentic.Features.AI.Usage;
using ChatAgentic.Features.Workflows;
using ChatAgentic.Utils;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Workflows.Executors
{
    public sealed partial class TextToSpeechExecutor : Executor
    {
        private readonly ILogger _logger;
        private readonly TextToSpeechService _ttsService;
        private readonly IAIUsageHistoryRepository _usageHistoryRepository;

        public TextToSpeechExecutor(ILogger<TextToSpeechService> logger, TextToSpeechService ttsService,
            IAIUsageHistoryRepository usageHistoryRepository) : base("TextToSpeech")
        {
            _logger = logger;
            _ttsService = ttsService;
            _usageHistoryRepository = usageHistoryRepository;
        }

        protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
        {
            return protocolBuilder
                .SendsMessage<WorkflowExecutionContext>()
                .ConfigureRoutes(routers =>
                {
                    routers.AddHandler<WorkflowExecutionContext>(HandleAsync);
                });
        }

        private async ValueTask HandleAsync(WorkflowExecutionContext weContext, IWorkflowContext context, CancellationToken ct)
        {
            _logger.LogDebug("Synthesizing text into audio");

            if (weContext.ReceiveidAudio)
            {
                foreach (var structuredResponse in weContext.OutputStructuredResponses)
                {
                    if (string.IsNullOrWhiteSpace(structuredResponse.SpeakableText))
                        continue;

                    var result = await _ttsService.SynthesizeAsync(structuredResponse.SpeakableText, ct);
                    var audioBase64 = Convert.ToBase64String(result.Audio);
                    var audioUri = new DataUri(result.MimeType, audioBase64).ToString();
                    var audioMessage = new ChatMessage(ChatRole.Assistant, [new UriContent(audioUri, result.MimeType)]);
                    weContext.OutputAudioMessages.Add(audioMessage);

                    await _usageHistoryRepository.AddAsync(AIUsageHistoryFactory.Create(weContext.WorkspaceId, weContext.ConversationId, result), ct);

                    _logger.LogDebug(
                        "TTS usage recorded workspace={workspaceId} conversation={conversationId} provider={provider}",
                        weContext.WorkspaceId,
                        weContext.ConversationId,
                        result.Provider);
                }
            }

            await context.SendMessageAsync(weContext, ct);
            _logger.LogDebug("Synthesis completed");
        }
    }
}