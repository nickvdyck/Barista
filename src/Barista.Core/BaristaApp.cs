using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using Barista.Common.FileSystem;
using Barista.Common.Redux;
using Barista.Data;
using Barista.Events;
using Barista.State;

[assembly: InternalsVisibleTo("Barista.Core.Tests")]
namespace Barista
{
    public class BaristaApp : IDisposable
    {
        public IReduxStore<BaristaPluginState> Store { get; }
        private readonly IDisposable _stopScheduler;
        private readonly IDisposable _watcherDisposer;

        public BaristaApp(IReduxStore<BaristaPluginState> store, IFileProvider fileProvider)
        {
            Store = store;
            _stopScheduler = ScheduleExecution();
            _watcherDisposer = fileProvider.Watch(() => Store.Dispatch(new SynchronizePlugins()));
        }

        private IDisposable ScheduleExecution()
        {
            return Store
                .Select(state => state.Plugins)
                .Select(plugins => plugins.Where(p => !p.Disabled))
                .Scan(
                    new
                    { Previous = new List<Plugin>().ToImmutableList(), Current = new List<Plugin>().ToImmutableList() },
                    (acc, cur) => new
                    {
                        Previous = acc.Current,
                        Current = cur.ToImmutableList(),
                    }
                )
                .Subscribe(state =>
                {
                    var addSchedules = state.Current.Except(state.Previous).ToImmutableList();
                    var removeSchedules = state.Previous.Except(state.Current).ToImmutableList();

                    System.Diagnostics.Debug.WriteLine($"Adding {addSchedules.Count} schedules");
                    System.Diagnostics.Debug.WriteLine($"Removing {removeSchedules.Count} schedules");

                    try
                    {
                        Store.Dispatch(new SchedulePlugins
                        {
                            Add = addSchedules.ToImmutableList(),
                            Remove = removeSchedules.ToImmutableList(),
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                });
        }

        public void Start()
        {
            Store.Dispatch(new SynchronizePlugins());
        }

        public void ExecuteAction(Item item)
        {
            Store.Dispatch(new ExecuteAction
            {
                Item = item
            });
        }

        public void ExecuteAllPlugins()
        {
            Store
                .Select(state => state.Plugins)
                .Take(1)
                .Subscribe(plugins =>
                {
                    foreach (var plugin in plugins)
                    {
                        Store.Dispatch(new ExecutePlugin
                        {
                            Name = plugin.Name,
                            FilePath = plugin.FilePath
                        });
                    }
                });
        }

        public void Dispose()
        {
            _stopScheduler.Dispose();
            Store.Dispose();
        }
    }
}
