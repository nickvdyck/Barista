using AppKit;
using Barista.Common.FileSystem;
using Barista.State;
using Barista.Views;
using Barista.Views.Preferences;
using Foundation;

namespace Barista
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private readonly App _app;
        private readonly StatusBar _statusBar;
        private readonly PreferencesWindowFactory _preferencesWindowFactory;

        public AppDelegate()
        {
            var path = NSUserDefaults.StandardUserDefaults.StringForKey("pluginDirectory");
            var _fileProvider = new LocalFileProvider(path);

            _app = new App(_fileProvider);
            _preferencesWindowFactory = new PreferencesWindowFactory();
            _statusBar = new StatusBar(_app, _preferencesWindowFactory);
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            _app.Start();
        }

        public override void WillTerminate(NSNotification notification)
        {
            _statusBar.Dispose();
            _app.Stop();
        }
    }
}
