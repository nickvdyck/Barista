using System;
using System.Threading.Tasks;
using Barista.Core.Data;
using Barista.Core.Execution;

namespace Barista.Core.Commands
{
    public class ExecuteItemCommand : ICommand
    {
        private readonly IExecutionHandler _executionHandler;

        public Item Item { get; set; }

        public ExecuteItemCommand(IExecutionHandler executionHandler)
        {
            _executionHandler = executionHandler;
        }

        public async Task Execute()
        {
            try
            {
                await _executionHandler.Execute(Item);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TODO: add application logger");
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }
    }
}
