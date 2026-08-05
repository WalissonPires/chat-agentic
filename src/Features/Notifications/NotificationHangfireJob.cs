using Hangfire;

namespace ChatAgentic.Features.Notifications
{
    public class NotificationHangfireJob
    {
        private readonly NotificationDispatcher _dispatcher;
        private readonly ILogger<NotificationHangfireJob> _logger;

        public NotificationHangfireJob(NotificationDispatcher dispatcher, ILogger<NotificationHangfireJob> logger)
        {
            _dispatcher = dispatcher;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 3)]
        public async Task ExecuteAsync(int notificationRuleId)
        {
            _logger.LogInformation("Hangfire triggered execution for NotificationRule {RuleId}", notificationRuleId);
            await _dispatcher.DispatchRuleAsync(notificationRuleId);
        }
    }
}
