using System;
using Barista.Common.FileSystem;
using Barista.Common.Redux;
using Barista.Handlers;
using Barista.Events;
using Barista.State;
using DryIoc;
using MediatR;
using Barista.Common.Jobs;

namespace Barista
{
    public class BaristaAppBuilder
    {
        private readonly Container _container;
        private BaristaAppOptions _options { get; set; }
        private Action<IContainer> _appServicesBuilder { get; set; }

        public BaristaAppBuilder()
        {
            _container = new Container(
                rules => rules.WithoutThrowOnRegisteringDisposableTransient()
            );
            _options = new BaristaAppOptions();
        }

        private void CreateServices()
        {
            // Set up Services
            _container.RegisterDelegate<ServiceFactory>(r => r.Resolve);
            _container.Register<IMediator, Mediator>(Reuse.Singleton);
            _container.RegisterDelegate<JobScheduler>(_ => new JobScheduler(), Reuse.Singleton);


            // Se tup FilProvider
            _container.Register<IFileSystemWatcher, NetFileSystemWatcher>();
            _container.RegisterDelegate<IFileProvider>((provider) =>
            {
                var watcher = provider.Resolve<IFileSystemWatcher>();
                return new LocalFileProvider(_options.PluginDirectory, watcher);
            });

            // Set up Redux
            _container.Register(typeof(IReduxStore<>), typeof(ReduxStore<>), Reuse.Singleton);
            _container.Register(typeof(IReducer<BaristaPluginState>), typeof(PluginReducer));

            // Set up Effect Handlers
            _container.Register<INotificationHandler<SynchronizePlugins>, SynchronizePluginsHandler>();
            _container.Register<INotificationHandler<SchedulePlugins>, SchedulePluginsHandler>();
            _container.Register<INotificationHandler<ExecutePlugin>, ExecutePluginHandler>();
            _container.Register<INotificationHandler<ExecuteAction>, ExecuteActionHandler>();

            // Set up App
            _container.Register<BaristaApp>();
        }

        public BaristaAppBuilder ConfigureOptions(Action<BaristaAppOptions> configBuilder)
        {
            var options = new BaristaAppOptions
            {
                PluginDirectory = _options.PluginDirectory,
            };

            configBuilder(options);
            _options = options;
            return this;
        }

        public BaristaAppBuilder ConfigureServices(Action<IContainer> servicesBuilder)
        {
            _appServicesBuilder = servicesBuilder;
            return this;
        }

        public BaristaApp Build()
        {
            CreateServices();
            _appServicesBuilder?.Invoke(_container);
            return _container.Resolve<BaristaApp>();
        }
    }
}
