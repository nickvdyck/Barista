using Barista.Core.Data;

namespace Barista.Core.Events
{
    public class PluginExecutedEvent : IPluginEvent
    {
        public Plugin Plugin { get; set; }
        public PluginExecution Execution { get; set; }
    }
}
