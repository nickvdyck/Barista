using System;
using Barista.Core;
using Barista.Core.FileSystem;
using Barista.Core.Plugins;
using Barista.Core.Plugins.Events;
using TinyIoC;

namespace Barista.IOC
{
    public static class TinyIoCContainerExtensions
    {
        public static TinyIoCContainer AddPluginManager(this TinyIoCContainer container, Action<IPluginManagerOptions> optionsBuilder)
        {
            container.Register<IPluginManagerOptions>((provider, _) =>
            {
                var options = new PluginManagerOptions();
                optionsBuilder(options);
                return options;
            });
            container.Register<IFileProvider, PluginFileProvider>().AsSingleton();
            container.Register<IObservable<IPluginEvent>, PluginEventsMonitor>().AsSingleton();
            container.Register<IPluginManager, PluginManager>().AsSingleton();
            return container;
        }
    }
}
