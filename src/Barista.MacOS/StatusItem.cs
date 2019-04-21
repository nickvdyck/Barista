using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Foundation;

namespace Barista.MacOS
{
    public class StatusItem : NSObject
    {
        private readonly NSStatusBar _statusBar;
        private readonly Plugin _plugin;
        private readonly NSMenuItem _lastUpdated;
        private readonly NSMenuItem _preferences;

        private NSStatusItem _mainMenu;
        private List<NSMenuItem> _menuItems = new List<NSMenuItem>();
        private bool _isOpen = false;

        public StatusItem(Plugin plugin, NSMenuItem preferences)
        {
            _plugin = plugin;
            _preferences = preferences;
            _statusBar = NSStatusBar.SystemStatusBar;
            _lastUpdated = new NSMenuItem("LastUpdated")
            {
                Title = $"Updated {TimeAgoUtils.TimeAgo(DateTime.Now)}",
            };
        }

        private void OnPluginExecuted(IReadOnlyList<IReadOnlyList<string>> items)
        {
            InvokeOnMainThread(() =>
            {
                _mainMenu.Button.Title = items.FirstOrDefault().FirstOrDefault();
                _lastUpdated.Title = $"Updated {TimeAgoUtils.TimeAgo(_plugin.LastExecution)}";
            });

            if (_isOpen) return;

            _menuItems.Clear();

            foreach (var section in items.Skip(1))
            {
                foreach (var item in section)
                {
                    var menuItem = new NSMenuItem(item)
                    {
                        Title = item,
                    };
                    _menuItems.Add(menuItem);
                }

                _menuItems.Add(NSMenuItem.SeparatorItem);
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
