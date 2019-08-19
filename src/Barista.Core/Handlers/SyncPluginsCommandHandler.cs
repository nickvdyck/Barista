using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Data;
using Barista.Data.Commands;
using Barista.Data.Events;
using Barista.Common.FileSystem;

namespace Barista.Handlers
{
    internal class SyncPluginsCommandHandler : ICommandHandler<SyncPluginsCommand>
    {
        private readonly IFileProvider _fileProvider;
        private readonly IPluginStore _store;
        private readonly IMediator _mediator;

        public SyncPluginsCommandHandler(IFileProvider fileProvider, IPluginStore store, IMediator mediator)
        {
            _fileProvider = fileProvider;
            _store = store;
            _mediator = mediator;
        }

        public Task Handle(SyncPluginsCommand command, CancellationToken cancellationToken = default)
        {
            var files = _fileProvider.GetDirectoryContents();

            if (!files.Exists) throw new Exception("Wow something went wrong, it looks like your plugin directory does not exist!");

            var builder = ImmutableList.CreateBuilder<Plugin>();
            foreach (var file in files)
            {
                if (file.IsDirectory) continue;
                var plugin = PluginParser.FromFilePath(file.PhysicalPath);

                builder.Add(plugin);
            }

            var plugins = builder.ToImmutableList();
            var removed = _store.Plugins.Except(plugins);
            var added = plugins.Except(_store.Plugins);

            _store.Update(builder.ToImmutable());
            _mediator.Publish(new PluginsUpdatedEvent
            {
                Added = added.ToImmutableList(),
                Removed = removed.ToImmutableList(),
            });

            return Task.CompletedTask;
        }
    }
}
