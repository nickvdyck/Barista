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
            var pluginManger = new PluginManager(provider);
            var statusBar = new StatusBar(pluginManger);

            pluginManger.Load();
            statusBar.Draw();
            pluginManger.Start();
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
