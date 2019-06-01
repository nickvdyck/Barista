using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Barista.Core.Data;
using Barista.Core.Events;
using Barista.Core.Jobs;

namespace Barista.Core.Plugins
{
    public class ItemExecutionJob : IJob
    {
        private readonly Item _item;
        public ItemExecutionJob(Item item)
        {
            _item = item;
        }

        public async Task<(string Data, string Error)> Execute(string fileName, string arguments = "", bool terminal = default)
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

        public async Task Execute()
        {
            if (_item.Type == ItemType.RunScriptAction || _item.Type == ItemType.RunScriptInTerminalAction)
            {
                await Execute(_item.BashScript, string.Join(" ", _item.Params ?? new string[] { }).Trim(), _item.Terminal);
            }
            else if (_item.Type == ItemType.Link)
            {
                await Execute("open", _item.Href);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("TODO: make this log error better");
                System.Diagnostics.Debug.WriteLine("Executing a non executable item!");
            }
        }
    }
}
