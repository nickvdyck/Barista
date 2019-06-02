using AppKit;
using Autofac;
using Barista.Core;
using Barista.MacOS.Services;
using Barista.MacOS.Utils;
using Barista.MacOS.ViewModels;
using Barista.MacOS.Views.Preferences;
using Barista.MacOS.Views.SystemStatusBar;
using Foundation;

namespace Barista.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private readonly IContainer container;

        public AppDelegate()
        {
            var builder = new ContainerBuilder();

            // Views
            builder.RegisterType<SystemStatusBar>().AsSelf();
            builder.RegisterType<GeneralViewController>().AsSelf();
            builder.RegisterType<PluginViewController>().AsSelf();
            builder.RegisterType<PreferencesWindowFactory>().AsSelf();

            // ViewModels
            builder.RegisterType<GeneralPreferencesViewModel>().AsSelf();
            builder.RegisterType<StatusBarViewModel>().AsSelf();

            // Services
            builder.RegisterType<DefaultsService>().As<ISettingsService>();
            builder
                .Register(s =>
                {
                    var settings = s.Resolve<ISettingsService>().GetSettings();
                    var watcher = new FileSystemWatcher(settings.PluginDirectory);
                    return PluginManager.CreateForDirectory(settings.PluginDirectory, watcher);
                })
                .OnActivated(ctx => ctx.Instance.Start())
                .As<IPluginManager>()
                .SingleInstance();

            container = builder.Build();
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            var statusBar = container.Resolve<SystemStatusBar>();
            statusBar.Show();
        }

        public override void WillTerminate(NSNotification notification)
        {
            var pluginManager = container.Resolve<IPluginManager>();
            pluginManager.Stop();
            pluginManager.Dispose();
        }
    }
}
