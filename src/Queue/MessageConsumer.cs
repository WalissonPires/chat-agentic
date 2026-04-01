using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Workflows;
using ChatAgentic.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace ChatAgentic.Queue
{
    public class MessageConsumer : BackgroundService
    {
        private readonly IMessageQueue<Message> _queue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;

        public MessageConsumer(IMessageQueue<Message> queue, IServiceScopeFactory scopeFactory, ILogger<MessageConsumer> logger)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Background consumer started.");

            await foreach (var message in _queue.DequeueAllAsync(stoppingToken))
            {
                _logger.LogInformation("Processing message for {channel}: {identifier}", message.Channel, message.SenderIdentifier);
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var sp = scope.ServiceProvider;
                    var workspaceLoader = sp.GetRequiredService<WorkspaceLoader>();
                    var workspace = await workspaceLoader.LoadFromWorkspaceIdAsync(message.WorkspaceId, stoppingToken);
                    if (workspace == null)
                    {
                        _logger.LogError("Workspace {WorkspaceId} not found for queued message", message.WorkspaceId);
                        continue;
                    }

                    var workflow = sp.GetRequiredService<AssistentWorkflow>();

                    await workflow.RunAsync(message, stoppingToken);

                    _logger.LogInformation("Processing completed for {channel}: {identifier}", message.Channel, message.SenderIdentifier);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled error processing message for {channel}: {identifier}", message.Channel, message.SenderIdentifier);
                }
            }

            _logger.LogInformation("Background consumer stopped.");
        }
    }
}