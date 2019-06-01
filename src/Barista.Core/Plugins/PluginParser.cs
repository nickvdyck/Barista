using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Barista.Core.Data;
using Barista.Core.Jobs;
using Cronos;

namespace Barista.Core.Plugins
{
    internal static class PluginParser
    {
        public static Plugin FromFilePath(string path)
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






        public static PluginExecution ParseExecution(string output, Plugin plugin)
        {
            var chunks = output.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

            var itemBuilder = ImmutableList.CreateBuilder<ImmutableList<Item>>();

            var title = chunks.FirstOrDefault().Trim();
            var titleItem = ParseItem(title, new List<Item>());
            titleItem.Plugin = plugin;
            itemBuilder.Add(ImmutableList.Create(titleItem));

            foreach (var chunk in chunks.Skip(1))
            {
                var lines = chunk.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var builder = ImmutableList.CreateBuilder<Item>();
                var children = new List<Item>();

                foreach (var line in lines)
                {
                    if (!line.StartsWith("--", StringComparison.CurrentCulture))
                    {
                        children = new List<Item>();
                        var item = ParseItem(line, children);
                        item.Plugin = plugin;
                        builder.Add(item);
                    }
                    else
                    {
                        var item = ParseItem(line.Replace("--", ""), new List<Item>());
                        item.Plugin = plugin;
                        children.Add(item);
                    }
                }

                itemBuilder.Add(builder.ToImmutable());
            }

            return new PluginExecution
            {
                Items = itemBuilder.ToImmutableList(),
            };
        }

        private static Item ParseItem(string line, List<Item> children)
        {
            var parts = line.Split('|');
            var title = parts.FirstOrDefault();
            var attributes = parts.ElementAtOrDefault(1);

            var item = new Item
            {
                OriginalTitle = title,
                Children = children,
            };

            if (!string.IsNullOrEmpty(attributes))
            {
                var attrs = attributes.Trim().Split(' ');

                foreach (var attribute in attrs)
                {
                    var chunks = attribute.Split('=');

                    var key = chunks.FirstOrDefault();
                    var value = chunks.ElementAtOrDefault(1);

                    item.Settings.Add(key, value);
                }
            }

            return item;
        }
    }
}
