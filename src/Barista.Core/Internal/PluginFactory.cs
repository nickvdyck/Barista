using System.IO;
using Barista.Core.Data;

namespace Barista.Core.Internal
{
    internal class PluginFactory : IPluginFactory
    {
        public Plugin FromFilePath(string path)
        {
            var fileName = Path.GetFileName(path);
            var (name, schedule, type) = ParseFileName(fileName);

            return new Plugin
            {
                FilePath = path,
                Name = name,
                Schedule = schedule,
                Type = GetPluginType(type),
                Interval = ParseInterval(schedule)
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
    }
}
