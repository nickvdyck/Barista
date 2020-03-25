using Cronos;

namespace Barista.Scheduler
{
    public static class Cron
    {
        /// <summary>
        /// Returns cron expression that fires every second.
        /// </summary>
        public static CronExpression Secondly()
        {
            return CronExpression.Parse("* * * * * *", CronFormat.IncludeSeconds);
        }

        /// <summary>
        /// Returns cron expression that fires every minute.
        /// </summary>
        public static CronExpression Minutely()
        {
            return CronExpression.Parse("* * * * *");
        }

        /// <summary>
        /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; seconds.
        /// </summary>
        /// <param name="interval">The number of seconds to wait between every activation.</param>
        public static CronExpression SecondInterval(int interval)
        {
            return CronExpression.Parse($"*/{interval} * * * * *", CronFormat.IncludeSeconds);
        }

        /// <summary>
        /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; minutes.
        /// </summary>
        /// <param name="interval">The number of minutes to wait between every activation.</param>
        public static CronExpression MinuteInterval(int interval)
        {
            return CronExpression.Parse($"*/{interval} * * * *");
        }

        /// <summary>
        /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; hours.
        /// </summary>
        /// <param name="interval">The number of hours to wait between every activation.</param>
        public static CronExpression HourInterval(int interval)
        {
            return CronExpression.Parse($"0 */{interval} * * *");
        }

        /// <summary>
        /// Returns cron expression that fires every &lt;<paramref name="interval"></paramref>&gt; days.
        /// </summary>
        /// <param name="interval">The number of days to wait between every activation.</param>
        public static CronExpression DayInterval(int interval)
        {
            return CronExpression.Parse($"0 0 */{interval} * *");
        }
    }
}
