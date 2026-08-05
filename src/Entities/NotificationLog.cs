using ChatAgentic.Features.Channels;

namespace ChatAgentic.Entities
{
    public class NotificationLog
    {
        public long Id { get; set; }

        public int NotificationRuleId { get; set; }
        public NotificationRule NotificationRule { get; set; } = default!;

        public int PersonId { get; set; }
        public Person Person { get; set; } = default!;

        public ChannelType Channel { get; set; }
        public NotificationLogStatus Status { get; set; }
        public string? ErrorMessage { get; set; }

        public Guid ExecutionBatchId { get; set; }
        public string ExecutionPeriodKey { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    public enum NotificationLogStatus
    {
        Sent = 1,
        Failed = 2,
        Skipped = 3
    }
}
