using System.Collections.Generic;
using Barista.Common;

namespace Barista.Data.Events
{
    public class PluginsUpdatedEvent :IEvent
    {
        public IReadOnlyCollection<Plugin> Removed { get; set; }
        public IReadOnlyCollection<Plugin> Added { get; set; }
    }
}
