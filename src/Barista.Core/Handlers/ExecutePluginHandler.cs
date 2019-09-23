using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Barista.Actions;
using Barista.Common.Redux;
using Barista.Data;
using Barista.Events;
using Barista.State;
using MediatR;

namespace Barista.Handlers
{
    public class ExecutePluginHandler : INotificationHandler<ExecutePlugin>
    {
        private readonly IReduxStore<BaristaPluginState> _store;

        public ExecutePluginHandler(IReduxStore<BaristaPluginState> store)
        {
            _store = store;
        }

        public async Task Handle(ExecutePlugin execute, CancellationToken cancellationToken)
        {
            System.Diagnostics.Debug.WriteLine($"Executing plugin {execute.Name}");

            var info = new ProcessStartInfo
            {
                FileName = execute.FilePath,
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

            var pluginExecutedAction = new PluginExecutedAction
            {
                Name = execute.Name,
                TimeStamp = DateTime.UtcNow,
            };

            if (!string.IsNullOrEmpty(result.Data))
            {
                var items = PluginParser.ParseExecution(result.Data, execute.Name);
                pluginExecutedAction.Items = items;
                pluginExecutedAction.Success = true;
            }
            else
            {
                pluginExecutedAction.Items = ImmutableList.CreateBuilder<ImmutableList<Item>>().ToImmutableList();
                pluginExecutedAction.Success = false;
            }

            _store.Dispatch(pluginExecutedAction);
        }
    }
}
