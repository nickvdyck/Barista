using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Barista.Common
{
    public static class ProcessHelpers
    {
        public static int ExecuteCommand(string fileName) => ExecuteCommand(fileName, Array.Empty<string>());

        public static int ExecuteCommand(string fileName, bool useShellExecute) =>
            ExecuteCommand(fileName, Array.Empty<string>(), useShellExecute);

        public static int ExecuteCommand(string fileName, IEnumerable<string> args, bool useShellExecute = false)
        {
            var info = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = string.Join(' ', args).Trim(),
                UseShellExecute = useShellExecute,
            };

            try
            {
                using var process = Process.Start(info);
                process.WaitForExit();
                return process.ExitCode;
            }
            catch
            {
                return 1;
            }
        }
    }
}
