using System;
using System.Collections.Generic;
using System.Timers;
using Barista.Core.FileSystem;
using Barista.Core.Internal;
using Barista.Core.Data;
using System.Linq;
using Barista.Core.Utils;
using Barista.Core.Commands;
using Barista.Core.Extensions;

namespace Barista
{
    public sealed class PluginManager
    {
        public static PluginManager CreateAtDirectory(string pluginDirectory)
        {
            var provider = new LocalFileProvider(pluginDirectory);
            var factory = new PluginFactory();
            return new PluginManager(provider, factory);
        }

        private readonly IFileProvider _fileProvider;
        private readonly IPluginFactory _pluginFactory;
        private readonly Dictionary<Plugin, PluginExecutionHandler> _plugins = new Dictionary<Plugin, PluginExecutionHandler>();
        private Timer _timer;

        internal PluginManager(IFileProvider pluginFileProvider, IPluginFactory pluginFactory)
        {
            _fileProvider = pluginFileProvider;
            _pluginFactory = pluginFactory;
            Load();
        }

        internal void Load()
        {
            var files = _fileProvider.GetDirectoryContents();

            if (!files.Exists) throw new Exception("Wow something went wrong, it looks like your plugin directory does not exist!");

            foreach (var file in files)
            {
                if (file.IsDirectory) continue;
                var plugin = _pluginFactory.FromFilePath(file.PhysicalPath);
                var handler = new PluginExecutionHandler(plugin, new PluginOutputParser());

                _plugins.Add(plugin, handler);
            }
        }

        public void InvokeCommand(ICommand command)
        {
            command.Execute().Forget();
        }

        public IReadOnlyList<Plugin> GetPlugins()
        {
            return _plugins.Keys.ToList();
        }

        public IObservable<IReadOnlyCollection<IPluginMenuItem>> Monitor(Plugin plugin)
        {
            if (_plugins.TryGetValue(plugin, out var handler))
            {
                return handler;
            }

            return null;
        }

        public void RunAll()
        {
            _timer.Stop();

            foreach (var plugin in _plugins)
            {
                plugin.Value.ExecutePlugin().Forget();
            }

            _timer.Start();
        }

        public void Start()
        {
            if (_timer == null) StartCore();
        }

        private void StartCore()
        {
            _timer = new Timer(100)
            {
                AutoReset = true,
                Enabled = true,
            };

            _timer.Elapsed += (sender, e) =>
            {
                foreach (var plugin in _plugins)
                {
                    var offset = DateTime.Now - plugin.Key.LastExecution;

                    if (offset.TotalSeconds > plugin.Key.Interval)
                    {
                        plugin.Value.ExecutePlugin().Forget();
                    }
                }
            };
        }
    }
}
