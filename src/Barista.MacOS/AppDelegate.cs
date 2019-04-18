using System.Linq;
using System.Text.RegularExpressions;
using AppKit;
using Barista.Core.FileSystem;
using Foundation;

namespace Barista.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {

        public override void DidFinishLaunching(NSNotification notification)
        {
            var provider = new LocalFileProvider(Settings.GetPluginDirectory());
            var _plugins = new PluginManager(provider);
            _plugins.Load();

            // Create a status bar menu
            NSStatusBar statusBar = NSStatusBar.SystemStatusBar;

            var preferences = new NSMenuItem("Preferences")
            {
                Title = "Preferences",
                Submenu = new NSMenu(),
            };

            var refreshAll = new NSMenuItem("RefreshAll")
            {
                Title = "Refresh All"
            };

            refreshAll.Activated += (sender, e) => _plugins.RunAll();

            var quit = new NSMenuItem("Quit")
            {
                Title = "Quit"
            };

            quit.Activated += (sender, e) => NSApplication.SharedApplication.Terminate(NSApplication.SharedApplication);

            preferences.Submenu.AddItem(refreshAll);
            preferences.Submenu.AddItem(NSMenuItem.SeparatorItem);
            preferences.Submenu.AddItem(quit);

            foreach (var plugin in _plugins.GetPlugins())
            {
                var pluginItem = statusBar.CreateStatusItem(NSStatusItemLength.Variable);

                pluginItem.Button.Title = "...";
                pluginItem.Menu = new NSMenu(plugin.Name);
                pluginItem.Menu.AddItem((NSMenuItem)preferences.Copy());

                plugin.OnExecuted = (string output) =>
                {
                    InvokeOnMainThread(() => pluginItem.Button.Title = output);
                };

            }

            _plugins.Start();

            //var data = await ProcessExecutor.Run("/Users/nickvd/Plugins/BitBar/cpu-load.5s.sh");

            //var title = data.Split("---").FirstOrDefault();

            //title = Regex.Replace(title, @"\t|\n|\r", "");

            //var item = statusBar.CreateStatusItem(NSStatusItemLength.Variable);

            //item.Title = title;
            //item.HighlightMode = true;
            //item.Menu = new NSMenu(title);
            //item.Menu.AddItem(preferences);

            //var timer = new System.Timers.Timer(2000) // two seconds
            //{
            //    AutoReset = true,
            //    Enabled = true
            //};

            //timer.Elapsed += (sender, e) =>
            //{
            //    data = ProcessExecutor.Run("/Users/nickvd/Plugins/BitBar/cpu-load.5s.sh");

            //    title = data.Split("---").FirstOrDefault();

            //    title = Regex.Replace(title, @"\t|\n|\r", "");
            //    System.Diagnostics.Debug.WriteLine(title);

            //    InvokeOnMainThread(() => item.Button.Title = title);
            //};
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
