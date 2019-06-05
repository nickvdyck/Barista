using System;
using AppKit;
using Foundation;

namespace Barista.MacOS.Views.StatusBar
{
    public abstract class StatusBarItem : NSObject
    {
        private readonly NSStatusItem _statusItem;

        class StatusBarItemMenuDelegate : NSMenuDelegate
        {
            public Action<NSMenu> OnMenuWillOpen { get; set; }
            public Action<NSMenu> OnMenuDidClose { get; set; }
            public Action<NSMenu, NSMenuItem> OnMenuWillHighlight { get; set; }

            public StatusBarItemMenuDelegate(Action<NSMenu> onMenuWillOpen, Action<NSMenu> onMenuDidClose)
            {
                OnMenuWillOpen = onMenuWillOpen;
                OnMenuDidClose = onMenuDidClose;
            }

            public override void MenuWillOpen(NSMenu menu) =>
                OnMenuWillOpen?.Invoke(menu);

            public override void MenuDidClose(NSMenu menu) =>
                OnMenuDidClose?.Invoke(menu);

            public override void MenuWillHighlightItem(NSMenu menu, NSMenuItem item) =>
                OnMenuWillHighlight?.Invoke(menu, item);
        }

        public StatusBarItem()
        {
            _statusItem = NSStatusBar.SystemStatusBar.CreateStatusItem(NSStatusItemLength.Variable);
            _statusItem.Menu = new NSMenu
            {
                Delegate = new StatusBarItemMenuDelegate(OnMenuWillOpen, OnMenuDidClose)
            };
        }

        public string Title
        {
            get => _statusItem.Button.Title;
            set => _statusItem.Button.Title = value;
        }

        public NSAttributedString AttributedTitle
        {
            get => _statusItem.Button.AttributedTitle;
            set => _statusItem.Button.AttributedTitle = value;
        }

        public NSMenu Menu
        {
            get => _statusItem.Menu;
        }

        public virtual void OnMenuWillOpen(NSMenu menu) { }

        public virtual void OnMenuDidClose(NSMenu menu) { }

        public new virtual void Dispose()
        {
            _statusItem.Menu.Delegate = null;
            _statusItem.Menu = null;
            NSStatusBar.SystemStatusBar.RemoveStatusItem(_statusItem);
        }
    }
}
