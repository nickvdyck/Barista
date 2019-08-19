using Barista.Common;
using Barista.Common.Jobs;
using Barista.Data;
using Barista.Data.Commands;
using Barista.Data.Events;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Barista.Handlers
{
    internal class ExecuteItemCommandHandler : ICommandHandler<ExecuteItemCommand>
    {
        private readonly IMediator _mediator;
        public ExecuteItemCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        private async Task<(string Data, string Error)> Execute(string fileName, string arguments = "", bool terminal = default)
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = terminal,
                RedirectStandardOutput = true,
            };

            try
            {
                using (var process = Process.Start(info))
                {
                    var data = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();

                    return (Data: data, Error: "");
                }
            }
            catch (Exception ex)
            {
                return (Data: "", Error: ex.ToString());
            }
        }

        public async Task Handle(ExecuteItemCommand command, CancellationToken cancellationToken = default)
        {
            var item = command.Item;

            if (item.Type == ItemType.RunScriptAction || item.Type == ItemType.RunScriptInTerminalAction)
            {
                await Execute(item.BashScript, string.Join(" ", item.Params ?? new string[] { }).Trim(), item.Terminal);
            }
            else if (item.Type == ItemType.Link)
            {
                await Execute("open", item.Href);
            }
            else
            {
                Debug.WriteLine("TODO: make this log error better");
                Debug.WriteLine("Executing a non executable item!");
            }

            var _ = _mediator.Publish(new ItemExecutedEvent());
        }
    }
}
