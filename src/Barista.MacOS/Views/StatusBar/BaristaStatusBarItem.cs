using System;
using System.Collections.Generic;
using AppKit;
using Barista.Core.Data;
using Barista.MacOS.Utils;
using Barista.MacOS.ViewModels;
using Foundation;

namespace Barista.MacOS.Views.StatusBar
{
    public class BaristaStatusBarItem : StatusBarItem
    {
        private readonly IDisposable _titleObserver;
        public StatusItemViewModel ViewModel { get; private set; }
        public BaristaStatusBarItem(StatusItemViewModel viewModel) : base()
        {
            ViewModel = viewModel;
            _titleObserver = ViewModel.AddObserver("IconAndTitle", NSKeyValueObservingOptions.New, OnTitleChanged);

            Title = ViewModel.IconAndTitle;
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

                AttributedTitle = title;
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

        public override void OnMenuWillOpen(NSMenu _)
        {
            var menuItems = new List<NSMenuItem>();

            foreach (var itemCollection in ViewModel.Items)
            {
                AddItemsToMenu(itemCollection, Menu);
            }

            Menu.AddItem(new NSMenuItem("LastUpdated")
            {
                Title = ViewModel.ExecutedTimeAgo,
            });

            var baristaSubMenu = new BaristaSubmenuView()
            {
                OnRefreshAllClicked = ViewModel.OnRefreshAll,
                OnPreferencesClicked = ViewModel.OnOpenPreferences,
                OnQuitClicked = ViewModel.OnExit
            };

            Menu.AddItem(baristaSubMenu);
        }

        public override void OnMenuDidClose(NSMenu _)
        {
            Menu.RemoveAllItems();
        }

        public override void Dispose()
        {
            System.Diagnostics.Debug.WriteLine($"Disposing menu item for {ViewModel.Plugin.Name}");
            _titleObserver.Dispose();
            ViewModel.Dispose();
            base.Dispose();
        }
    }
}