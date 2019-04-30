using System.Diagnostics;
using System.Threading.Tasks;
using Barista.Core.Data;

namespace Barista.Core.Commands
{
    internal class PluginMenuItemExecuteCommand : ICommand
    {
        private readonly IPluginMenuItem _menuItem;
        public PluginMenuItemExecuteCommand(IPluginMenuItem menuItem)
        {
            _menuItem = menuItem;
        }

        private async Task<string> Run(string filePath, string[] args = default, bool terminal = false)
        {
            var info = new ProcessStartInfo
            {
                FileName = filePath,
                Arguments = string.Join(" ", args ?? new string[] { }).Trim(),
                UseShellExecute = terminal,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(info);

            var data = await process.StandardOutput.ReadToEndAsync();
            process.WaitForExit();
            return data;
        }

        public Task Execute()
        {
            if (_menuItem.BashScript != string.Empty)
            {
                return Run(_menuItem.BashScript, _menuItem.Params, _menuItem.Terminal);
            }
            else if(_menuItem.Href != string.Empty)
            {
                return Run("open", new string[] { _menuItem.Href });
            }

            return Task.CompletedTask;
        }
    }
}
