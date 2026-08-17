using System;
using System.Globalization;
using System.IO;
using System.Windows;
using SteamAchievementUnlocker.Updates;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal static class Program
    {
        internal const int ReturnToPickerExitCode = 10;

        [STAThread]
        private static int Main(string[] args)
        {
            if (UpdateInstaller.IsApplyUpdateRequest(args))
            {
                return UpdateInstaller.Apply(args);
            }

            string startupPath = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
            if (string.Equals(API.SteamInstallLocator.GetInstallPath(), startupPath, StringComparison.OrdinalIgnoreCase))
            {
                StyledDialog.Show(
                    null,
                    "This tool declines to being run from the Steam directory.",
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
                return 1;
            }

            if (args.Length > 0 && string.Equals(args[0], "--game", StringComparison.OrdinalIgnoreCase))
            {
                return RunGameManager(args);
            }

            return RunGameSelection();
        }

        private static int RunGameSelection()
        {
            if (ApplicationUpdateService.TryOfferUpdate())
            {
                return 0;
            }

            using (API.SteamClientSession client = new())
            {
                if (SteamClientStartup.TryInitialize(client, 0) == false)
                {
                    return 1;
                }

                System.Windows.Application application = new();
                application.Run(new GameSelectionWindow(client));
                return 0;
            }
        }

        private static int RunGameManager(string[] args)
        {
            if (args.Length < 2 || long.TryParse(args[1], NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out long appId) == false)
            {
                StyledDialog.Show(
                    null,
                    "Could not parse the game application ID.",
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
                return ReturnToPickerExitCode;
            }

            using (API.SteamClientSession client = new())
            {
                if (SteamClientStartup.TryInitialize(client, (uint)appId) == false)
                {
                    return ReturnToPickerExitCode;
                }

                SteamAchievementUnlocker.GameManagerWindow manager = new(appId, client);
                GameWindowPlacement.Apply(manager, args);

                bool returnToPicker = false;
                manager.ReturnToPickerRequested += (_, _) =>
                {
                    returnToPicker = true;
                    manager.Close();
                };

                System.Windows.Application application = new();
                application.Run(manager);
                return returnToPicker ? ReturnToPickerExitCode : 0;
            }
        }

    }
}
