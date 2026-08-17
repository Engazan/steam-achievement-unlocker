using System;
using System.Globalization;
using System.Windows;

namespace SteamAchievementUnlocker
{
    internal static class GameWindowPlacement
    {
        public static void Apply(Window window, string[] arguments)
        {
            if (TryGetDoubleOption(arguments, "--left", out double left) &&
                TryGetDoubleOption(arguments, "--top", out double top) &&
                TryGetDoubleOption(arguments, "--width", out double width) &&
                TryGetDoubleOption(arguments, "--height", out double height))
            {
                window.WindowStartupLocation = WindowStartupLocation.Manual;
                window.Left = left;
                window.Top = top;
                window.Width = Math.Max(window.MinWidth, width);
                window.Height = Math.Max(window.MinHeight, height);
            }

            string stateValue = GetOption(arguments, "--state");
            if (Enum.TryParse(stateValue, true, out WindowState state) && state != WindowState.Minimized)
            {
                window.WindowState = state;
            }
        }

        private static bool TryGetDoubleOption(string[] arguments, string name, out double value) =>
            double.TryParse(GetOption(arguments, name), NumberStyles.Float,
                CultureInfo.InvariantCulture, out value);

        private static string GetOption(string[] arguments, string name)
        {
            for (int index = 2; index < arguments.Length - 1; index++)
            {
                if (string.Equals(arguments[index], name, StringComparison.OrdinalIgnoreCase))
                {
                    return arguments[index + 1];
                }
            }
            return null;
        }
    }
}
