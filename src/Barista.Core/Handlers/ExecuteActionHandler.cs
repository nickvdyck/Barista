using System;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Barista.Common.Redux;
using Barista.Data;
using Barista.Events;
using Barista.State;
using MediatR;

namespace Barista.Handlers
{
    public class ExecuteActionHandler : INotificationHandler<ExecuteAction>
    {
        private readonly IReduxStore<BaristaPluginState> _store;

        public ExecuteActionHandler(IReduxStore<BaristaPluginState> store)
        {
            _store = store;
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

        public async Task Handle(ExecuteAction executeAction, CancellationToken cancellationToken)
        {
            var item = executeAction.Item;

            if (item.Type == ItemType.RunScriptAction || item.Type == ItemType.RunScriptInTerminalAction)
            {
                await Execute(item.BashScript, string.Join(" ", item.Params ?? new string[] { }).Trim(), item.Terminal);
            }
            else if (item.Type == ItemType.Link)
            {
                await Execute("open", item.Href);
            }
            else if (item.Type == ItemType.RefreshAction)
            {

                _store
                    .Select(state => state.Plugins)
                    .SelectMany(p => p)
                    .Where(p => p.Name == item.PluginName)
                    .Take(1)
                    .Subscribe(plugin =>
                        _store.Dispatch(new ExecutePlugin
                        {
                            Name = plugin.Name,
                            FilePath = plugin.FilePath,
                        })
                    );
            }
            else
            {
                Debug.WriteLine("TODO: make this log error better");
                Debug.WriteLine("Executing a non executable item!");
            }
        }
    }
}
