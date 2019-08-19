using System;
using System.Collections.Generic;
using Barista.Common;
using Barista.Common.DependencyInjection;
using Barista.Common.FileSystem;
using Barista.Common.Jobs;
using Barista.Data;
using Barista.Data.Commands;
using Barista.Data.Events;
using Barista.Data.Queries;
using Barista.Handlers;
using DryIoc;

namespace Barista.Extensions
{
    public static class ContainerExtensions
    {
        public static Container AddPluginManager(this Container container, Action<PluginManagerOptions> optionsBuilder)
        {
            // Provider
            container.RegisterDelegate<ServiceProvider>(r => r.Resolve);

            // Handlers
            //container.RegisterMany(new[] { typeof(IMediator).GetAssembly(), },
            //    type =>
            //        type.Namespace == "Barista.Handlers" &&
            //        type.IsAssignableTo(typeof(ICommandHandler<>)) ||
            //        type.IsAssignableTo(typeof(IQueryHandler<,>)) ||
            //        type.IsAssignableTo(typeof(IEventHandler<>))
            //);

            container.Register<ICommandHandler<ExecuteItemCommand>, ExecuteItemCommandHandler>(Reuse.Singleton);
            container.Register<ICommandHandler<ExecutePluginCommand>, ExecutePluginCommandHandler>(Reuse.Singleton);
            container.Register<IQueryHandler<GetPluginQuery, Plugin>, GetPluginQueryHandler>(Reuse.Singleton);
            container.Register<IQueryHandler<GetPluginsQuery, IReadOnlyCollection<Plugin>>, GetPluginsQueryHandler>();
            container.Register<IEventHandler<PluginsUpdatedEvent>, SchedulePluginsEventHandler>(Reuse.Singleton);
            container.Register<ICommandHandler<SyncPluginsCommand>, SyncPluginsCommandHandler>(Reuse.Singleton);

            // Services
            container.Register<IMediator, Mediator>();
            container.RegisterDelegate<JobScheduler>(_ => new JobScheduler(), Reuse.Singleton);
            container.RegisterDelegate<IFileProvider>((provider) =>
            {
                var options = provider.Resolve<PluginManagerOptions>();
                var watcher = provider.Resolve<IFileSystemWatcher>();
                return new LocalFileProvider(options.Directory, watcher);
            });
            container.Register<IPluginStore, PluginStore>(Reuse.Singleton);

            // Manager
            container.RegisterDelegate<PluginManagerOptions>(_ =>
            {
                var options = new PluginManagerOptions();
                optionsBuilder(options);
                return options;
            });
            container.Register<IPluginManager, PluginManager>(Reuse.Singleton);

            return container;
        }
    }
}
