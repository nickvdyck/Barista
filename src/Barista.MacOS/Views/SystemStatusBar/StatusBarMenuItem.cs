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
        private readonly NSStatusItem _mainMenu;
        private readonly StatusItemViewModel ViewModel;

        private readonly IDisposable _titleObserver;

        public StatusBarMenuItem(StatusItemViewModel viewModel)
        {
            ViewModel = viewModel;
            _statusBar = NSStatusBar.SystemStatusBar;
            _mainMenu = _statusBar.CreateStatusItem(NSStatusItemLength.Variable);

            _titleObserver = ViewModel.AddObserver("IconAndTitle", NSKeyValueObservingOptions.New, OnTitleChanged);

            //_mainMenu.Button.Bind((NSString)"Title", ViewModel, "IconAndTitle", null);
            System.Diagnostics.Debug.WriteLine($"Adding menu item for {viewModel.Plugin.Name}");
        }

        private void OnTitleChanged(NSObservedChange change)
        {
            InvokeOnMainThread(() => _mainMenu.Button.Title = ViewModel.IconAndTitle);
        }

        private void OnMenuWillOpen(NSMenu _)
        {
            // _lastUpdated.Title = $"Updated {TimeAgo.Format(_lastExecution)}";
            var menuItems = new List<NSMenuItem>();

            foreach (var itemCollection in ViewModel.Items)
            {
                foreach (var item in itemCollection)
                {
                    var title = item.Title;
                    if (item.Title.Length > item.Length)
                        title = item.Title.Substring(0, item.Length) + " ...";

                    var nsMenuItem = new NSMenuItem(item.Title)
                    {
                        Title = title,
                    };

                    if (item.Type != ItemType.Empty) nsMenuItem.Activated += (sender, e) => ViewModel.OnStatusItemClicked(item);

                    _mainMenu.Menu.AddItem(nsMenuItem);

                }
                _mainMenu.Menu.AddItem(NSMenuItem.SeparatorItem);
            }

            // _mainMenu.Menu.AddItem(_lastUpdated);

            var baristaSubMenu = new BaristaSubmenuView()
            {
                OnRefreshAllClicked = ViewModel.OnRefreshAll,
                OnPreferencesClicked = ViewModel.OnOpenPreferences,
                OnQuitClicked = ViewModel.OnExit
            };
            _mainMenu.Menu.AddItem(baristaSubMenu);
        }

        private void OnMenuDidClose(NSMenu _)
        {
            _mainMenu.Menu.RemoveAllItems();
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

            _mainMenu.Button.Title = ViewModel.IconAndTitle;

            _mainMenu.Menu = new NSMenu()
            {
                Delegate = events,
            };
        }

        public new void Dispose()
        {
            System.Diagnostics.Debug.WriteLine($"Disposing menu item for {ViewModel.Plugin.Name}");
            _titleObserver.Dispose();
            base.Dispose();
        }
    }
}
