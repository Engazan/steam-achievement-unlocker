using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace SteamAchievementUnlocker
{
    internal sealed class GameManagerLauncher
    {
        public async Task<int> LaunchAndWaitAsync(uint appId, Rect placement, WindowState windowState)
        {
            string executablePath = Path.Combine(
                AppContext.BaseDirectory,
                "Steam Achievement Unlocker.exe");
            if (File.Exists(executablePath) == false)
            {
                executablePath = Environment.ProcessPath;
            }

            if (string.IsNullOrEmpty(executablePath) ||
                string.Equals(Path.GetExtension(executablePath), ".dll", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "The application executable could not be found next to the application assembly.");
            }

            string arguments = string.Format(
                CultureInfo.InvariantCulture,
                "--game {0} --left {1:R} --top {2:R} --width {3:R} --height {4:R} --state {5}",
                appId,
                placement.Left,
                placement.Top,
                placement.Width,
                placement.Height,
                windowState);

            using Process process = Process.Start(new ProcessStartInfo
            {
                FileName = executablePath,
                Arguments = arguments,
                WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory,
                UseShellExecute = true,
            }) ?? throw new InvalidOperationException("The game process could not be started.");

            await Task.Run(process.WaitForExit);
            return process.ExitCode;
        }
    }
}
