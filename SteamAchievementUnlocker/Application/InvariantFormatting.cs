using System;

namespace SteamAchievementUnlocker
{
    internal static class InvariantFormatting
    {
        public static string Format(FormattableString formattable)
        {
            return FormattableString.Invariant(formattable);
        }
    }
}
