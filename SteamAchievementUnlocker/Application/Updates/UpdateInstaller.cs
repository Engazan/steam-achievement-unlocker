using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace SteamAchievementUnlocker.Updates
{
    internal static class UpdateInstaller
    {
        private const string ApplyUpdateArgument = "--apply-update";

        public static bool IsApplyUpdateRequest(string[] args) =>
            args.Length == 4 && string.Equals(args[0], ApplyUpdateArgument, StringComparison.Ordinal);

        public static int Apply(string[] args)
        {
            if (IsApplyUpdateRequest(args) == false ||
                int.TryParse(args[3], out int parentProcessId) == false)
            {
                return 1;
            }

            string downloadedExecutable = Path.GetFullPath(args[1]);
            string targetExecutable = Path.GetFullPath(args[2]);
            WaitForParentProcess(parentProcessId);

            for (int attempt = 0; attempt < 30; attempt++)
            {
                try
                {
                    File.Move(downloadedExecutable, targetExecutable, true);
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = targetExecutable,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(targetExecutable),
                    });
                    ScheduleSelfDelete();
                    return 0;
                }
                catch (IOException)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
                catch (UnauthorizedAccessException)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(500));
                }
            }

            return 1;
        }

        private static void ScheduleSelfDelete()
        {
            string updaterPath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(updaterPath))
            {
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c timeout /t 2 /nobreak >nul & del /f /q \"{updaterPath}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
            });
        }

        private static void WaitForParentProcess(int processId)
        {
            try
            {
                using Process parent = Process.GetProcessById(processId);
                parent.WaitForExit(TimeSpan.FromSeconds(30));
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }
    }
}
