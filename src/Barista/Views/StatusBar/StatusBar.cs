using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Barista.Common;
using Barista.State;
using Barista.Views.Preferences;

namespace Barista.Views
{
    public class StatusBar : IDisposable
    {
        private readonly App _app;
        private readonly PreferencesWindowFactory _preferencesWindowFactory;
        private readonly IDisposable _subscription;
        private readonly Dictionary<string, StatusBarItem> _items = new Dictionary<string, StatusBarItem>();

        public StatusBar(App app, PreferencesWindowFactory preferencesWindowFactory)
        {
            _app = app;
            _preferencesWindowFactory = preferencesWindowFactory;

            var comparer = new PluginComparer();
            _subscription = _app
                .Select(plugins => plugins.Where(p => p.IsActive))
                .DistinctUntilChanged()
                .Select(plugins => plugins.ToImmutableList())
                .Subscribe(plugins => OnUpdateMenu(plugins));
        }

        private void OnUpdateMenu(ImmutableList<Plugin> plugins)
        {

            // Add or update status bar items
            foreach (var plugin in plugins)
            {
                if (_items.TryGetValue(plugin.Metadata.Name, out var item))
                {
                    // Guid is diffrent item needs to update
                    if (plugin.Uuid != item.Plugin.Uuid)
                        item.Update(plugin);
                }
                else
                {
                    var statusBarItem = new StatusBarItem(plugin, _preferencesWindowFactory);
                    _items.Add(plugin.Metadata.Name, statusBarItem);
                }
            }

            // Remove items
            var toRemove = _items.Where(i => plugins.FirstOrDefault(p => p.Metadata.Name == i.Key) == null).ToArray();
            foreach (var record in toRemove)
            {
                _items.Remove(record.Key);
                record.Value.Dispose();
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();

            // Improve this
            foreach (var item in _items.Values)
            {
                item.Dispose();
            }

            _items.Clear();
        }
    }
}
