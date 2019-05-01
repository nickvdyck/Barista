using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AppKit;
using Barista.Core;
using Barista.Core.Data;
using Barista.Core.Extensions;
using Foundation;

namespace Barista.MacOS
{
    public class StatusItem : NSObject
    {
        private readonly NSStatusBar _statusBar;
        private readonly NSMenuItem _lastUpdated;
        private readonly NSMenuItem _preferences;

        private NSStatusItem _mainMenu;
        private List<NSMenuItem> _menuItems = new List<NSMenuItem>();
        private bool _isOpen;

        private DateTime _lastExecution = DateTime.Now;

        private readonly Action<IPluginMenuItem> OnMenuItemClicked;

        public StatusItem(IObservable<IReadOnlyCollection<IPluginMenuItem>> observable, Action<IPluginMenuItem> onMenuItemClicked, NSMenuItem preferences)
        {
            OnMenuItemClicked = onMenuItemClicked;
            _preferences = preferences;

            _statusBar = NSStatusBar.SystemStatusBar;
            _lastUpdated = new NSMenuItem("LastUpdated")
            {
                Title = $"Updated {TimeAgoUtils.TimeAgo(_lastExecution)}",
            };

            observable.Subscribe(OnNextPluginExecution);
        }

        private void OnNextPluginExecution(IReadOnlyCollection<IPluginMenuItem> result)
        {
            _lastExecution = DateTime.Now;
            var first = result.FirstOrDefault();
            InvokeOnMainThread(() =>
            {
                _mainMenu.Button.Title = first.Title;
                _lastUpdated.Title = $"Updated {TimeAgoUtils.TimeAgo(_lastExecution)}";
            });

            if (_isOpen) return;

            _menuItems.Clear();

            foreach (var item in result.Skip(1))
            {
                if (item == PluginMenuItemBase.Separator)
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

                    if (item.IsCommand) nsMenuItem.Activated += (sender, e) => OnMenuItemClicked(item);

                    _menuItems.Add(nsMenuItem);
                }
            }
        }

        private void OnMenuWillOpen(NSMenu _)
        {
            _lastUpdated.Title = $"Updated {TimeAgoUtils.TimeAgo(_lastExecution)}";

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

            _mainMenu.Menu = new NSMenu()
            {
                Delegate = events,
            };
        }
    }
}
