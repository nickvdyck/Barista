using System;
using System.Collections.Immutable;

namespace Barista.Domain
{
    public class PluginExecutionResult
    {
        public ImmutableList<ImmutableList<Item>> Items { get; internal set; }
        public bool Success { get; set; }
        public DateTime LastExecution { get; set; }
    }
}
