using ChatAgentic.Entities;
using ChatAgentic.Features.Channels;

namespace ChatAgentic.Features.Notifications
{
    public record NotificationRuleInput
    {
        public string Name { get; init; } = string.Empty;
        public List<ChannelType> Channels { get; init; } = [];
        public string MessageTemplate { get; init; } = string.Empty;
        public NotificationFrequency Frequency { get; init; } = NotificationFrequency.Daily;
        public TimeOnly SendTime { get; init; } = new(9, 0);
        public int? SendDay { get; init; }
        public int? SendMonth { get; init; }
        public NotificationTargetType TargetType { get; init; }
        public List<int>? TargetPersonIds { get; init; }
        public List<NotificationTargetFilter>? TargetFilters { get; init; }
        public bool Enabled { get; init; } = true;
    }

    public record NotificationRuleOutput
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public List<ChannelType> Channels { get; init; } = [];
        public string MessageTemplate { get; init; } = string.Empty;
        public NotificationFrequency Frequency { get; init; }
        public TimeOnly SendTime { get; init; }
        public int? SendDay { get; init; }
        public int? SendMonth { get; init; }
        public NotificationTargetType TargetType { get; init; }
        public List<int> TargetPersonIds { get; init; } = [];
        public List<NotificationTargetFilter> TargetFilters { get; init; } = [];
        public bool Enabled { get; init; }
        public DateTime? LastExecutedAt { get; init; }
        public DateTime? NextExecutionAt { get; init; }
        public int WorkspaceId { get; init; }
        public DateTime CreatedAt { get; init; }

        public static NotificationRuleOutput FromEntity(NotificationRule rule) => new()
        {
            Id = rule.Id,
            Name = rule.Name,
            Channels = rule.Channels,
            MessageTemplate = rule.MessageTemplate,
            Frequency = rule.Frequency,
            SendTime = rule.SendTime,
            SendDay = rule.SendDay,
            SendMonth = rule.SendMonth,
            TargetType = rule.TargetType,
            TargetPersonIds = rule.TargetPersonIds,
            TargetFilters = rule.TargetFilters,
            Enabled = rule.Enabled,
            LastExecutedAt = rule.LastExecutedAt,
            NextExecutionAt = rule.NextExecutionAt,
            WorkspaceId = rule.WorkspaceId,
            CreatedAt = rule.CreatedAt
        };
    }
}
