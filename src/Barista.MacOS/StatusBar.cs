using System;
using AppKit;

namespace Barista.MacOS
{
    public class StatusBar
    {
        private readonly PluginManager _pluginManager;

        public StatusBar(PluginManager pluginManager)
        {
            _pluginManager = pluginManager;
        }

        public void Draw()
        {
            var preferences = new NSMenuItem("Preferences")
            {
                Title = "Preferences",
                Submenu = new NSMenu(),
            };

            var refreshAll = new NSMenuItem("RefreshAll")
            {
                Title = "Refresh All",
            };

            refreshAll.Activated += (sender, e) => _pluginManager.RunAll();

            var quit = new NSMenuItem("Quit")
            {
                Title = "Quit",
            };

            quit.Activated += (sender, e) => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);

            preferences.Submenu.AddItem(refreshAll);
            preferences.Submenu.AddItem(NSMenuItem.SeparatorItem);
            preferences.Submenu.AddItem(quit);

            foreach (var plugin in _pluginManager.GetPlugins())
            {
                var item = new StatusItem(_pluginManager, plugin, (NSMenuItem)preferences.Copy());

                item.Draw();
            }
        }
    }
}
