using System;
using AppKit;

namespace Barista.MacOS.Views.SystemStatusBar
{
    public static class BaristaSubMenu
    {
        public static void AddBaristaSubMenu(
            this NSMenu menu,
            Action OnRefresshAll,
            Action OnPreferences,
            Action OnQuit
        )
        {
            var baristaMenu = new NSMenuItem("Barista")
            {
                Title = "Barista",
                Submenu = new NSMenu(),
            };

            var refreshAll = new NSMenuItem("RefreshAll")
            {
                Title = "Refresh All",
            };
            refreshAll.Activated += (sender, e) => OnRefresshAll?.Invoke();

            var settings = new NSMenuItem("Preferences")
            {
                Title = "Preferences",
            };
            settings.Activated += (sender, e) => OnPreferences?.Invoke();

            var quit = new NSMenuItem("Quit")
            {
                Title = "Quit",
            };

            quit.Activated += (sender, e) => OnQuit?.Invoke();

            baristaMenu.Submenu.AddItem(refreshAll);
            baristaMenu.Submenu.AddItem(NSMenuItem.SeparatorItem);
            baristaMenu.Submenu.AddItem(settings);
            baristaMenu.Submenu.AddItem(quit);

            menu.AddItem(baristaMenu);

        }
    }
}
