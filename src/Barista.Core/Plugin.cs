using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Barista
{
    public class Plugin
    {
        internal static PluginType GetPluginType(string extension)
        {
            switch (extension)
            {
                case "sh":
                    return PluginType.Shell;
                case "py":
                    return PluginType.Python;
                default:
                    return PluginType.Unknown;
            }
        }

        private const int DEFAULT_TIME_INTERVAL = 60;

        private const int ONE_MINUTE = 60;
        private const int ONE_HOUR = 60 * ONE_MINUTE;
        private const int ONE_DAY = 24 * ONE_HOUR;

        internal static int ParseInterval(string schedule)
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

        internal static string ParseOutput(string output)
        {
            var title = output.Split('-', '-', '-').FirstOrDefault();

            title = Regex.Replace(title, @"\t|\n|\r", "");
            return title;
        }

        public string Name { get; private set; }
        public string Schedule { get; private set; }
        public PluginType Type { get; private set; }

        public Action<string> OnExecuted { get; set; }

        private string PhysicalPath { get; set; }
        internal int Interval { get; private set; }
        public DateTime LastExecution { get; private set; } = DateTime.MinValue;

        public Plugin(string filePath)
        {
            PhysicalPath = filePath;
            var fileName = Path.GetFileName(PhysicalPath);
            var chunks = fileName.Split('.');

            Name = chunks[0];
            Schedule = chunks[1];
            Type = GetPluginType(chunks[2]);

            Interval = ParseInterval(Schedule);
        }

        internal async Task Execute()
        {
            string title = string.Empty;
            try
            {
                var output = await ProcessExecutor.Run(PhysicalPath);
                LastExecution = DateTime.Now;
                title = ParseOutput(output);
            }
            catch (Exception)
            {
                title = "⚠️";
            }
            finally
            {
                OnExecuted?.Invoke(title);
            }
        }
    }
}
