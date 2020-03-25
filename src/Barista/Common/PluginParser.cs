using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Barista.Domain;
using Barista.Scheduler;
using Cronos;

namespace Barista.Common
{
    internal static class PluginParser
    {
        private const string VALIDATE_SCHEDULE_REGEX = "^[0-9]+[smhd]?$";
        public static PluginMetadata FromFilePath(string path)
        {
            var disabled = false;
            var fileName = Path.GetFileName(path);

            if (fileName.StartsWith("_", StringComparison.CurrentCulture))
            {
                fileName = fileName.TrimStart('_');
                disabled = true;
            }

            var (name, schedule, type) = ParseFileName(fileName);

            return new PluginMetadata
            {
                FilePath = path,
                Name = name,
                Schedule = schedule,
                Runtime = GetRuntime(type),
                Cron = ParseScheduleToCron(schedule),
                Disabled = disabled,
            };
        }

        private static (string name, string schedule, string type) ParseFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return ("", "", "");

            var chunks = fileName.Split('.');

            if (chunks.Length == 2)
            {
                var match = Regex.Match(chunks[1], VALIDATE_SCHEDULE_REGEX);

                if (match.Success)
                {
                    return (chunks[0], chunks[1], "");
                }
                else
                {
                    return (chunks[0], "", chunks[1]);
                }

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

        private static PluginRuntime GetRuntime(string extension)
        {
            switch (extension)
            {
                case "sh":
                    return PluginRuntime.Shell;
                case "py":
                    return PluginRuntime.Python;
                case "js":
                    return PluginRuntime.JavaScript;
                default:
                    return PluginRuntime.Unknown;
            }
        }

        private static CronExpression ParseScheduleToCron(string schedule)
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

        public static ImmutableList<ImmutableList<Item>> ParseExecution(string output, string pluginName)
        {
           var chunks = output.Split(new[] { "---" }, StringSplitOptions.RemoveEmptyEntries);

           var itemBuilder = ImmutableList.CreateBuilder<ImmutableList<Item>>();

           var title = chunks.FirstOrDefault().Trim();
           var titleItem = ParseItem(title, new List<Item>(), pluginName);
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
                       var item = ParseItem(line, children, pluginName);
                       builder.Add(item);
                   }
                   else
                   {
                       var item = ParseItem(line.Replace("--", ""), new List<Item>(), pluginName);
                       children.Add(item);
                   }
               }

               itemBuilder.Add(builder.ToImmutable());
           }

           return itemBuilder.ToImmutableList();
        }

        private static Item ParseItem(string line, List<Item> children, string pluginName)
        {
           var parts = line.Split('|');
           var title = parts.FirstOrDefault();
           var attributes = parts.ElementAtOrDefault(1);

           var item = new Item
           {
               OriginalTitle = title,
               PluginName = pluginName,
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
