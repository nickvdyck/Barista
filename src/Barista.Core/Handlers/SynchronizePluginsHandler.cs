using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Barista.Actions;
using Barista.Common.FileSystem;
using Barista.Common.Redux;
using Barista.Data;
using Barista.Events;
using Barista.State;
using MediatR;

namespace Barista.Handlers
{
    internal class SynchronizePluginsHandler : INotificationHandler<SynchronizePlugins>
    {
        private readonly IReduxStore<BaristaPluginState> _store;
        private readonly IFileProvider _fileProvider;

        public SynchronizePluginsHandler(IReduxStore<BaristaPluginState> store, IFileProvider fileProvider)
        {
            _store = store;
            _fileProvider = fileProvider;
        }

        public Task Handle(SynchronizePlugins effect, CancellationToken cancellationToken)
        {
            var files = _fileProvider.GetDirectoryContents();

            if (!files.Exists)
            {
                return Task.CompletedTask;
            }

            var builder = ImmutableList.CreateBuilder<Plugin>();
            foreach (var file in files)
            {
                if (file.IsDirectory) continue;

                var plugin = PluginParser.FromFilePath(file.PhysicalPath);

                builder.Add(plugin);
            }

            _store.Dispatch(new PluginsUpdatedAction
            {
                Plugins = builder.ToImmutableList(),
            });

            return Task.CompletedTask;
        }
    }
}
