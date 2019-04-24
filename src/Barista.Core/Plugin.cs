using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Barista
{
    public sealed class Plugin
    {
        internal static PluginType GetPluginType(string extension)
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

        internal static List<PluginRecord> ParseOutput(string output)
        {
            var chunks = output.Split(new [] { "---" }, StringSplitOptions.RemoveEmptyEntries);
            var records = new List<PluginRecord>();

            var title = chunks.FirstOrDefault().Trim();
            records.Add(ParseRecord(title));

            foreach (var chunk in chunks.Skip(1))
            {
                var lines = chunk.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

                var lineList = new List<string>();

                foreach (var line in lines)
                {
                    var record = ParseRecord(line);
                    records.Add(record);
                }

                records.Add(PluginRecord.Seperator);
            }

            return records;
        }

        internal static PluginRecord ParseRecord(string line)
        {
            var parts = line.Split('|');
            var title = parts.FirstOrDefault();
            var attributes = parts.ElementAtOrDefault(1);

            var record = new PluginRecord
            {
                Title = title.ReplaceEmoji()
            };

            if (!string.IsNullOrEmpty(attributes))
            {
                var attrs = attributes.Trim().Split(' ');

                foreach (var attribute in attrs)
                {
                    var chunks = attribute.Split('=');

                    var key = chunks.FirstOrDefault();
                    var value = chunks.ElementAtOrDefault(1);

                    record.Settings.Add(key, value);
                }

            }

            return record;
        }

        public string Name { get; private set; }
        public string Schedule { get; private set; }
        public PluginType Type { get; private set; }

        public Action<IReadOnlyCollection<PluginRecord>> OnExecuted { get; set; }

        private string PhysicalPath { get; set; }
        internal int Interval { get; private set; }
        public DateTime LastExecution { get; private set; } = DateTime.MinValue;

        public Plugin(string filePath)
        {
            PhysicalPath = filePath;
            var fileName = Path.GetFileName(PhysicalPath);
            var (name, schedule, type) = ParseFileName(fileName);

            Name = name;
            Schedule = schedule;
            Type = GetPluginType(type);

            Interval = ParseInterval(Schedule);
        }

        private (string name, string schedule, string type) ParseFileName(string fileName)
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

        internal async Task Execute()
        {
            var result = new List<PluginRecord>();
            try
            {
                var output = await ProcessExecutor.Run(PhysicalPath);
                LastExecution = DateTime.Now;
                result = ParseOutput(output);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 5)
            {
                result.Add(new PluginRecord { Title = "⚠️" });
                result.Add(new PluginRecord
                {
                    Title = "Script is not executable!",
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                result.Add(new PluginRecord { Title = "⚠️" });
            }
            finally
            {
                OnExecuted?.Invoke(result);
            }
        }
    }
}
