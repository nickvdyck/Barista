using Barista.Core.Data;

namespace Barista.Core.Plugins.Events
{
    public class PluginExecutedEvent : IPluginEvent
    {
        public Plugin Plugin { get; set; }
        public PluginExecution Execution { get; set; }
    }
}
