using Barista.Core.FileSystem;

namespace Barista.Core
{
    internal class PluginFileProvider : LocalFileProvider
    {
        public PluginFileProvider(IPluginManagerOptions options, IFileSystemWatcher watcher): base(options.Directory, watcher)
        {
        }
    }
}
