using System.Text.RegularExpressions;
using ChatAgentic.Entities;

namespace ChatAgentic.Features.Notifications
{
    public partial class NotificationTagReplacer
    {
        [GeneratedRegex(@"\{\{([^}]+)\}\}")]
        private static partial Regex TagRegex();

        public string Replace(string template, Person person, Workspace workspace, DateTime executionTime)
        {
            if (string.IsNullOrWhiteSpace(template))
                return template;

            return TagRegex().Replace(template, match =>
            {
                var tag = match.Groups[1].Value.Trim();

                if (tag.Equals("person.name", StringComparison.OrdinalIgnoreCase))
                {
                    return person.Name;
                }

                if (tag.StartsWith("person.meta.", StringComparison.OrdinalIgnoreCase))
                {
                    var metaKey = tag["person.meta.".Length..].Trim();
                    var metaItem = person.Metadata?.FirstOrDefault(m => m.Name.Equals(metaKey, StringComparison.OrdinalIgnoreCase));
                    return metaItem?.Value ?? match.Value;
                }

                if (tag.Equals("workspace.name", StringComparison.OrdinalIgnoreCase))
                {
                    return workspace.Name;
                }

                if (tag.Equals("date", StringComparison.OrdinalIgnoreCase))
                {
                    return executionTime.ToString("dd/MM/yyyy");
                }

                if (tag.Equals("time", StringComparison.OrdinalIgnoreCase))
                {
                    return executionTime.ToString("HH:mm");
                }

                return match.Value;
            });
        }
    }
}
