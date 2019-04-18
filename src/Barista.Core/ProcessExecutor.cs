using System.Diagnostics;
using System.Threading.Tasks;

namespace Barista
{
    public static class ProcessExecutor
    {
        public static async Task<string> Run(string filename)
        {
            var info = new ProcessStartInfo
            {
                FileName = filename,
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(info);

            var data = await process.StandardOutput.ReadToEndAsync();

            process.WaitForExit();

            return data;
        }
    }
}
