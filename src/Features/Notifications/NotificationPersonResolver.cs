using ChatAgentic.Entities;
using ChatAgentic.Features.Notifications.Filtering;
using ChatAgentic.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Features.Notifications
{
    public class NotificationPersonResolver
    {
        private readonly AppDbContext _db;
        private readonly IEnumerable<IPersonFilterStrategy> _strategies;

        public NotificationPersonResolver(AppDbContext db, IEnumerable<IPersonFilterStrategy> strategies)
        {
            _db = db;
            _strategies = strategies;
        }

        public async Task<List<Person>> ResolveTargetPeopleAsync(NotificationRule rule, CancellationToken ct = default)
        {
            var query = _db.People
                .Include(p => p.Contacts)
                .Where(p => p.WorkspaceId == rule.WorkspaceId);

            switch (rule.TargetType)
            {
                case NotificationTargetType.All:
                    return await query.ToListAsync(ct);

                case NotificationTargetType.Specific:
                    if (rule.TargetPersonIds == null || rule.TargetPersonIds.Count == 0)
                        return [];

                    return await query
                        .Where(p => rule.TargetPersonIds.Contains(p.Id))
                        .ToListAsync(ct);

                case NotificationTargetType.Dynamic:
                    if (rule.TargetFilters != null && rule.TargetFilters.Count > 0)
                    {
                        var today = DateTime.UtcNow.Date;
                        query = ApplyDynamicFilters(query, rule.TargetFilters, today);
                    }

                    return await query.ToListAsync(ct);

                default:
                    return [];
            }
        }

        private IQueryable<Person> ApplyDynamicFilters(IQueryable<Person> query, List<NotificationTargetFilter> filters, DateTime today)
        {
            foreach (var filter in filters)
            {
                if (string.IsNullOrWhiteSpace(filter.Field))
                    continue;

                var strategy = _strategies.FirstOrDefault(s => s.CanHandle(filter.Field));
                if (strategy != null)
                {
                    query = strategy.Apply(query, filter, today);
                }
            }

            return query;
        }
    }
}
