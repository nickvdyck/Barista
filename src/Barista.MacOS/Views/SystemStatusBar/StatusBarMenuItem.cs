using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Barista.Core.Data;
using Barista.Core.Extensions;
using Barista.MacOS.Utils;
using Barista.MacOS.ViewModels;
using Foundation;

namespace Barista.MacOS.Views.SystemStatusBar
{
    public class StatusBarMenuItem : NSObject
    {
        private readonly NSStatusBar _statusBar;
        private readonly NSMenuItem _lastUpdated;

        private NSStatusItem _mainMenu;
        private List<NSMenuItem> _menuItems = new List<NSMenuItem>();
        private bool _isOpen;

        private DateTime _lastExecution = DateTime.Now;

        private readonly IObservable<IReadOnlyCollection<IPluginMenuItem>> _observable;
        private readonly StatusBarViewModel ViewModel;

        public StatusBarMenuItem(IObservable<IReadOnlyCollection<IPluginMenuItem>> observable, StatusBarViewModel viewModel)
        {
            _observable = observable;
            ViewModel = viewModel;
            _statusBar = NSStatusBar.SystemStatusBar;
            _lastUpdated = new NSMenuItem("LastUpdated")
            {
                Title = $"Updated {TimeAgo.Format(_lastExecution)}",
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
                _lastUpdated.Title = $"Updated {TimeAgo.Format(_lastExecution)}";
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

                    if (item.IsCommand) nsMenuItem.Activated += (sender, e) => ViewModel.OnStatusItemClicked(item);

                    _menuItems.Add(nsMenuItem);
                }
            }
        }

        private void OnMenuWillOpen(NSMenu _)
        {
            _lastUpdated.Title = $"Updated {TimeAgo.Format(_lastExecution)}";

            foreach (var item in _menuItems)
            {
                _mainMenu.Menu.AddItem(item);
            }

            _mainMenu.Menu.AddItem(NSMenuItem.SeparatorItem);
            _mainMenu.Menu.AddItem(_lastUpdated);
            _mainMenu.Menu.AddBaristaSubMenu(ViewModel.OnRefreshAll, ViewModel.OnOpenPreferences, ViewModel.OnExit);

            _isOpen = true;
        }

        private void OnMenuDidClose(NSMenu _)
        {
            _mainMenu.Menu.RemoveAllItems();
            _isOpen = false;
        }

        class StatusBarMenuItemEvents : NSMenuDelegate
        {

            public Action<NSMenu> OnMenuWillOpen { get; set; }
            public Action<NSMenu> OnMenuDidClose { get; set; }
            public Action<NSMenu, NSMenuItem> OnMenuWillHighlight { get; set; }

            public override void MenuWillOpen(NSMenu menu)
            {
                OnMenuWillOpen?.Invoke(menu);
            }

            public override void MenuDidClose(NSMenu menu)
            {
                OnMenuDidClose?.Invoke(menu);
            }

            public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item)
            {
                OnMenuWillHighlight?.Invoke(menu, item);
            }
        }
        public void Show()
        {

            var events = new StatusBarMenuItemEvents
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
