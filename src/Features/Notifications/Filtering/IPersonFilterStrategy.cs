using ChatAgentic.Entities;

namespace ChatAgentic.Features.Notifications.Filtering
{
    public interface IPersonFilterStrategy
    {
        bool CanHandle(string field);
        IQueryable<Person> Apply(IQueryable<Person> query, NotificationTargetFilter filter, DateTime today);
    }
}
