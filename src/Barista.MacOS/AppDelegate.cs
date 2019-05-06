using AppKit;
using Autofac;
using Barista.MacOS.Services;
using Barista.MacOS.ViewModels;
using Barista.MacOS.Views.Preferences;
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
            builder.RegisterType<StatusBar>().AsSelf();
            builder.RegisterType<GeneralViewController>().AsSelf();
            builder.RegisterType<PluginViewController>().AsSelf();
            builder.RegisterType<PreferencesWindowFactory>().AsSelf();

            // ViewModels
            builder.RegisterType<GeneralPreferencesViewModel>().AsSelf();

            // Services
            builder.RegisterType<DefaultsService>().As<ISettingsService>();

            builder.Register(s =>
            {
                var settings = s.Resolve<ISettingsService>().GetSettings();
                return PluginManager.CreateAtDirectory(settings.PluginDirectory);
            }).As<PluginManager>().SingleInstance();


            container = builder.Build();

        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            var statusBar = container.Resolve<StatusBar>();
            var pluginManager = container.Resolve<PluginManager>();

            statusBar.Draw();
            pluginManager.Start();
        }

        public override void WillTerminate(NSNotification notification)
        {
            // Insert code here to tear down your application
        }
    }
}
