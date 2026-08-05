using ChatAgentic.Features.Channels;

namespace ChatAgentic.Entities
{
    public class NotificationRule
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<ChannelType> Channels { get; set; } = [];
        public string MessageTemplate { get; set; } = string.Empty;
        
        public NotificationFrequency Frequency { get; set; }
        public TimeOnly SendTime { get; set; } = new(9, 0);
        public int? SendDay { get; set; }
        public int? SendMonth { get; set; }

        public NotificationTargetType TargetType { get; set; }
        public List<int> TargetPersonIds { get; set; } = [];
        public List<NotificationTargetFilter> TargetFilters { get; set; } = [];
        public bool Enabled { get; set; } = true;
        public DateTime? LastExecutedAt { get; set; }
        public DateTime? NextExecutionAt { get; set; }
        public string? HangfireJobId { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum NotificationFrequency
    {
        Daily = 1,
        Monthly = 2,
        Yearly = 3
    }

    public enum NotificationTargetType
    {
        All = 1,
        Specific = 2,
        Dynamic = 3
    }

    public class NotificationTargetFilter
    {
        public string Field { get; set; } = string.Empty;
        public NotificationFilterOperator Operator { get; set; } = NotificationFilterOperator.Equals;
        public string Value { get; set; } = string.Empty;
    }

    public enum NotificationFilterOperator
    {
        Equals = 1,
        NotEquals = 2,
        DayOfMonthWithin = 3
    }
}
