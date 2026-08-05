using ChatAgentic.Entities;
using ChatAgentic.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Features.Notifications
{
    public class NotificationRuleService : INotificationRuleService
    {
        private readonly AppDbContext _db;
        private readonly NotificationSchedulerSync _schedulerSync;
        private readonly NotificationDispatcher _dispatcher;
        private readonly ILogger<NotificationRuleService> _logger;

        public NotificationRuleService(
            AppDbContext db,
            NotificationSchedulerSync schedulerSync,
            NotificationDispatcher dispatcher,
            ILogger<NotificationRuleService> logger)
        {
            _db = db;
            _schedulerSync = schedulerSync;
            _dispatcher = dispatcher;
            _logger = logger;
        }

        public async Task<NotificationRuleOutput> CreateAsync(int workspaceId, NotificationRuleInput input, CancellationToken ct = default)
        {
            var rule = new NotificationRule
            {
                Name = input.Name,
                Channels = input.Channels,
                MessageTemplate = input.MessageTemplate,
                Frequency = input.Frequency,
                SendTime = input.SendTime,
                SendDay = input.SendDay,
                SendMonth = input.SendMonth,
                TargetType = input.TargetType,
                TargetPersonIds = input.TargetPersonIds ?? [],
                TargetFilters = input.TargetFilters ?? [],
                Enabled = input.Enabled,
                WorkspaceId = workspaceId,
                CreatedAt = DateTime.UtcNow
            };

            _db.NotificationRules.Add(rule);
            await _db.SaveChangesAsync(ct);

            _schedulerSync.SyncRule(rule);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("NotificationRule {RuleId} created in workspace {WorkspaceId}", rule.Id, workspaceId);

            return NotificationRuleOutput.FromEntity(rule);
        }

        public async Task<List<NotificationRuleOutput>> ListAsync(int workspaceId, CancellationToken ct = default)
        {
            var rules = await _db.NotificationRules
                .AsNoTracking()
                .Where(r => r.WorkspaceId == workspaceId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync(ct);

            return rules.Select(NotificationRuleOutput.FromEntity).ToList();
        }

        public async Task<NotificationRuleOutput?> GetByIdAsync(int workspaceId, int id, CancellationToken ct = default)
        {
            var rule = await _db.NotificationRules
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == workspaceId, ct);

            return rule != null ? NotificationRuleOutput.FromEntity(rule) : null;
        }

        public async Task<NotificationRuleOutput?> UpdateAsync(int workspaceId, int id, NotificationRuleInput input, CancellationToken ct = default)
        {
            var rule = await _db.NotificationRules
                .FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == workspaceId, ct);

            if (rule == null)
                return null;

            rule.Name = input.Name;
            rule.Channels = input.Channels;
            rule.MessageTemplate = input.MessageTemplate;
            rule.Frequency = input.Frequency;
            rule.SendTime = input.SendTime;
            rule.SendDay = input.SendDay;
            rule.SendMonth = input.SendMonth;
            rule.TargetType = input.TargetType;
            rule.TargetPersonIds = input.TargetPersonIds ?? [];
            rule.TargetFilters = input.TargetFilters ?? [];
            rule.Enabled = input.Enabled;

            _schedulerSync.SyncRule(rule);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("NotificationRule {RuleId} updated in workspace {WorkspaceId}", rule.Id, workspaceId);

            return NotificationRuleOutput.FromEntity(rule);
        }

        public async Task<bool> DeleteAsync(int workspaceId, int id, CancellationToken ct = default)
        {
            var rule = await _db.NotificationRules
                .FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == workspaceId, ct);

            if (rule == null)
                return false;

            _schedulerSync.RemoveRule(rule.Id);
            _db.NotificationRules.Remove(rule);
            await _db.SaveChangesAsync(ct);

            _logger.LogInformation("NotificationRule {RuleId} deleted from workspace {WorkspaceId}", rule.Id, workspaceId);

            return true;
        }

        public async Task<Guid?> TriggerAsync(int workspaceId, int id, CancellationToken ct = default)
        {
            var rule = await _db.NotificationRules
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && r.WorkspaceId == workspaceId, ct);

            if (rule == null)
                return null;

            return await _dispatcher.DispatchRuleAsync(id, ct);
        }
    }
}
