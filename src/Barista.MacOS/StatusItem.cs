using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Foundation;

namespace Barista.MacOS
{
    public class StatusItem : NSObject
    {
        private readonly NSStatusBar _statusBar;
        private readonly Plugin _plugin;
        private readonly PluginManager _pluginManager;
        private readonly NSMenuItem _lastUpdated;
        private readonly NSMenuItem _preferences;

        private NSStatusItem _mainMenu;
        private List<NSMenuItem> _menuItems = new List<NSMenuItem>();
        private bool _isOpen = false;

        public StatusItem(PluginManager manger, Plugin plugin, NSMenuItem preferences)
        {
            _pluginManager = manger;
            _plugin = plugin;
            _preferences = preferences;

            _statusBar = NSStatusBar.SystemStatusBar;
            _lastUpdated = new NSMenuItem("LastUpdated")
            {
                Title = $"Updated {TimeAgoUtils.TimeAgo(DateTime.Now)}",
            };
        }

        private void OnPluginExecuted(IReadOnlyCollection<PluginRecord> result)
        {
            var first = result.FirstOrDefault();
            InvokeOnMainThread(() =>
            {
                _mainMenu.Button.Title = first.Title;
                _lastUpdated.Title = $"Updated {TimeAgoUtils.TimeAgo(_plugin.LastExecution)}";
            });

            if (_isOpen) return;

            _menuItems.Clear();

            foreach (var record in result.Skip(1))
            {
                if (record == PluginRecord.Seperator)
                {
                    _menuItems.Add(NSMenuItem.SeparatorItem);
                }
                else
                {
                    var title = record.Title;
                    if (record.Title.Length > record.Length)
                        title = record.Title.Substring(0, record.Length) + " ...";


                    var menuItem = new NSMenuItem(record.Title)
                    {
                        Title = title,
                    };

                    if (record.Refresh || record.BashScript != string.Empty || record.Href != string.Empty)
                    {
                        menuItem.Activated += (sender, e) => OnMenuItemClicked(menuItem, record);
                    }

                    _menuItems.Add(menuItem);
                }
            }
        }

        private void OnMenuWillOpen(NSMenu _)
        {
            _lastUpdated.Title = $"Updated {TimeAgoUtils.TimeAgo(_plugin.LastExecution)}";

            foreach (var item in _menuItems)
            {
                _mainMenu.Menu.AddItem(item);
            }

            _mainMenu.Menu.AddItem(NSMenuItem.SeparatorItem);
            _mainMenu.Menu.AddItem(_lastUpdated);
            _mainMenu.Menu.AddItem(_preferences);

            _isOpen = true;
        }

        private void OnMenuDidClose(NSMenu _)
        {
            _mainMenu.Menu.RemoveAllItems();
            _isOpen = false;
        }

        private void OnMenuItemClicked(NSMenuItem menuItem, PluginRecord record)
        {
            if (record.Refresh)
            {
                _pluginManager.Run(_plugin);
            }

            if (record.BashScript != string.Empty)
            {
                var _ = Task.Factory.StartNew(async () =>
                {
                    await ProcessExecutor.Run(record.BashScript, record.Params);
                });

            }

            if (record.Href != string.Empty)
            {
                var _ = Task.Factory.StartNew(async () =>
                {
                    await ProcessExecutor.Run("open", new string[] { record.Href });
                });
            }
        }

        public void Draw()
        {
            var events = new StatusItemEvents
            {
                OnMenuWillOpen = OnMenuWillOpen,
                OnMenuDidClose = OnMenuDidClose
            };

            _mainMenu = _statusBar.CreateStatusItem(NSStatusItemLength.Variable);

            _mainMenu.Button.Title = "...";

            _mainMenu.Menu = new NSMenu(_plugin.Name)
            {
                Delegate = events,
            };

            _plugin.OnExecuted = OnPluginExecuted;
        }
    }
}
