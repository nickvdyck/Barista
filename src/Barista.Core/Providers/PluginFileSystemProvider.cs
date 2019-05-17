using System;
using System.Collections.Immutable;
using Barista.Core.Data;
using Barista.Core.Events;
using Barista.Core.FileSystem;

namespace Barista.Core.Providers
{
    internal class PluginFileSystemProvider : BasePluginProvider, IDisposable
    {
        private readonly IFileProvider _fileProvider;
        private ImmutableList<Plugin> _pluginCache;
        private IDisposable _watcherDisposer;
        private readonly PluginEventsMonitor _eventsMonitor;

        public PluginFileSystemProvider(IFileProvider fileProvider, PluginEventsMonitor eventsMonitor)
        {
            _fileProvider = fileProvider;
            _watcherDisposer = _fileProvider.Watch(OnPluginChanged);
            _eventsMonitor = eventsMonitor;
        }

        public override ImmutableList<Plugin> ListPlugins()
        {
            if (_pluginCache != null) return _pluginCache;

            var files = _fileProvider.GetDirectoryContents();

            if (!files.Exists) throw new Exception("Wow something went wrong, it looks like your plugin directory does not exist!");

            var builder = ImmutableList.CreateBuilder<Plugin>();
            foreach (var file in files)
            {
                if (file.IsDirectory) continue;
                var plugin = FromFilePath(file.PhysicalPath);

                builder.Add(plugin);
            }

            _pluginCache = builder.ToImmutable();
            return _pluginCache;
        }

        public void OnPluginChanged()
        {
            _pluginCache = null;
            _eventsMonitor.PluginsChanged();
        }

        public void Dispose()
        {
            _watcherDisposer.Dispose();
        }
    }
}
