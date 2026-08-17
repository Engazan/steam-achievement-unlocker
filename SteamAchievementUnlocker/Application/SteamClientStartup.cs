using System;
using System.Windows;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal static class SteamClientStartup
    {
        public static bool TryInitialize(API.SteamClientSession client, uint appId)
        {
            try
            {
                client.Initialize(appId);
                return true;
            }
            catch (API.SteamClientInitializationException exception)
            {
                ShowInitializationError(exception);
            }
            catch (DllNotFoundException)
            {
                StyledDialog.Show(
                    null,
                    "You've caused an exceptional error!",
                    "Error",
                    StyledDialogButtons.Ok,
                    StyledDialogIcon.Error);
            }

            return false;
        }

        private static void ShowInitializationError(API.SteamClientInitializationException exception)
        {
            string message;
            if (exception.Failure == API.SteamClientInitializationFailure.ConnectToGlobalUser)
            {
                message = "Steam is not running. Please start Steam then run this tool again.\n\n" +
                          "If you have the game through Family Share, the game may be locked due to\n" +
                          "the Family Share account actively playing a game.\n\n" +
                          "(" + exception.Message + ")";
            }
            else if (string.IsNullOrEmpty(exception.Message) == false)
            {
                message = "Steam is not running. Please start Steam then run this tool again.\n\n" +
                          "(" + exception.Message + ")";
            }
            else
            {
                message = "Steam is not running. Please start Steam then run this tool again.";
            }

            StyledDialog.Show(
                null,
                message,
                "Error",
                StyledDialogButtons.Ok,
                StyledDialogIcon.Error);
        }
    }
}
