using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Barista.Core.Data;
using Barista.Core;
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


            _pluginManager.Monitor(plugin, OnPluginExecuted);
        }

        private void OnPluginExecuted(IReadOnlyCollection<IPluginMenuItem> result)
        {
            var first = result.FirstOrDefault();
            InvokeOnMainThread(() =>
            {
                _mainMenu.Button.Title = first.Title;
                _lastUpdated.Title = $"Updated {TimeAgoUtils.TimeAgo(_plugin.LastExecution)}";
            });

            if (_isOpen) return;

            _menuItems.Clear();

            foreach (var item in result.Skip(1))
            {
                if (item == PluginMenuItemBase.Seperator)
                {
                    _menuItems.Add(NSMenuItem.SeparatorItem);
                }
                else
                {
                    var title = item.Title;
                    if (item.Title.Length > item.Length)
                        title = item.Title.Substring(0, item.Length) + " ...";


                    var nsMenuItem = new NSMenuItem(item.Title)
                    {
                        Title = title,
                    };

                    if (item.IsCommand) nsMenuItem.Activated += (sender, e) => _pluginManager.InvokeCommand(item.Command);

                    _menuItems.Add(nsMenuItem);
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
        }
    }
}
