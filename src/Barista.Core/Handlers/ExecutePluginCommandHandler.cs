using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Barista.Common;
using Barista.Data;
using Barista.Data.Commands;
using Barista.Data.Events;
using Barista.Data.Queries;

namespace Barista.Handlers
{
    internal class ExecutePluginCommandHandler : ICommandHandler<ExecutePluginCommand>
    {
        private readonly IMediator _mediator;

        public ExecutePluginCommandHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task Handle(ExecutePluginCommand command, CancellationToken cancellationToken = default)
        {
            var plugin = await _mediator.Send(new GetPluginQuery
            {
                Name = command.PluginName,
            });

            Debug.WriteLine($"Executing plugin {plugin.Name}");

            var info = new ProcessStartInfo
            {
                FileName = plugin.FilePath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8,
            };

            (string Data, string Error) result;

            try
            {
                using (var process = Process.Start(info))
                {
                    var data = await process.StandardOutput.ReadToEndAsync();
                    process.WaitForExit();

                    result = (Data: data, Error: "");
                }
            }
            catch (Exception ex)
            {
                result = (Data: "", Error: ex.ToString());
            }

            var executed = new PluginExecutedEvent
            {
                Name = plugin.Name,
                LastExecution = DateTime.UtcNow,
            };

            if (!string.IsNullOrEmpty(result.Data))
            {
                var items = PluginParser.ParseExecution(result.Data, plugin);
                executed.Items = items;
                executed.Success = true;
            }
            else
            {
                executed.Items = ImmutableList.CreateBuilder<ImmutableList<Item>>().ToImmutableList();
                executed.Success = false;
            }

            var _ = _mediator.Publish(executed);
        }
    }
}
