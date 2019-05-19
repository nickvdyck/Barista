using System;
using System.Collections.Generic;
using System.Linq;
using AppKit;
using Barista.Core.Data;
using Barista.MacOS.Utils;
using Barista.MacOS.ViewModels;
using Foundation;

namespace Barista.MacOS.Views.SystemStatusBar
{
    public class StatusBarMenuItem : NSObject
    {
        private readonly NSStatusBar _statusBar;
        private readonly NSStatusItem _mainMenu;
        public StatusItemViewModel ViewModel;

        private readonly IDisposable _titleObserver;

        public StatusBarMenuItem(StatusItemViewModel viewModel)
        {
            ViewModel = viewModel;
            _statusBar = NSStatusBar.SystemStatusBar;
            _mainMenu = _statusBar.CreateStatusItem(NSStatusItemLength.Variable);

            _titleObserver = ViewModel.AddObserver("IconAndTitle", NSKeyValueObservingOptions.New, OnTitleChanged);
        }

        private void OnTitleChanged(NSObservedChange change)
        {
            InvokeOnMainThread(() =>
            {
                var title = new NSMutableAttributedString(ViewModel.IconAndTitle);

                if (!string.IsNullOrEmpty(ViewModel.Color))
                {
                    var color = ColorUtil.FromWebString(ViewModel.Color);
                    title.AddAttribute(NSStringAttributeKey.ForegroundColor, color, new NSRange(0, ViewModel.IconAndTitle.Length));
                }

                _mainMenu.Button.AttributedTitle = title;
            });
        }

        private void AddItemsToMenu(IEnumerable<Item> items, NSMenu menu)
        {
            foreach (var item in items)
            {
                var title = item.Title;

                if (item.Title.Length > item.Length) title = item.Title.Substring(0, item.Length) + " ...";

                var titleAttribs = new NSMutableAttributedString(item.Title);

                if (!string.IsNullOrEmpty(item.Color))
                {
                    var color = ColorUtil.FromWebString(item.Color);
                    titleAttribs.AddAttribute(NSStringAttributeKey.ForegroundColor, color, new NSRange(0, item.Title.Length - 1));
                }

                if (!string.IsNullOrEmpty(item.Font))
                {
                    var font = NSFont.FromFontName(item.Font, 14);
                    titleAttribs.AddAttribute(NSStringAttributeKey.Font, font, new NSRange(0, item.Title.Length - 1));
                }

                var nsMenuItem = new NSMenuItem(item.Title)
                {
                    AttributedTitle = titleAttribs,
                };

                if (item.Type != ItemType.Empty) nsMenuItem.Activated += (sender, e) => ViewModel.OnStatusItemClicked(item);

                if (item.Children.Count > 0)
                {
                    nsMenuItem.Submenu = new NSMenu(item.Title);
                    AddItemsToMenu(item.Children, nsMenuItem.Submenu);
                }

                menu.AddItem(nsMenuItem);
            }

            menu.AddItem(NSMenuItem.SeparatorItem);
        }

        private void OnMenuWillOpen(NSMenu _)
        {
            var menuItems = new List<NSMenuItem>();

            foreach (var itemCollection in ViewModel.Items)
            {
                AddItemsToMenu(itemCollection, _mainMenu.Menu);
            }

            _mainMenu.Menu.AddItem(new NSMenuItem("LastUpdated")
            {
                Title = ViewModel.ExecutedTimeAgo,
            });

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
            _statusBar.RemoveStatusItem(_mainMenu);
            _titleObserver.Dispose();
            ViewModel.Dispose();
            base.Dispose();
        }
    }
}
