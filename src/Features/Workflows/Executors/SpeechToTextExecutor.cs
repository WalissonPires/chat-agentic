using ChatAgentic.Features.AI;
using ChatAgentic.Features.AI.Usage;
using ChatAgentic.Features.Workflows;
using ChatAgentic.Utils;
using Microsoft.Agents.AI.Workflows;

namespace ChatAgentic.Features.Workflows.Executors
{
    public sealed partial class SpeechToTextExecutor : Executor
    {
        private readonly ILogger _logger;
        private readonly SpeechToTextService _sttService;
        private readonly MessageMediaStream _mediaStream;
        private readonly IAIUsageHistoryRepository _usageHistoryRepository;

        public SpeechToTextExecutor(ILogger<SpeechToTextExecutor> logger, SpeechToTextService sttService,
            MessageMediaStream mediaStream, IAIUsageHistoryRepository usageHistoryRepository) : base("SpeechToText")
        {
            _logger = logger;
            _sttService = sttService;
            _mediaStream = mediaStream;
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

        public async ValueTask HandleAsync(WorkflowExecutionContext weContext, IWorkflowContext context, CancellationToken ct)
        {
            _logger.LogInformation("Transcribing audio for {channel}: {identifier}", weContext.Channel, weContext.SenderIdentifier);

            var audioMessages = weContext.InputMessages.Where(x => x.ContentType == Channels.MessageContentType.Audio).ToList();
            if (audioMessages.Count == 0)
            {
                _logger.LogWarning("Audio messages not found");
                await context.SendMessageAsync(weContext);
                return;
            }

            for (int i = 0; i < audioMessages.Count; i++)
            {
                var audioMessage = audioMessages[i];

                _logger.LogDebug("Transcript audio message {index}/{count}", i + 1, audioMessages.Count);

                if (string.IsNullOrEmpty(audioMessage.MediaUri))
                {
                    _logger.LogWarning("Audio message uri is empty");
                    continue;
                }

                if (string.IsNullOrEmpty(audioMessage.MimeType))
                    _logger.LogWarning("Audio message MIME Type is empty");

                using var mediaStream = await _mediaStream.GetMediaStream(audioMessage.MediaUri);
                var transcriptResult = await _sttService.TranscribeAsync(mediaStream, audioMessage.MimeType ?? "audio/*", ct);

                _logger.LogDebug("Audio transcribed: {text}", transcriptResult.Text);

                await _usageHistoryRepository.AddAsync(AIUsageHistoryFactory.Create(weContext.WorkspaceId, weContext.ConversationId, transcriptResult), ct);

                _logger.LogDebug(
                    "STT usage recorded workspace={workspaceId} conversation={conversationId} provider={provider}",
                    weContext.WorkspaceId,
                    weContext.ConversationId,
                    transcriptResult.Provider);

                var transcriptMessage = audioMessage with
                {
                    ContentType = Channels.MessageContentType.Text,
                    ContentText = transcriptResult.Text,
                    MediaUri = null,
                    MimeType = null,
                    FileName = null
                };

                var messageIndex = weContext.InputMessages.FindIndex(msg => msg == audioMessage);
                weContext.InputMessages.Insert(messageIndex, transcriptMessage);
                weContext.InputMessages.Remove(audioMessage);

                await context.SendMessageAsync(weContext);
            }
        }
    }
}