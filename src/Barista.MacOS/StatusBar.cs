using AppKit;
using Barista.Core.Data;
using Barista.MacOS.Views.Preferences;

namespace Barista.MacOS
{
    public class StatusBar
    {
        private readonly PluginManager _pluginManager;
        private readonly PreferencesWindowFactory _preferencesWindowFactory;

        public StatusBar(PluginManager pluginManager, PreferencesWindowFactory preferencesWindowFactory)
        {
            _pluginManager = pluginManager;
            _preferencesWindowFactory = preferencesWindowFactory;
        }

        public void OnMenuItemClicked(IPluginMenuItem item)
        {
            if (item.IsCommand)
            {
                _pluginManager.InvokeCommand(item.Command);
            }
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

            var settings = new NSMenuItem("Settings")
            {
                Title = "Settings",
            };
            settings.Activated += (sender, e) =>
            {
                var controller = _preferencesWindowFactory.Create();
                controller.Show();

                NSApplication.SharedApplication.ActivateIgnoringOtherApps(true);
                controller.Window.MakeKeyAndOrderFront(settings);
            };


            var quit = new NSMenuItem("Quit")
            {
                Title = "Quit",
            };

            quit.Activated += (sender, e) => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);

            preferences.Submenu.AddItem(refreshAll);
            preferences.Submenu.AddItem(NSMenuItem.SeparatorItem);
            preferences.Submenu.AddItem(settings);
            preferences.Submenu.AddItem(quit);

            foreach (var plugin in _pluginManager.GetPlugins())
            {
                var monitor = _pluginManager.Monitor(plugin);
                var item = new StatusItem(monitor, OnMenuItemClicked, (NSMenuItem)preferences.Copy());

                item.Draw();
            }
        }
    }
}
