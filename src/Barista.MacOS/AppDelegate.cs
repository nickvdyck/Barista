using AppKit;
using DryIoc;
using Barista.Common.FileSystem;
using Barista.MacOS.Services;
using Barista.MacOS.Utils;
using Barista.MacOS.ViewModels;
using Barista.MacOS.Views.Preferences;
using Barista.MacOS.Views.StatusBar;
using Foundation;
using System;

namespace Barista.MacOS
{
    [Register("AppDelegate")]
    public class AppDelegate : NSApplicationDelegate
    {
        private readonly Container _container;

        public AppDelegate()
        {
            var container = new Container(rules => rules.WithoutThrowOnRegisteringDisposableTransient());
            var defaults = new DefaultsService();

            var baristaApp = new BaristaAppBuilder()
                .ConfigureServices(services =>
                {
                    services.RegisterDelegate<IFileSystemWatcher>(
                        (provider) =>
                        {
                            var settings = defaults.GetSettings();
                            return new MacFileSystemWatcher(settings.PluginDirectory);
                        },
                        ifAlreadyRegistered: IfAlreadyRegistered.Replace
                    );
                })
                .ConfigureOptions(options =>
                {
                    var settings = defaults.GetSettings();
                    options.PluginDirectory = settings.PluginDirectory;
                })
                .Build();

            // App
            container.RegisterDelegate<BaristaApp>(_ => baristaApp);

            // Utils
            container.RegisterDelegate<ServiceProvider>(r => r.Resolve);

            // Views
            container.Register<BaristaStatusBar>();
            container.Register<BaristaStatusBarItem>(made: Made.Of(
                () => new BaristaStatusBarItem(Arg.Of<BaristaApp>(), Arg.Of<PreferencesWindowFactory>())
            ));
            container.Register<GeneralViewController>(made: Made.Of(() => new GeneralViewController(Arg.Of<GeneralPreferencesViewModel>())));
            container.Register<PluginViewController>(made: Made.Of(() => new PluginViewController()));
            container.Register<PreferencesWindowFactory>();

            // ViewModels
            container.Register<GeneralPreferencesViewModel>(Reuse.Singleton);

            _container = container;
        }

        public override void DidFinishLaunching(NSNotification notification)
        {
            var app = _container.Resolve<BaristaApp>();
            app.Start();
            _container.Resolve<BaristaStatusBar>();

            app.Store
                .Select()
                .Subscribe(state => System.Diagnostics.Debug.WriteLine($"Total Plugins {state.Plugins.Count}"));
        }

        public override void WillTerminate(NSNotification notification)
        {
            var app = _container.Resolve<BaristaApp>();
            app.Dispose();
        }
    }
}
