using System;
using AppKit;

namespace Barista.MacOS.Views.SystemStatusBar
{
    public class BaristaSubmenuView : NSMenuItem, IDisposable
    {
        public Action OnRefreshAllClicked { get; set; }
        public Action OnPreferencesClicked { get; set; }
        public Action OnQuitClicked { get; set; }

        private NSMenuItem _refreshAllMenuItem;
        private NSMenuItem _settingsMenuItem;
        private NSMenuItem _quitMenuItem;

        public BaristaSubmenuView() : base("Barista")
        {
            Title = "Barista";
            Submenu = new NSMenu();
            RenderSubMenu();
        }

        private void OnRefreshAllClickedInternal(object sender, EventArgs e) =>
            OnRefreshAllClicked?.Invoke();

        private void OnPreferencesClickedInternal(object sender, EventArgs e) =>
            OnPreferencesClicked?.Invoke();

        private void OnQuitClickedInternal(object sender, EventArgs e) =>
            OnQuitClicked?.Invoke();

        private void RenderSubMenu()
        {
            _refreshAllMenuItem = new NSMenuItem("RefreshAll")
            {
                Title = "Refresh All",
            };
            _refreshAllMenuItem.Activated += OnRefreshAllClickedInternal;

            _settingsMenuItem = new NSMenuItem("Preferences")
            {
                Title = "Preferences",
            };
            _settingsMenuItem.Activated += OnPreferencesClickedInternal;

            _quitMenuItem = new NSMenuItem("Quit")
            {
                Title = "Quit",
            };

            _quitMenuItem.Activated += OnQuitClickedInternal;

            Submenu.AddItem(_refreshAllMenuItem);
            Submenu.AddItem(NSMenuItem.SeparatorItem);
            Submenu.AddItem(_settingsMenuItem);
            Submenu.AddItem(_quitMenuItem);
        }

        public new void Dispose()
        {
            base.Dispose();
            _refreshAllMenuItem.Activated -= OnRefreshAllClickedInternal;
            _settingsMenuItem.Activated -= OnPreferencesClickedInternal;
            _quitMenuItem.Activated -= OnQuitClickedInternal;

            Submenu.Dispose();
            _refreshAllMenuItem.Dispose();
            _settingsMenuItem.Dispose();
            _quitMenuItem.Dispose();
        }
    }
}
