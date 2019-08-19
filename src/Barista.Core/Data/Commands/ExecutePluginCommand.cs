using Barista.Common;

namespace Barista.Data.Commands
{
    public class ExecutePluginCommand : ICommand
    {
        public string PluginName { get; set; }
    }
}
