using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Barista.Common.Redux;
using Barista.MacOS.Selectors;
using Barista.MacOS.Utils;
using Barista.State;
using static Barista.MacOS.Selectors.SelectPluginsWithExecutionResult;

namespace Barista.MacOS.Views.StatusBar
{
    public class BaristaStatusBar : IDisposable
    {
        private readonly Dictionary<string, BaristaStatusBarItem> _items = new Dictionary<string, BaristaStatusBarItem>();

        private readonly IDisposable _pluginDataSubscription;

        private readonly ServiceProvider _provider;

        public BaristaStatusBar(BaristaApp app, ServiceProvider provider)
        {
            _provider = provider;

            _pluginDataSubscription = app.Store
                .SelectPlugins()
                .Scan(
                    new
                    {
                        Previous = new List<PluginViewModel>().ToImmutableList(),
                        Current = new List<PluginViewModel>().ToImmutableList()
                    },
                    (acc, current) => new
                    {
                        Previous = acc.Current,
                        Current = current.ToImmutableList(),
                    }
                )
                .Select(data =>
                {
                    var removed = data.Previous.Except(data.Current);
                    var added = data.Current.Except(data.Previous);
                    var updated = data.Current.Except(added);

                    return new
                    {
                        Added = added.ToImmutableList(),
                        Removed = removed.ToImmutableList(),
                        Updated = updated.ToImmutableList(),
                    };
                })
                .Subscribe(data =>
                {
                    OnUpdateItems(data.Added, data.Updated, data.Removed);
                },
                () =>
                {
                    System.Diagnostics.Debug.WriteLine("Barista select plugins subscription completed");
                });
        }

        public void OnUpdateItems(ImmutableList<PluginViewModel> added, ImmutableList<PluginViewModel> updated, ImmutableList<PluginViewModel> removed)
        {
            foreach (var plugin in added)
            {
                var item = _provider.GetInstance<BaristaStatusBarItem>();
                item.OnUpdate(plugin);
                _items.Add(plugin.PluginName, item);
            }

            foreach (var plugin in updated)
            {
                if (_items.TryGetValue(plugin.PluginName, out var item))
                {
                    item.OnUpdate(plugin);
                }
            }

            foreach (var plugin in removed)
            {
                if (_items.Remove(plugin.PluginName, out var item))
                {
                    item.Dispose();
                }
            }
        }

        public void Dispose()
        {
            _pluginDataSubscription.Dispose();
            foreach (var item in _items)
            {
                item.Value.Dispose();
                _items.Remove(item.Key);
            }
        }
    }
}
