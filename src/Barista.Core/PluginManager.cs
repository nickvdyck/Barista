using System;
using System.Collections.Generic;
using System.Timers;
using Barista.Core.FileSystem;
using Barista.Core.Data;
using System.Linq;
using Barista.Core.Commands;
using Barista.Core.Extensions;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using Barista.Core.Providers;
using Barista.Core.Events;
using Barista.Core.Execution;
using System.Diagnostics;

[assembly: InternalsVisibleTo("Barista.Core.Tests")]

namespace Barista.Core
{
    public sealed class PluginManager : IPluginManager, IDisposable
    {
        public static PluginManager CreateForDirectory(string pluginDirectory)
        {
            var fileProvider = new LocalFileProvider(pluginDirectory);
            var pluginProvider = new PluginFileSystemProvider(fileProvider);
            var monitor = new PluginEventsMonitor();
            var handler = new ProcessExecutionHandler(monitor);
            var executePluginCommand = new ExecutePluginCommand(handler);
            var executeItemCommand = new ExecuteItemCommand(handler);

            return new PluginManager(pluginProvider, monitor, executePluginCommand, executeItemCommand);
        }

        private Timer _timer;
        private readonly IPluginProvider _pluginProvider;
        private readonly IObservable<IPluginEvent> _monitor;
        private readonly ExecutePluginCommand _executePluginCommand;
        private readonly ExecuteItemCommand _executeItemCommand;

        internal PluginManager(IPluginProvider pluginProvider, IObservable<IPluginEvent> monitor, ExecutePluginCommand executePluginCommand, ExecuteItemCommand executeItemCommand)
        {
            _pluginProvider = pluginProvider;
            _monitor = monitor;
            _executePluginCommand = executePluginCommand;
            _executeItemCommand = executeItemCommand;
        }

        public IReadOnlyCollection<Plugin> ListPlugins() =>
            _pluginProvider.ListPlugins();

        public void Execute(int interval)
        {
            if (_timer != null) return;

            _timer = new Timer(interval)
            {
                AutoReset = true,
                Enabled = true,
            };

            _timer.Elapsed += RunPluginLoop;
        }

        private void RunPluginLoop(object sender, ElapsedEventArgs e)
        {
            //Debug.WriteLine($"Elapsed {e.SignalTime}");
            foreach (var plugin in _pluginProvider.ListPlugins())
            {
                if (!plugin.Enabled) continue;

                var offset = e.SignalTime - plugin.LastExecution;

                if (offset.TotalSeconds > plugin.Interval)
                {
                    //System.Diagnostics.Debug.WriteLine($"RunLoop executing plugin {plugin.Name} offset {offset.TotalSeconds} interval {plugin.Interval}");
                    Execute(plugin);
                }
            }
        }

        public void Execute(Plugin plugin)
        {
            _executePluginCommand.Plugin = plugin;
            _executePluginCommand.Execute().Forget();
        }

        public void Execute(Item item)
        {
            _executeItemCommand.Item = item;
            _executeItemCommand.Execute().Forget();
        }

        public IObservable<IPluginEvent> Monitor() => _monitor;

        public void Dispose()
        {
            _timer.Elapsed -= RunPluginLoop;
            _timer.Dispose();
        }
    }
}
