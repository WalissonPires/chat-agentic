using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Workflows;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Workflows.Executors
{
    public sealed partial class ReplyMessgeExecutor : Executor
    {
        private readonly ILogger _logger;
        private readonly ChannelSendMessageFactory _sendMesageFactory;

        public ReplyMessgeExecutor(ILogger<ReplyMessgeExecutor> logger, ChannelSendMessageFactory sendMesageFactory) : base("ReplyMessage")
        {
            _logger = logger;
            _sendMesageFactory = sendMesageFactory;
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

        private async ValueTask HandleAsync(WorkflowExecutionContext weContext, IWorkflowContext context, CancellationToken ct)
        {
            var sendMesage = _sendMesageFactory.Create(weContext.Channel);
            var speakableAlreadySentAsAudio = weContext.OutputAudioMessages.Count > 0;

            if (weContext.OutputAudioMessages.Count > 0)
            {
                foreach (var message in weContext.OutputAudioMessages)
                {
                    await sendMesage.ExecuteAsync(new(weContext.SenderIdentifier, weContext.ChatId, message), ct);
                }
            }

            var outboundCount = 0;
            foreach (var structured in weContext.OutputStructuredResponses)
            {
                foreach (var message in structured.ToOutboundChatMessages(speakableAlreadySentAsAudio))
                {
                    outboundCount++;
                    await sendMesage.ExecuteAsync(new(weContext.SenderIdentifier, weContext.ChatId, message), ct);
                }
            }

            _logger.LogDebug("Send {audioCount} audio and {textCount} text reply messages", weContext.OutputAudioMessages.Count, outboundCount);

            await context.SendMessageAsync(weContext);

            _logger.LogDebug("Sending reply messages is complete");
        }
    }
}