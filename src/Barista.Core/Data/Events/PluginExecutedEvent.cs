using System;
using System.Collections.Generic;
using Barista.Common;
using Barista.Data;

namespace Barista.Data.Events
{
    public class PluginExecutedEvent : IEvent
    {
        public string Name { get; set; }
        public IReadOnlyCollection<IReadOnlyCollection<Item>> Items { get; set; }
        public bool Success { get; set; }
        public DateTime LastExecution { get; set; }
    }
}
