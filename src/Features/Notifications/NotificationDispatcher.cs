using ChatAgentic.Entities;
using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Workflows;
using ChatAgentic.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.Notifications
{
    public class NotificationDispatcher
    {
        private readonly AppDbContext _db;
        private readonly NotificationPersonResolver _personResolver;
        private readonly NotificationChannelResolver _channelResolver;
        private readonly NotificationTagReplacer _tagReplacer;
        private readonly ChannelSendMessageFactory _channelSendMessageFactory;
        private readonly WorkspaceLoader _workspaceLoader;
        private readonly WorkflowContext _workflowContext;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            AppDbContext db,
            NotificationPersonResolver personResolver,
            NotificationChannelResolver channelResolver,
            NotificationTagReplacer tagReplacer,
            ChannelSendMessageFactory channelSendMessageFactory,
            WorkspaceLoader workspaceLoader,
            WorkflowContext workflowContext,
            ILogger<NotificationDispatcher> logger)
        {
            _db = db;
            _personResolver = personResolver;
            _channelResolver = channelResolver;
            _tagReplacer = tagReplacer;
            _channelSendMessageFactory = channelSendMessageFactory;
            _workspaceLoader = workspaceLoader;
            _workflowContext = workflowContext;
            _logger = logger;
        }

        public async Task<Guid> DispatchRuleAsync(int notificationRuleId, CancellationToken ct = default)
        {
            var rule = await _db.NotificationRules
                .Include(r => r.Workspace)
                .FirstOrDefaultAsync(r => r.Id == notificationRuleId, ct);

            if (rule == null)
            {
                _logger.LogWarning("NotificationRule {RuleId} not found.", notificationRuleId);
                return Guid.Empty;
            }

            var workspace = await _workspaceLoader.LoadFromWorkspaceIdAsync(rule.WorkspaceId, ct) ?? rule.Workspace;

            var agentDefinition = await _db.Agents.AsNoTracking()
                .FirstOrDefaultAsync(a => a.WorkspaceId == rule.WorkspaceId, ct);

            if (agentDefinition != null)
            {
                _workflowContext.SetFromAgentDefinition(agentDefinition);
            }

            var now = DateTime.UtcNow;
            var batchId = Guid.NewGuid();
            var periodKey = NextExecutionCalculator.GetExecutionPeriodKey(rule.Frequency, now);

            var targetPeople = await _personResolver.ResolveTargetPeopleAsync(rule, ct);

            _logger.LogInformation("Dispatching notification rule {RuleName} (ID: {RuleId}) to {Count} people. BatchId: {BatchId}, PeriodKey: {PeriodKey}",
                rule.Name, rule.Id, targetPeople.Count, batchId, periodKey);

            foreach (var person in targetPeople)
            {
                var alreadySent = await _db.NotificationLogs.AnyAsync(l =>
                    l.NotificationRuleId == rule.Id &&
                    l.PersonId == person.Id &&
                    l.ExecutionPeriodKey == periodKey &&
                    l.Status == NotificationLogStatus.Sent, ct);

                if (alreadySent)
                {
                    _logger.LogInformation("Skipping Person {PersonId} - Notification already sent in period {PeriodKey}", person.Id, periodKey);
                    continue;
                }

                var contact = _channelResolver.ResolveContact(rule, person);
                if (contact == null)
                {
                    _db.NotificationLogs.Add(new NotificationLog
                    {
                        NotificationRuleId = rule.Id,
                        PersonId = person.Id,
                        Channel = rule.Channels.FirstOrDefault(),
                        Status = NotificationLogStatus.Skipped,
                        ErrorMessage = "No available contact for configured channels",
                        ExecutionBatchId = batchId,
                        ExecutionPeriodKey = periodKey,
                        SentAt = DateTime.UtcNow
                    });
                    continue;
                }

                var messageText = _tagReplacer.Replace(rule.MessageTemplate, person, workspace, now);

                try
                {
                    var sender = _channelSendMessageFactory.Create(contact.Channel);
                    var chatMessage = new ChatMessage(ChatRole.Assistant, messageText);

                    await sender.ExecuteAsync(new ChannelSendMessageInput(
                        SenderIdentifier: contact.Identifier,
                        ChatId: contact.Identifier,
                        Message: chatMessage
                    ), ct);

                    _db.NotificationLogs.Add(new NotificationLog
                    {
                        NotificationRuleId = rule.Id,
                        PersonId = person.Id,
                        Channel = contact.Channel,
                        Status = NotificationLogStatus.Sent,
                        ExecutionBatchId = batchId,
                        ExecutionPeriodKey = periodKey,
                        SentAt = DateTime.UtcNow
                    });

                    _logger.LogInformation("Notification sent successfully to Person {PersonId} via {Channel}", person.Id, contact.Channel);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send notification to Person {PersonId} via {Channel}", person.Id, contact.Channel);

                    _db.NotificationLogs.Add(new NotificationLog
                    {
                        NotificationRuleId = rule.Id,
                        PersonId = person.Id,
                        Channel = contact.Channel,
                        Status = NotificationLogStatus.Failed,
                        ErrorMessage = ex.Message,
                        ExecutionBatchId = batchId,
                        ExecutionPeriodKey = periodKey,
                        SentAt = DateTime.UtcNow
                    });
                }
            }

            rule.LastExecutedAt = now;
            rule.NextExecutionAt = NextExecutionCalculator.CalculateNextExecution(rule, now);

            await _db.SaveChangesAsync(ct);

            return batchId;
        }
    }
}
