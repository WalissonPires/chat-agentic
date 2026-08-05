using ChatAgentic.Entities;

namespace ChatAgentic.Features.Notifications
{
    public static class NextExecutionCalculator
    {
        public static string GetExecutionPeriodKey(NotificationFrequency frequency, DateTime date)
        {
            return frequency switch
            {
                NotificationFrequency.Daily => date.ToString("yyyy-MM-dd"),
                NotificationFrequency.Monthly => date.ToString("yyyy-MM"),
                NotificationFrequency.Yearly => date.ToString("yyyy"),
                _ => date.ToString("yyyy-MM-dd")
            };
        }

        public static DateTime CalculateNextExecution(NotificationRule rule, DateTime fromUtc)
        {
            var time = rule.SendTime;

            if (rule.SendMonth.HasValue)
            {
                var month = Math.Clamp(rule.SendMonth.Value, 1, 12);
                var day = Math.Min(rule.SendDay ?? 1, DateTime.DaysInMonth(fromUtc.Year, month));
                var candidate = new DateTime(fromUtc.Year, month, day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
                if (candidate <= fromUtc)
                {
                    var nextYear = fromUtc.Year + 1;
                    day = Math.Min(rule.SendDay ?? 1, DateTime.DaysInMonth(nextYear, month));
                    candidate = new DateTime(nextYear, month, day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
                }
                return candidate;
            }

            if (rule.SendDay.HasValue)
            {
                var day = Math.Min(rule.SendDay.Value, 28);
                var candidate = new DateTime(fromUtc.Year, fromUtc.Month, day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
                if (candidate <= fromUtc)
                {
                    var nextMonth = fromUtc.AddMonths(1);
                    var daysInMonth = DateTime.DaysInMonth(nextMonth.Year, nextMonth.Month);
                    day = Math.Min(rule.SendDay.Value, daysInMonth);
                    candidate = new DateTime(nextMonth.Year, nextMonth.Month, day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
                }
                return candidate;
            }

            // Default: Daily
            var dailyCandidate = new DateTime(fromUtc.Year, fromUtc.Month, fromUtc.Day, time.Hour, time.Minute, time.Second, DateTimeKind.Utc);
            if (dailyCandidate <= fromUtc)
                dailyCandidate = dailyCandidate.AddDays(1);

            return dailyCandidate;
        }

        public static string ToCronExpression(NotificationRule rule)
        {
            var minute = rule.SendTime.Minute;
            var hour = rule.SendTime.Hour;

            if (rule.SendMonth.HasValue)
                return $"{minute} {hour} {rule.SendDay ?? 1} {rule.SendMonth.Value} *";

            if (rule.SendDay.HasValue)
                return $"{minute} {hour} {rule.SendDay.Value} * *";

            return $"{minute} {hour} * * *";
        }
    }
}
