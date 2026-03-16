using ChatAgentic.Data;
using ChatAgentic.Workflows;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Channels
{
    public class WebhookMessageProcessor
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
                _logger.LogDebug("Webhook token not found");
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

            await _queue.EnqueueAsync(result.Message);

            _logger.LogDebug("Message processed");
        }
    }

    public record WebhookMessageProcessorInput(
        ChannelType Channel,
        string Token,
        string JsonPayload
    );
}