using ChatAgentic.Channels;
using ChatAgentic.Data;
using ChatAgentic.Models;
using Microsoft.Agents.AI.Workflows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Workflows
{
    public sealed partial class SaveConversationExecutor : Executor
    {
        private readonly AppDbContext _dbContext;
        private readonly ILogger _logger;

        public SaveConversationExecutor(AppDbContext dbContext, ILogger<SaveConversationExecutor> logger) : base("SaveConversation")
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
        {
            return protocolBuilder
                .ConfigureRoutes(routes =>
                {
                    routes.AddHandler<WorkflowExecutionContext>(HandleAsync);
                });
        }

        public async ValueTask HandleAsync(WorkflowExecutionContext weContext, IWorkflowContext context, CancellationToken ct)
        {
            _logger.LogDebug("Save conversation started");

            ChatMessage[] newChatMessages = [ ..weContext.InputMessages.Select(x => x.ToChatMessage()).ToArray(), ..weContext.OutputMessages ];

            _logger.LogDebug("{messageCount} new messages to save", newChatMessages.Length);

            foreach (var chatMessage in newChatMessages)
            {
                var messages = chatMessage.MapToConversationMessage();

                foreach (var message in messages)
                {
                    message.ConversationId = weContext.ConversationId;
                    _dbContext.ConversationMessages.Add(message);
                }
            }

            using (var transaction = await _dbContext.Database.BeginTransactionAsync())
            {
                await _dbContext.Conversations.Where(x => x.Id == weContext.ConversationId).ExecuteUpdateAsync(x => x.SetProperty(a => a.ExpireAt, DateTime.UtcNow.AddMinutes(1)));
                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }

            _logger.LogDebug("Save conversation completed");
        }
    }
}