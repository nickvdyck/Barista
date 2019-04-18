using System;
using System.Collections.Generic;
using System.Timers;
using Barista.Core.FileSystem;

namespace Barista
{
    public sealed class PluginManager
    {
        private readonly IFileProvider _fileProvider;
        private List<Plugin> _plugins { get; set; }
        private Timer _timer;

        public PluginManager(IFileProvider pluginFileProvider)
        {
            _plugins = new List<Plugin>();
            _fileProvider = pluginFileProvider;
        }

        public void Load()
        {
            var files = _fileProvider.GetDirectoryContents();

            if (!files.Exists) throw new Exception("Wow something went wrong, it looks like your plugin directory does not exist!");

            foreach (var file in files)
            {
                if (file.IsDirectory) continue;

                _plugins.Add(new Plugin(file.PhysicalPath));
            }
        }

        public IReadOnlyList<Plugin> GetPlugins()
        {
            return _plugins;
        }

        public void RunAll()
        {
            _timer.Stop();

            foreach (var plugin in _plugins)
            {
                var _ = plugin.Execute();
            }

            _timer.Start();
        }

        public void Start()
        {
            if (_timer == null) StartCore();
        }

        private void StartCore()
        {
            _timer = new Timer(1000)
            {
                AutoReset = true,
                Enabled = true,
            };

            _timer.Elapsed += (sender, e) =>
            {
                foreach (var plugin in _plugins)
                {
                    if (plugin.LastExecution == DateTime.MinValue)
                    {
                        var _ = plugin.Execute();
                    }

                    var offset = DateTime.Now - plugin.LastExecution;

                    if (offset.Seconds > plugin.Interval)
                    {
                        var _ = plugin.Execute();
                    }
                }
            };
        }
    }
}
