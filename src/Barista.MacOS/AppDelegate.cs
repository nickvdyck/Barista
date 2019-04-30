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
            var pluginManger = PluginManager.CreateAtDirectory(Settings.GetPluginDirectory());
            var statusBar = new StatusBar(pluginManger);

            statusBar.Draw();
            pluginManger.Start();
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
