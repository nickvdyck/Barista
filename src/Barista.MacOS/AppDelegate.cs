using AppKit;
using Barista.Core;
using Barista.Core.FileSystem;
using Barista.IOC;
using Barista.MacOS.Services;
using Barista.MacOS.Utils;
using Barista.MacOS.ViewModels;
using Barista.MacOS.Views.Preferences;
using Barista.MacOS.Views.StatusBar;
using Foundation;
using TinyIoC;

namespace Barista.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private readonly TinyIoCContainer _container;

        public AppDelegate()
        {
            var container = TinyIoCContainer.Current;

            container.AddPluginManager(options =>
            {
                var settings = container.Resolve<ISettingsService>();
                var appsettings = settings.GetSettings();
                options.Directory = appsettings.PluginDirectory;
            });

            // Views
            container.Register<BaristaStatusBar>().AsMultiInstance();
            container.Register<GeneralViewController>().AsMultiInstance();
            container.Register<PluginViewController>().AsMultiInstance();
            container.Register<PreferencesWindowFactory>().AsMultiInstance();

            // ViewModels
            container.Register<GeneralPreferencesViewModel>().AsMultiInstance();
            container.Register<StatusBarViewModel>().AsMultiInstance();

            // Services
            container.Register<ISettingsService, DefaultsService>().AsSingleton();
            container.Register<IFileSystemWatcher>((provider, _) =>
            {
                var settings = provider.Resolve<ISettingsService>();
                return new MacFileSystemWatcher(settings.GetSettings().PluginDirectory);
            });

            _container = container;
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            var pluginManager = _container.Resolve<IPluginManager>();
            var statusBar = _container.Resolve<BaristaStatusBar>();

            pluginManager.Start();
            statusBar.Show();
        }

        public override void WillTerminate(NSNotification notification)
        {
            var pluginManager = _container.Resolve<IPluginManager>();
            pluginManager.Stop();
            pluginManager.Dispose();
        }
    }
}
