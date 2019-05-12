using System;
using System.Threading.Tasks;
using Barista.Core.Data;
using Barista.Core.Execution;

namespace Barista.Core.Commands
{
    internal class ExecutePluginCommand : ICommand
    {
        private readonly IExecutionHandler _executionHandler;

        public Plugin Plugin { get; set; }

        public ExecutePluginCommand(IExecutionHandler executionHandler)
        {
            _executionHandler = executionHandler;
        }

        public async Task Execute()
        {
            try
            {
                if (Plugin.Enabled)
                {
                    await _executionHandler.Execute(Plugin);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("ERROR: trying to execute a disabled plugin");
                }

            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TODO: add application logger");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
