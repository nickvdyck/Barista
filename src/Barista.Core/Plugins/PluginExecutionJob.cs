using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using Barista.Core.Data;
using Barista.Core.Events;
using Barista.Core.Jobs;

namespace Barista.Core.Plugins
{
    public class PluginExecutionJob : IJob
    {
        private readonly Plugin _plugin;
        private readonly PluginEventsMonitor _monitor;

        public PluginExecutionJob(Plugin plugin, PluginEventsMonitor monitor)
        {
            _plugin = plugin;
            _monitor = monitor;
        }

        public async Task Execute()
        {
            System.Diagnostics.Debug.WriteLine($"Executing plugin {_plugin.Name}");

            var info = new ProcessStartInfo
            {
                FileName = _plugin.FilePath,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            (string Data, string Error) result;

            try
            {
                using (var process = Process.Start(info))
                {
                    var data = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();

                    result = (Data: data, Error: "");
                }
            }
            catch (Exception ex)
            {
                result = (Data: "", Error: ex.ToString());
            }

            PluginExecution execution;

            if (!string.IsNullOrEmpty(result.Data))
            {
                execution = PluginParser.ParseExecution(result.Data, _plugin);
                execution.Plugin = _plugin;
                execution.LastExecution = DateTime.UtcNow;
            }
            else
            {
                execution = new PluginExecution
                {
                    Plugin = _plugin,
                    Items = ImmutableList.CreateBuilder<ImmutableList<Item>>().ToImmutableList(),
                    Success = false,
                    LastExecution = DateTime.UtcNow,
                };
            }

            _monitor.PluginExecuted(execution);
        }
    }
}
