using ChatAgentic.Channels;
using ChatAgentic.Data;
using ChatAgentic.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Workflows
{
    public sealed partial class LoadConversationExecutor : Executor
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public LoadConversationExecutor(AppDbContext dbContext, ILogger<LoadConversationExecutor> logger) : base("LoadSession")
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
        {
            return protocolBuilder
                .SendsMessage<WorkflowExecutionContext>()
                .ConfigureRoutes(routes =>
                {
                    routes.AddHandler<Message>(HandleAsync);
                });
        }

        public async ValueTask HandleAsync(Message message, IWorkflowContext context, CancellationToken ct)
        {
            _logger.LogDebug("Load conversation started");

            var conversation = await _dbContext.Conversations.AsNoTracking()
                .Include(x => x.Messages)
                .Where(x => x.WorkspaceId == message.WorkspaceId && x.Channel == message.Channel && x.SenderIdentifier == message.SenderIdentifier && x.ExpireAt > DateTime.UtcNow)
                .FirstOrDefaultAsync();

            if (conversation == null)
            {
                _logger.LogDebug("No active conversations found. Creating a new conversation.");

                conversation = new Models.Conversation
                {
                    WorkspaceId = message.WorkspaceId,
                    Channel = message.Channel,
                    SenderIdentifier = message.SenderIdentifier,
                    CreatedAt = DateTime.UtcNow,
                    ExpireAt = DateTime.UtcNow.AddMinutes(1),
                    Messages = []
                };

                _dbContext.Conversations.Add(conversation);
                await _dbContext.SaveChangesAsync(ct);

                _logger.LogDebug("Conversation {conversationId} created", conversation.Id);
            }
            else
                _logger.LogDebug("Active conversation loaded. Conversation {conversationId}. {messagesCount} Messages", conversation.Id, conversation.Messages.Count);

            var weContext = new WorkflowExecutionContext(
                WorkspaceId: message.WorkspaceId,
                ConversationId: conversation.Id,
                Channel: message.Channel,
                SenderIdentifier: message.SenderIdentifier,
                Messages: conversation.Messages.Select(x => x.MapToChatMessage()).ToList(),
                LastMessagesCount: conversation.Messages.Count
            );

            weContext.Messages.Add(message.ToChatMessage());

            await context.SendMessageAsync(weContext, ct);

            _logger.LogDebug("Load conversation completed");
        }
    }
}