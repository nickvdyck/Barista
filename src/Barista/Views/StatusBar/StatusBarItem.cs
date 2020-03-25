using AppKit;
using Barista.Common;
using Barista.Domain;
using Barista.State;
using Barista.Views.Preferences;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Barista.Views
{
    public class StatusBarItem : BaseStatusBarItem, IDisposable
    {
        private readonly PreferencesWindowFactory _preferencesWindowFactory;
        private IDisposable _subscription;

        public Plugin Plugin { get; private set; }
        private PluginExecutionResult _lastExecutionResult { get; set; }

        public StatusBarItem(Plugin plugin, PreferencesWindowFactory preferencesWindowFactory) : base()
        {
            Title = "...";

            Plugin = plugin;
            _preferencesWindowFactory = preferencesWindowFactory;
            _subscription = plugin.Subscribe(OnUpdate);
        }

        public void Update(Plugin plugin)
        {
            Plugin = plugin;
            _subscription.Dispose();
            _subscription = plugin.Subscribe(OnUpdate);
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

                Title = string.Empty;
                AttributedTitle = nsTitle;
            });
        }

        private void OnUpdate(PluginExecutionResult result)
        {
            System.Diagnostics.Debug.WriteLine($"Updating result for {Plugin.Metadata.Name}");
            if (result.Success)
            {
                var titleRecord = result.Items.FirstOrDefault().FirstOrDefault();
                UpdateTitle(titleRecord.Title, titleRecord.Color);
            }
            else
            {
                UpdateTitle("⚠️", string.Empty);
            }

            _lastExecutionResult = result;
        }

        private void OnRefresh() => Plugin.Execute();

        private void OnRefreshAll()
        { }

        private void OnOpenPreferences()
        {
            var controller = _preferencesWindowFactory.Create();
            controller.Show();

            NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
            controller.Window.MakeKeyAndOrderFront(this);
        }

        private void OnExit() => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);

        private void OnStatusItemClicked(Item item)
        {
            switch (item.Type)
            {
                case ItemType.RunScriptAction:
                case ItemType.RunScriptInTerminalAction:
                    ProcessHelpers.ExecuteCommand(item.BashScript, item.Params, item.Terminal);
                    break;

                case ItemType.Link:
                    ProcessHelpers.ExecuteCommand("open", new string[] { item.Href });
                    break;

                case ItemType.RefreshAction:
                    Plugin.Execute();
                    break;

                default:
                    System.Diagnostics.Debug.WriteLine($"No action registered for item {item.Title} with type {item.Type}.");
                    break;
            }
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

        public override void OnMenuWillOpen(NSMenu _)
        {
            if (_lastExecutionResult != null && _lastExecutionResult.Success)
            {
                if (_lastExecutionResult.Success)
                {
                    foreach (var record in _lastExecutionResult.Items.Skip(1))
                    {
                        AddItemsToMenu(record, Menu);
                    }
                }

                Menu.AddItem(new NSMenuItem("LastUpdated")
                {
                    Title = $"Updated {TimeAgo.Format(_lastExecutionResult.LastExecution.ToLocalTime())}",
                });
            }
            else
            {
                Menu.AddItem(new NSMenuItem("Execution")
                {
                    Title =  Plugin.IsExecuting ? "Still Executing" : "Finished Executing",
                });
            }

            Menu.AddItem(new DefaultSubMenuSection
            {
                OnRefreshClicked = OnRefresh,
                OnRefreshAllClicked = OnRefreshAll,
                OnPreferencesClicked = OnOpenPreferences,
                OnQuitClicked = OnExit
            });
        }

        public override void OnMenuDidClose(NSMenu _)
        {
            Menu.RemoveAllItems();
        }

        public override void Dispose()
        {
            base.Dispose();
            _subscription.Dispose();
        }
    }
}
