using System.Diagnostics;
using System.Threading.Tasks;

namespace Barista
{
    public static class ProcessExecutor
    {
        public static async Task<string> Run(string filename, string[] args = default)
        {
            var info = new ProcessStartInfo
            {
                FileName = filename,
                Arguments = string.Join(" ", args ?? new string[] { }).Trim(),
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
