using System;
using System.Collections.Immutable;

namespace Barista.Data
{
    public class PluginExecution
    {
        public ImmutableList<ImmutableList<Item>> Items { get; internal set; }
        public bool Success { get; set; }
        public DateTime LastExecution { get; set; }
    }
}
