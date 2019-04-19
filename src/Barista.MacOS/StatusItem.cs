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
        private bool _isInitialized = false;

        public StatusItem(Plugin plugin, NSMenuItem preferences)
        {
            _plugin = plugin;
            _preferences = preferences;
            _statusBar = NSStatusBar.SystemStatusBar;
            _lastUpdated = new NSMenuItem("LastUpdated")
            {
                Title = $"Updated {TimeAgoUtils.TimeAgo(_plugin.LastExecution)}",
            };
        }

        private void OnPluginExecuted(IReadOnlyList<IReadOnlyList<string>> items)
        {

            InvokeOnMainThread(() =>
            {
                _mainMenu.Button.Title = items.FirstOrDefault().FirstOrDefault();

                if (_isInitialized == false)
                {
                    System.Diagnostics.Debug.WriteLine("first time");

                    foreach (var section in items.Skip(1).Reverse())
                    {
                        foreach (var item in section)
                        {
                            var menuItem =new NSMenuItem(item)
                            {
                                Title = item,
                            };

                            _mainMenu.Menu.InsertItem(menuItem, 0);
                        }
                    }
                    _isInitialized = true;
                }
            });

        }

        private void OnMenuWillOpen(NSMenu _)
        {
            var updatedMsg = $"Updated {TimeAgoUtils.TimeAgo(_plugin.LastExecution)}";

            if (_lastUpdated?.Title != updatedMsg)
            {
                _lastUpdated.Title = updatedMsg;
            }
        }

        public void Draw()
        {
            var events = new StatusItemMenuEvents
            {
                OnMenuWillOpen = OnMenuWillOpen,
            };

            _mainMenu = _statusBar.CreateStatusItem(NSStatusItemLength.Variable);


            _mainMenu.Button.Title = "...";

            _mainMenu.Menu = new NSMenu(_plugin.Name)
            {
                Delegate = events,
            };

            _mainMenu.Menu.AddItem(NSMenuItem.SeparatorItem);
            _mainMenu.Menu.AddItem(_lastUpdated);
            _mainMenu.Menu.AddItem(_preferences);

            _plugin.OnExecuted = OnPluginExecuted;
        }
    }

    class StatusItemMenuEvents : NSMenuDelegate
    {

        public Action<NSMenu> OnMenuWillOpen { get; set; }

        public override void MenuWillOpen(NSMenu menu)
        {
            System.Diagnostics.Debug.WriteLine("I am going to open a menu");
            OnMenuWillOpen?.Invoke(menu);
        }

        public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item)
        {
            System.Diagnostics.Debug.WriteLine("I am going to highlight a menu");
        }
    }
}
