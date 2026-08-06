using System.Globalization;
using System.Text.RegularExpressions;
using ChatAgentic.Features.Channels.Whatsapp;
using ChatAgentic.Persistence;
using ChatAgentic.Queue;

namespace ChatAgentic.Features.Channels
{
    public partial class WebhookMessageProcessor
    {
        private readonly ILogger _logger;
        private readonly ChannelMessageTransformFactory _processorFactory;
        private readonly IMessageQueue<Message> _queue;
        private readonly AgentDefinitionLoader _agentDefinitionLoader;
        private readonly WorkspaceLoader _workspaceLoader;

        public WebhookMessageProcessor(ILogger<WhatsappMessageTransform> logger, ChannelMessageTransformFactory processorFactory, IMessageQueue<Message> queue, AgentDefinitionLoader agentDefinitionLoader, WorkspaceLoader workspaceLoader)
        {
            _logger = logger;
            _processorFactory = processorFactory;
            _queue = queue;
            _agentDefinitionLoader = agentDefinitionLoader;
            _workspaceLoader = workspaceLoader;
        }

        public async Task Execute(WebhookMessageProcessorInput input)
        {
            _logger.LogDebug("Recevied webhook message");
            _logger.LogDebug(input.JsonPayload);

            if (string.IsNullOrEmpty(input.Token))
            {
                _logger.LogDebug("Webhook token is empty");
                return;
            }

            var agentDefinition = await _agentDefinitionLoader.LoadFromWebhookTokenAsync(input.Token);
            if (agentDefinition == null)
            {
                _logger.LogError("Webhook token not found");
                return;
            }

            await _workspaceLoader.LoadFromWorkspaceIdAsync(agentDefinition.WorkspaceId);

            _logger.LogDebug("Create channel message processor");
            var processor = _processorFactory.Create(input.Channel);

            _logger.LogDebug("Process message");
            var result = await processor.Execute(new(agentDefinition.WorkspaceId, agentDefinition.Id, input.JsonPayload));

            if (result.Skip || result.SelfMessage || result.Message == null)
            {
                _logger.LogDebug("Skip message processing");
                return;
            }

            var message = result.Message;
            if (!string.IsNullOrEmpty(message.ContentText))
            {
                message = message with
                {
                    ContentText = TextSanatization(message.ContentText)
                };
            }

            await _queue.EnqueueAsync(message);

            _logger.LogDebug("Message processed");
        }

        public string TextSanatization(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            // Remove invisible control characters
            var textSanitized = TextSanatizationRegex().Replace(input, "");

            // Counting Grapheme Clusters
            var info = new StringInfo(textSanitized);
            if (info.LengthInTextElements > 200)
                throw new Exception("The message exceeds the 200-character limit.");

            return textSanitized;
        }


        // \p{Cf} remove Format characters (LRM, RLM, ZWJ invisíveis isolados)
        // \p{Cc} remove Control characters (como caracteres de sistema)
        [GeneratedRegex(@"[\p{Cf}\p{Cc}]")]
        private static partial Regex TextSanatizationRegex();
    }

    public record WebhookMessageProcessorInput(
        ChannelType Channel,
        string Token,
        string JsonPayload
    );
}