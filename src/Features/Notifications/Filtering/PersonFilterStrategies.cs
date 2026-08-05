using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Features.Notifications.Filtering
{
    public class PersonNameFilterStrategy : IPersonFilterStrategy
    {
        public bool CanHandle(string field) =>
            field.Trim().Equals("person.name", StringComparison.OrdinalIgnoreCase);

        public IQueryable<Person> Apply(IQueryable<Person> query, NotificationTargetFilter filter, DateTime today)
        {
            var val = (filter.Value ?? string.Empty).Trim().ToLower();

            return filter.Operator switch
            {
                NotificationFilterOperator.Equals => query.Where(p => p.Name.ToLower() == val),
                NotificationFilterOperator.NotEquals => query.Where(p => p.Name.ToLower() != val),
                _ => query
            };
        }
    }

    public class PersonIdFilterStrategy : IPersonFilterStrategy
    {
        public bool CanHandle(string field) =>
            field.Trim().Equals("person.id", StringComparison.OrdinalIgnoreCase);

        public IQueryable<Person> Apply(IQueryable<Person> query, NotificationTargetFilter filter, DateTime today)
        {
            if (!int.TryParse(filter.Value, out var personId))
                return query;

            return filter.Operator switch
            {
                NotificationFilterOperator.Equals => query.Where(p => p.Id == personId),
                NotificationFilterOperator.NotEquals => query.Where(p => p.Id != personId),
                _ => query
            };
        }
    }

    public class PersonMetadataFilterStrategy : IPersonFilterStrategy
    {
        public bool CanHandle(string field)
        {
            var raw = field.Trim();
            return raw.StartsWith("person.meta.", StringComparison.OrdinalIgnoreCase) ||
                   raw.StartsWith("meta.", StringComparison.OrdinalIgnoreCase) ||
                   !raw.Contains('.');
        }

        public IQueryable<Person> Apply(IQueryable<Person> query, NotificationTargetFilter filter, DateTime today)
        {
            var metaKey = ExtractMetaKey(filter.Field);
            var val = (filter.Value ?? string.Empty).Trim().ToLower();

            switch (filter.Operator)
            {
                case NotificationFilterOperator.Equals:
                    return query.Where(p => p.Metadata.Any(m =>
                        m.Name.ToLower() == metaKey &&
                        m.Value.ToLower() == val));

                case NotificationFilterOperator.NotEquals:
                    return query.Where(p => !p.Metadata.Any(m =>
                        m.Name.ToLower() == metaKey &&
                        m.Value.ToLower() == val));

                case NotificationFilterOperator.DayOfMonthWithin:
                    if (int.TryParse(filter.Value, out var maxDays) && maxDays >= 0)
                    {
                        var validDays = Enumerable.Range(0, maxDays + 1)
                            .SelectMany(i =>
                            {
                                var day = today.AddDays(i).Day;
                                return new[] { day.ToString(), day.ToString("D2") };
                            })
                            .Distinct()
                            .ToList();

                        return query.Where(p => p.Metadata.Any(m =>
                            m.Name.ToLower() == metaKey &&
                            validDays.Contains(m.Value)));
                    }
                    return query;

                default:
                    return query;
            }
        }

        private static string ExtractMetaKey(string field)
        {
            var raw = field.Trim().ToLower();
            if (raw.StartsWith("person.meta."))
                return raw["person.meta.".Length..];

            if (raw.StartsWith("meta."))
                return raw["meta.".Length..];

            return raw;
        }
    }
}
