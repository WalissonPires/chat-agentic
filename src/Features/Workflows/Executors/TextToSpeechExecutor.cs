using ChatAgentic.Features.AI;
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

        public TextToSpeechExecutor(ILogger<TextToSpeechService> logger, TextToSpeechService ttsService) : base("TextToSpeech")
        {
            _logger = logger;
            _ttsService = ttsService;
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

            foreach (var message in weContext.OutputMessages.ToArray())
            {
                var textContents = message.Contents.OfType<TextContent>().ToArray();
                var audioMessage = new ChatMessage(message.Role, []);

                foreach (var content in textContents)
                {
                    var result = await _ttsService.SynthesizeAsync(content.Text, ct);

                    // var filename = Path.GetTempFileName();
                    // using var file = File.OpenWrite(filename);
                    // await result.Audio.ToStream().CopyToAsync(file, ct);
                    //audioMessage.Contents.Add(new UriContent("file://" + filename, result.MimeType));

                    var audioBase64 = Convert.ToBase64String(result.Audio);
                    var audioUri = new DataUri(result.MimeType, audioBase64).ToString();
                    audioMessage.Contents.Add(new UriContent(audioUri, result.MimeType));
                }

                if (audioMessage.Contents.Count > 0)
                    weContext.OutputAudioMessages.Add(audioMessage);

            }

            await context.SendMessageAsync(weContext, ct);
            _logger.LogDebug("Synthesis completed");
        }
    }
}