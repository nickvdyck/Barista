using Barista.Data;
using Barista.Common.FileSystem;
using Barista.Common.Jobs;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Collections.Immutable;
using Barista.Common;
using Barista.Data.Commands;

[assembly: InternalsVisibleTo("Barista.Core.Tests")]
namespace Barista
{
    internal sealed class PluginManager : IPluginManager, IDisposable
    {
        private readonly IPluginStore _store;
        private readonly IMediator _mediator;
        private readonly JobScheduler _scheduler;
        private readonly IDisposable _watcherDisposer;

        internal ImmutableList<Plugin> PluginCache { get; set; }

        public PluginManager(IPluginStore store, IMediator mediator, IFileProvider fileProvider, JobScheduler scheduler)
        {
            _store = store;
            _mediator = mediator;
            _scheduler = scheduler;
            _watcherDisposer = fileProvider.Watch(SynchronizePlugins);

            _scheduler.JobException += (ex) =>
            {
                System.Diagnostics.Debug.WriteLine(ex);
            };
        }

        private void SynchronizePlugins() => _mediator.Send(new SyncPluginsCommand());

        // internal PluginManager(IFileProvider fileProvider, IObservable<IPluginEvent> monitor)
        // {
        //     _fileProvider = fileProvider;
        //     _watcherDisposer = _fileProvider.Watch(OnPluginChanged);
        //     _monitor = monitor;
        //     _scheduler = new JobScheduler();
        //     AddPluginsToScheduler();
        // }

        public IReadOnlyCollection<Plugin> ListPlugins() => _store.Plugins;

        public void Start()
        {
            SynchronizePlugins();
            _scheduler.Start();
        }

        public void Stop() => _scheduler.StopAndBlock();

        // public IObservable<IPluginEvent> Monitor() => _monitor;

        public void Execute(Plugin plugin)
        {
            if (plugin.Disabled) return;

            _scheduler.RunSchedule(plugin.Name);
        }

        public void Execute(Item item)
        {
            if (item.Type == ItemType.RefreshAction)
            {
                Execute(item.Plugin);
                return;
            }

            _scheduler.Schedule(() =>
                _mediator.Send(new ExecuteItemCommand
                {
                    Item = item,
                }))
                .ToRunNow()
                .ToRunOnce()
                .Start();
        }

        public void Dispose()
        {
            _watcherDisposer.Dispose();
            _scheduler.Stop();
        }
    }
}
