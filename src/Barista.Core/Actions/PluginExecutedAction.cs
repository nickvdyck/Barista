using System;
using System.Collections.Immutable;
using Barista.Common.Redux;
using Barista.Data;

namespace Barista.Actions
{
    public class PluginExecutedAction : IAction
    {
        public string Name { get; set; }
        public ImmutableList<ImmutableList<Item>> Items { get; internal set; }
        public bool Success { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
