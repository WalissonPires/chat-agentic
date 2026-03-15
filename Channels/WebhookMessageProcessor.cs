using ChatAgentic.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Channels
{
    public class WebhookMessageProcessor
    {
        private readonly ILogger _logger;
        private readonly AppDbContext _dbContext;
        private readonly ChannelMessageTransformFactory _processorFactory;

        public WebhookMessageProcessor(ILogger<WhatsappMessageTransform> logger, AppDbContext dbContext, ChannelMessageTransformFactory processorFactory)
        {
            _logger = logger;
            _dbContext = dbContext;
            _processorFactory = processorFactory;
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

            var isValidToken = await _dbContext.Workspaces.Where(x => x.WebhookToken == input.Token).AnyAsync();
            if (!isValidToken)
            {
                _logger.LogDebug("Webhook token not found");
                return;
            }

            _logger.LogDebug("Create channel message processor");
            var processor = _processorFactory.Create(input.Channel);

            _logger.LogDebug("Process message");
            var result = await processor.Execute(new (input.JsonPayload));

            if (result.SelfMessage)
            {
                _logger.LogDebug("Skip self message");
                return;
            }

            _logger.LogDebug("Message processed");
        }
    }

    public record WebhookMessageProcessorInput(
        ChannelType Channel,
        string Token,
        string JsonPayload
    );
}