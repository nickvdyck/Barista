using System.Collections.Generic;
using Barista.Data;

namespace Barista
{
    internal class PluginStore : IPluginStore
    {
        public IReadOnlyCollection<Plugin> Plugins { get; private set; } = new List<Plugin>();

        public void Update(IReadOnlyCollection<Plugin> plugins)
        {
            Plugins = plugins;
        }
    }
}
