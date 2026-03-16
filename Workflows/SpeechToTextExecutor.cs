using ChatAgentic.Services;
using Microsoft.Agents.AI.Workflows;

namespace ChatAgentic.Workflows
{
    public sealed partial class SpeechToTextExecutor : Executor
    {
        private readonly ILogger _logger;
        private readonly SpeechToTextService _sttService;
        private readonly MessageMediaStream _mediaStream;

        public SpeechToTextExecutor(ILogger<SpeechToTextExecutor> logger, SpeechToTextService sttService, MessageMediaStream mediaStream) : base("SpeechToText")
        {
            _logger = logger;
            _sttService = sttService;
            _mediaStream = mediaStream;
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
                var transcriptText = await _sttService.TranscribeAsync(mediaStream, audioMessage.MimeType ?? "audio/*", ct);

                _logger.LogDebug("Audio transcribed: {text}", transcriptText);

                var transcriptMessage = audioMessage with
                {
                    ContentType = Channels.MessageContentType.Text,
                    ContentText = transcriptText,
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