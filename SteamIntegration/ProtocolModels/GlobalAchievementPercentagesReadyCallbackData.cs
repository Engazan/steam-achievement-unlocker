using System.Runtime.InteropServices;

namespace SteamIntegration.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct GlobalAchievementPercentagesReadyCallbackData
    {
        public const int CallbackId = 1110;

        public ulong GameId;
        public int Result;
    }
}
