using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Barista.Core.Data;
using Barista.Core.Events;

namespace Barista.Core.Execution
{
    internal class ProcessExecutionHandler : IExecutionHandler
    {
        private readonly PluginEventsMonitor _eventsMonitor;
        private readonly ConcurrentDictionary<string, Plugin> executions = new ConcurrentDictionary<string, Plugin>();

        public ProcessExecutionHandler(PluginEventsMonitor eventsMonitor)
        {
            _eventsMonitor = eventsMonitor;
        }

        public async Task<(string Data, string Error)> Execute(string fileName, string arguments = "", bool terminal = default)
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = terminal,
                RedirectStandardOutput = true,
            };

            try
            {
                using (var process = Process.Start(info))
                {
                    var data = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();

                    return (Data: data, Error: "");
                }
            }
            catch (Exception ex)
            {
                return (Data: "", Error: ex.ToString());
            }
        }

        public async Task Execute(Plugin plugin)
        {
            if (executions.ContainsKey(plugin.Name)) return;


            if (!executions.TryAdd(plugin.Name, plugin))
            {
                System.Diagnostics.Debug.WriteLine("TODO: weird state add logging");
                return;
            }

            var result = await Execute(plugin.FilePath);
            plugin.LastExecution = DateTime.UtcNow;

            PluginExecution execution;

            if (!string.IsNullOrEmpty(result.Data))
            {
                execution = ParseExecution(result.Data, plugin);
                execution.Plugin = plugin;
            }
            else
            {
                execution = new PluginExecution
                {
                    Plugin = plugin,
                    Items = ImmutableList.CreateBuilder<ImmutableList<Item>>().ToImmutableList(),
                    Success = false,
                };

            }

            if (!executions.TryRemove(plugin.Name, out var _))
            {
                System.Diagnostics.Debug.WriteLine("TODO: weird state add logging");
                System.Diagnostics.Debug.WriteLine("Error removing execution");
            }

            _eventsMonitor.PluginExecuted(execution);
        }

        public async Task Execute(Item item)
        {
            if (item.Type == ItemType.RunScriptAction || item.Type == ItemType.RunScriptInTerminalAction)
            {
                await Execute(item.BashScript, string.Join(" ", item.Params ?? new string[] { }).Trim(), item.Terminal);
            }
            else if (item.Type == ItemType.Link)
            {
                await Execute("open", item.Href);
            }
            else if (item.Type == ItemType.RefreshAction)
            {
                await Execute(item.Plugin);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("TODO: make this log error better");
                System.Diagnostics.Debug.WriteLine("Executing a non executable item!");
            }
        }

        private PluginExecution ParseExecution(string output, Plugin plugin)
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

        private Item ParseItem(string line, List<Item> children)
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
