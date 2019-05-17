using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barista.Core.Data
{
    public class PluginExecution
    {
        public Plugin Plugin { get; internal set; }
        public ImmutableList<ImmutableList<Item>> Items { get; internal set; }
        public bool Success { get; set; } = true;
    }
}
