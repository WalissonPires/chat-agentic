namespace ChatAgentic.Features.Notifications
{
    public interface INotificationRuleService
    {
        Task<NotificationRuleOutput> CreateAsync(int workspaceId, NotificationRuleInput input, CancellationToken ct = default);
        Task<List<NotificationRuleOutput>> ListAsync(int workspaceId, CancellationToken ct = default);
        Task<NotificationRuleOutput?> GetByIdAsync(int workspaceId, int id, CancellationToken ct = default);
        Task<NotificationRuleOutput?> UpdateAsync(int workspaceId, int id, NotificationRuleInput input, CancellationToken ct = default);
        Task<bool> DeleteAsync(int workspaceId, int id, CancellationToken ct = default);
        Task<Guid?> TriggerAsync(int workspaceId, int id, CancellationToken ct = default);
    }
}
