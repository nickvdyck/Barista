using System;
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
            var result = await Execute(plugin.FilePath);
            plugin.LastExecution = DateTime.Now;

            var execution = ParseExecution(result.Data, plugin);
            execution.Plugin = plugin;

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
            var titleItem = ParseItem(title);
            titleItem.Plugin = plugin;
            itemBuilder.Add(ImmutableList.Create(titleItem));

            foreach (var chunk in chunks.Skip(1))
            {
                var lines = chunk.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                var builder = ImmutableList.CreateBuilder<Item>();

                foreach (var line in lines)
                {
                    var item = ParseItem(line);
                    item.Plugin = plugin;
                    builder.Add(item);
                }

                itemBuilder.Add(builder.ToImmutable());
            }

            return new PluginExecution
            {
                Items = itemBuilder.ToImmutableList(),
            };
        }

        private Item ParseItem(string line)
        {
            var parts = line.Split('|');
            var title = parts.FirstOrDefault();
            var attributes = parts.ElementAtOrDefault(1);

            var item = new Item
            {
                OriginalTitle = title
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
