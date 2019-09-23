using System;
using System.Collections.Generic;
using AppKit;
using Barista.Data;
using Barista.MacOS.Utils;
using Barista.MacOS.Views.Preferences;
using Foundation;
using static Barista.MacOS.Selectors.SelectPluginsWithExecutionResult;

namespace Barista.MacOS.Views.StatusBar
{
    public class BaristaStatusBarItem : StatusBarItem
    {
        public PluginViewModel ViewModel { get; private set; }

        private readonly BaristaApp _app;

        private readonly PreferencesWindowFactory _preferencesWindowFactory;

        public BaristaStatusBarItem(BaristaApp app, PreferencesWindowFactory preferencesWindowFactory)
        {
            _app = app;
            _preferencesWindowFactory = preferencesWindowFactory;
        }

        private void UpdateTitle(string title, string color)
        {

            InvokeOnMainThread(() =>
            {
                var nsTitle = new NSMutableAttributedString(title);

                if (!string.IsNullOrEmpty(color))
                {
                    nsTitle.AddAttribute(
                        NSStringAttributeKey.ForegroundColor,
                        ColorUtil.FromWebString(color),
                        new NSRange(0, title.Length)
                    );
                }

                AttributedTitle = nsTitle;
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

                if (item.Type != ItemType.Empty)
                {
                    nsMenuItem.Activated += (sender, e) => OnStatusItemClicked(item);
                }

                if (item.Children.Count > 0)
                {
                    nsMenuItem.Submenu = new NSMenu(item.Title);
                    AddItemsToMenu(item.Children, nsMenuItem.Submenu);
                }

                menu.AddItem(nsMenuItem);
            }

            menu.AddItem(NSMenuItem.SeparatorItem);
        }

        private void OnStatusItemClicked(Item item) =>
            _app.ExecuteAction(item);

        private void OnRefreshAll() => _app.ExecuteAllPlugins();

        private void OnOpenPreferences()
        {
            var controller = _preferencesWindowFactory.Create();
            controller.Show();

            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            controller.Window.MakeKeyAndOrderFront(this);
        }

        private void OnExit() => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);

        public override void OnMenuWillOpen(NSMenu _)
        {
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
                OnRefreshAllClicked = OnRefreshAll,
                OnPreferencesClicked = OnOpenPreferences,
                OnQuitClicked = OnExit
            };

            Menu.AddItem(baristaSubMenu);
        }

        public override void OnMenuDidClose(NSMenu _)
        {
            Menu.RemoveAllItems();
        }

        public void OnUpdate(PluginViewModel viewModel)
        {
            ViewModel = viewModel;
            UpdateTitle(viewModel.TitleAndIcon, viewModel.Colour);
        }
    }
}
