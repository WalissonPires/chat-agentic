using System.Globalization;
using System.Text.RegularExpressions;
using ChatAgentic.Features.Channels.Whatsapp;
using ChatAgentic.Features.Workflows;
using ChatAgentic.Persistence;
using ChatAgentic.Queue;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Features.Channels
{
    public partial class WebhookMessageProcessor
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;
        private readonly ChannelMessageTransformFactory _processorFactory;
        private readonly IMessageQueue<Message> _queue;

        public WebhookMessageProcessor(ILogger<WhatsappMessageTransform> logger, AppDbContext dbContext, ChannelMessageTransformFactory processorFactory, IMessageQueue<Message> queue)
        {
            _logger = logger;
            _dbContext = dbContext;
            _processorFactory = processorFactory;
            _queue = queue;
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

            var workspaceId = await _dbContext.Workspaces.Where(x => x.WebhookToken == input.Token).Select(x => x.Id).FirstOrDefaultAsync();
            if (workspaceId == default)
            {
                _logger.LogError("Webhook token not found");
                return;
            }

            _logger.LogDebug("Create channel message processor");
            var processor = _processorFactory.Create(input.Channel);

            _logger.LogDebug("Process message");
            var result = await processor.Execute(new(workspaceId, input.JsonPayload));

            if (result.SelfMessage)
            {
                _logger.LogDebug("Skip self message");
                return;
            }

            if (!string.IsNullOrEmpty(result.Message.ContentText))
            {
                result = result with
                {
                    Message = result.Message with
                    {
                        ContentText = TextSanatization(result.Message.ContentText)
                    }
                };
            }

            await _queue.EnqueueAsync(result.Message);

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