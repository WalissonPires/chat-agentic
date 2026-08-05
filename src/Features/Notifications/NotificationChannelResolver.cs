using ChatAgentic.Entities;

namespace ChatAgentic.Features.Notifications
{
    public class NotificationChannelResolver
    {
        public Contact? ResolveContact(NotificationRule rule, Person person)
        {
            if (rule.Channels == null || rule.Channels.Count == 0 || person.Contacts == null || person.Contacts.Count == 0)
                return null;

            foreach (var channel in rule.Channels)
            {
                var contact = person.Contacts.FirstOrDefault(c => c.Channel == channel && !string.IsNullOrWhiteSpace(c.Identifier));
                if (contact != null)
                {
                    return contact;
                }
            }

            return null;
        }
    }
}
