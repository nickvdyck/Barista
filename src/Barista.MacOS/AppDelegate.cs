using AppKit;
using Barista.Extensions;
using Barista.Common.FileSystem;
using Barista.MacOS.Services;
using Barista.MacOS.Utils;
using Barista.MacOS.ViewModels;
using Barista.MacOS.Views.Preferences;
using Barista.MacOS.Views.StatusBar;
using Foundation;
using DryIoc;
using Barista.Common;
using Barista.Data.Events;

namespace Barista.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private readonly Container _container;

        public AppDelegate()
        {
            var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());

            container.AddPluginManager(options =>
            {
                var settings = container.Resolve<ISettingsService>();
                var appsettings = settings.GetSettings();
                options.Directory = appsettings.PluginDirectory;
            });

            // Handlers
            container.RegisterDelegate<IEventHandler<PluginExecutedEvent>>(
                resolver => resolver.Resolve<StatusBarViewModel>()
            );
            container.RegisterDelegate<IEventHandler<PluginsUpdatedEvent>>(
                resolver => resolver.Resolve<StatusBarViewModel>()
            );

             // Views
            container.Register<BaristaStatusBar>();
            container.Register<GeneralViewController>(made: Made.Of(() => new GeneralViewController(Arg.Of<GeneralPreferencesViewModel>())));
            container.Register<PluginViewController>(made: Made.Of(() => new PluginViewController()));
            container.Register<PreferencesWindowFactory>();

            // ViewModels
            container.Register<GeneralPreferencesViewModel>(Reuse.Singleton);
            container.Register<StatusBarViewModel>(Reuse.Singleton);

            // Services
            container.Register<ISettingsService, DefaultsService>(Reuse.Singleton);
            container.RegisterDelegate<IFileSystemWatcher>((provider) =>
            {
                var settings = provider.Resolve<ISettingsService>();
                return new MacFileSystemWatcher(settings.GetSettings().PluginDirectory);
            });

            _container = container;
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            var pluginManager = _container.Resolve<IPluginManager>();
            pluginManager.Start();

            _container.Resolve<BaristaStatusBar>();
        }

        public override void WillTerminate(NSNotification notification)
        {
            var pluginManager = _container.Resolve<IPluginManager>();
            pluginManager.Stop();
            pluginManager.Dispose();
        }
    }
}
