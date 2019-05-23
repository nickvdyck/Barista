using System.Collections.Immutable;
using System.IO;
using Barista.Core.Data;
using Barista.Core.Jobs;
using Cronos;

namespace Barista.Core.Providers
{
    public abstract class BasePluginProvider : IPluginProvider
    {
        public abstract ImmutableList<Plugin> ListPlugins();
        public Plugin FromFilePath(string path)
        {
            var enabled = true;
            var fileName = Path.GetFileName(path);

            if (fileName.StartsWith("_", System.StringComparison.CurrentCulture))
            {
                fileName = fileName.TrimStart('_');
                enabled = false;
            }

            var (name, schedule, type) = ParseFileName(fileName);

            return new Plugin
            {
                FilePath = path,
                Name = name,
                Schedule = schedule,
                Type = GetPluginType(type),
                Interval = ParseInterval(schedule),
                Cron = ParseIntervalToCron(schedule),
                Enabled = enabled,
            };
        }

        private static (string name, string schedule, string type) ParseFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return ("", "", "");

            var chunks = fileName.Split('.');

            if (chunks.Length == 2)
            {
                return (chunks[0], "", chunks[1]);

            }
            else if (chunks.Length == 3)
            {
                return (chunks[0], chunks[1], chunks[2]);
            }
            else
            {
                return (fileName, "", "");
            }
        }

        private static PluginType GetPluginType(string extension)
        {
            switch (extension)
            {
                case "sh":
                    return PluginType.Shell;
                case "py":
                    return PluginType.Python;
                case "js":
                    return PluginType.JavaScript;
                default:
                    return PluginType.Unknown;
            }
        }

        private const int DEFAULT_TIME_INTERVAL = 60;
        private const int ONE_MINUTE = 60;
        private const int ONE_HOUR = 60 * ONE_MINUTE;
        private const int ONE_DAY = 24 * ONE_HOUR;

        private static int ParseInterval(string schedule)
        {
            if (schedule.Length < 2) return DEFAULT_TIME_INTERVAL;

            var intervalStr = schedule.Substring(0, schedule.Length - 1);


            if (!int.TryParse(intervalStr, out var interval)) return DEFAULT_TIME_INTERVAL;


            var token = schedule[schedule.Length - 1];

            switch (token)
            {
                case 's':
                    return interval;
                case 'm':
                    return interval * ONE_MINUTE;
                case 'h':
                    return interval * ONE_HOUR;
                case 'd':
                    return interval * ONE_DAY;
                default:
                    return DEFAULT_TIME_INTERVAL;
            }
        }

        private static CronExpression ParseIntervalToCron(string schedule)
        {
            if (schedule.Length < 2) return Cron.Minutely();

            var intervalStr = schedule.Substring(0, schedule.Length - 1);


            if (!int.TryParse(intervalStr, out var interval)) return Cron.Minutely();


            var token = schedule[schedule.Length - 1];

            switch (token)
            {
                case 's':
                    // return interval;
                    return Cron.SecondInterval(interval);
                case 'm':
                    // return interval * ONE_MINUTE;
                    return Cron.MinuteInterval(interval);
                case 'h':
                    // return interval * ONE_HOUR;
                    return Cron.HourInterval(interval);
                case 'd':
                    return Cron.DayInterval(interval);
                default:
                    return Cron.Minutely();
            }
        }
    }
}
