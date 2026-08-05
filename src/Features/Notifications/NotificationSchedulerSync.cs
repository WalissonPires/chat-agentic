using ChatAgentic.Entities;
using Hangfire;

namespace ChatAgentic.Features.Notifications
{
    public class NotificationSchedulerSync
    {
        private readonly IRecurringJobManager _recurringJobManager;
        private readonly ILogger<NotificationSchedulerSync> _logger;

        public NotificationSchedulerSync(IRecurringJobManager recurringJobManager, ILogger<NotificationSchedulerSync> logger)
        {
            _recurringJobManager = recurringJobManager;
            _logger = logger;
        }

        public void SyncRule(NotificationRule rule)
        {
            var jobId = $"notification-rule-{rule.Id}";

            if (rule.Enabled)
            {
                var cron = NextExecutionCalculator.ToCronExpression(rule);

                _recurringJobManager.AddOrUpdate<NotificationHangfireJob>(
                    jobId,
                    job => job.ExecuteAsync(rule.Id),
                    cron);

                rule.HangfireJobId = jobId;
                rule.NextExecutionAt = NextExecutionCalculator.CalculateNextExecution(rule, DateTime.UtcNow);

                _logger.LogInformation("Hangfire recurring job synced for rule {RuleId} ({RuleName}) with cron '{Cron}'", rule.Id, rule.Name, cron);
            }
            else
            {
                RemoveRule(rule.Id);
                rule.HangfireJobId = null;
                rule.NextExecutionAt = null;
            }
        }

        public void RemoveRule(int ruleId)
        {
            var jobId = $"notification-rule-{ruleId}";
            _recurringJobManager.RemoveIfExists(jobId);
            _logger.LogInformation("Hangfire recurring job removed for rule {RuleId}", ruleId);
        }
    }
}
