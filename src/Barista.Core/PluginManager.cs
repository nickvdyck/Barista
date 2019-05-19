﻿using Barista.Core.Data;
using Barista.Core.Events;
using Barista.Core.Execution;
using Barista.Core.Extensions;
using Barista.Core.Providers;
using Barista.Core.FileSystem;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Timers;

[assembly: InternalsVisibleTo("Barista.Core.Tests")]

namespace Barista.Core
{
    public sealed class PluginManager : IPluginManager, IDisposable
    {
        public static PluginManager CreateForDirectory(string pluginDirectory, IFileSystemWatcher watcher)
        {
            var fileProvider = new LocalFileProvider(pluginDirectory, watcher);
            var monitor = new PluginEventsMonitor();
            var pluginProvider = new PluginFileSystemProvider(fileProvider, monitor);
            var handler = new ProcessExecutionHandler(monitor);

            return new PluginManager(pluginProvider, monitor, handler);
        }

        private Timer _timer;
        private readonly IPluginProvider _pluginProvider;
        private readonly IObservable<IPluginEvent> _monitor;
        private readonly IExecutionHandler _executionHandler;

        internal PluginManager(IPluginProvider pluginProvider, IObservable<IPluginEvent> monitor, IExecutionHandler executionHandler)
        {
            _pluginProvider = pluginProvider;
            _monitor = monitor;
            _executionHandler = executionHandler;
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
            foreach (var plugin in _pluginProvider.ListPlugins())
            {
                if (!plugin.Enabled) continue;

                var offset = DateTime.Now - plugin.LastExecution;

                if (offset.TotalSeconds >= plugin.Interval) Execute(plugin);
            }
        }

        public void Execute(Plugin plugin) => _executionHandler.Execute(plugin).Forget();

        public void Execute(Item item) => _executionHandler.Execute(item).Forget();

        public IObservable<IPluginEvent> Monitor() => _monitor;

        public void Dispose()
        {
            _timer.Elapsed -= RunPluginLoop;
            _timer.Dispose();
        }
    }
}
