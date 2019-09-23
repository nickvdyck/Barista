using System.Collections.Generic;
using System.Collections.Immutable;
using Barista.Common.Redux;
using Barista.Data;

namespace Barista.State
{
    public class BaristaPluginState
    {
        public BaristaPluginState()
        {
            Plugins = new List<Plugin>().ToImmutableList();
            ExecutionResults = new Dictionary<string, PluginExecution>().ToImmutableDictionary();
        }

        public ImmutableList<Plugin> Plugins { get; internal set; }
        public ImmutableDictionary<string, PluginExecution> ExecutionResults;
    }
}
