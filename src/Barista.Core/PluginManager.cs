using Barista.Core.Data;
using Barista.Core.FileSystem;
using Barista.Core.Jobs;
using Barista.Core.Plugins;
using Barista.Core.Plugins.Events;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;
using System.Linq;

[assembly: InternalsVisibleTo("Barista.Core.Tests")]
namespace Barista.Core
{
    public sealed class PluginManager : IPluginManager, IDisposable
    {
        public static PluginManager CreateForDirectory(string pluginDirectory, IFileSystemWatcher watcher)
        {
            var fileProvider = new LocalFileProvider(pluginDirectory, watcher);
            var monitor = new PluginEventsMonitor();

            return new PluginManager(fileProvider, monitor);
        }

        private readonly IFileProvider _fileProvider;
        private readonly IDisposable _watcherDisposer;
        private readonly IObservable<IPluginEvent> _monitor;
        private readonly JobScheduler _scheduler;

        internal ImmutableList<Plugin> PluginCache { get; set; }

        internal PluginManager(IFileProvider fileProvider, IObservable<IPluginEvent> monitor)
        {
            _fileProvider = fileProvider;
            _watcherDisposer = _fileProvider.Watch(OnPluginChanged);
            _monitor = monitor;
            _scheduler = new JobScheduler();
            AddPluginsToScheduler();
        }

        public IReadOnlyCollection<Plugin> ListPlugins() => PluginCache;

        public void Start()
        {
            _scheduler.Start();
        }

        public void Stop() => _scheduler.StopAndBlock();

        public IObservable<IPluginEvent> Monitor() => _monitor;

        public void Execute(Plugin plugin)
        {
            if (plugin.Disabled) return;

            var job = new PluginExecutionJob(plugin, _monitor as PluginEventsMonitor);
            _scheduler.Schedule(job).ToRunNow().ToRunOnce().Start();
        }

        public void Execute(Item item)
        {
            if (item.Type == ItemType.RefreshAction)
            {
                Execute(item.Plugin);
            }
            else
            {
                var job = new ItemExecutionJob(item);
                _scheduler.Schedule(job).ToRunNow().ToRunOnce().Start();
            }
        }

        public void Dispose()
        {
            _watcherDisposer.Dispose();
            _scheduler.Stop();
        }

        private void LoadPlugins()
        {
            var files = _fileProvider.GetDirectoryContents();

            if (!files.Exists) throw new Exception("Wow something went wrong, it looks like your plugin directory does not exist!");

            var builder = ImmutableList.CreateBuilder<Plugin>();
            foreach (var file in files)
            {
                if (file.IsDirectory) continue;
                var plugin = PluginParser.FromFilePath(file.PhysicalPath);

                builder.Add(plugin);
            }

            PluginCache = builder.ToImmutable();
        }

        private void AddPluginsToScheduler()
        {
            LoadPlugins();

            var builder = _scheduler.ScheduleMany();

            foreach (var plugin in ListPlugins().Where(p => !p.Disabled))
            {
                var job = new PluginExecutionJob(plugin, _monitor as PluginEventsMonitor);
                builder.Schedule(job).WithName(plugin.Name).ToRunNow().ToRunAt(plugin.Cron).Start();
            }

            builder.Build();
        }

        // TODO: This should be smarter, the filewatcher can tell me wich plugin
        // has changed
        private void OnPluginChanged()
        {
            LoadPlugins();
            var plugins = ListPlugins();

            foreach (var plugin in plugins)
            {
                var schedule = _scheduler.GetSchedule(plugin.Name);

                if (plugin.Disabled && schedule != null)
                {
                    _scheduler.RemoveJob(schedule.Name);
                    continue;
                }

                if (!plugin.Disabled && schedule == null)
                {
                    var job = new PluginExecutionJob(plugin, _monitor as PluginEventsMonitor);
                    _scheduler.Schedule(job)
                        .WithName(plugin.Name)
                        .ToRunNow()
                        .ToRunAt(plugin.Cron)
                        .Start();
                }
            }

            foreach (var schedule in _scheduler.AllSchedules)
            {
                var plugin = plugins.FirstOrDefault(p => p.Name == schedule.Name);

                if (plugin == null)
                {
                    _scheduler.RemoveJob(schedule.Name);
                }
            }

            (_monitor as PluginEventsMonitor).PluginsChanged();
        }
    }
}
