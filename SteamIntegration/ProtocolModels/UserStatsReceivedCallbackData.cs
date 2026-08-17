using System.Runtime.InteropServices;

namespace SteamIntegration.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UserStatsReceivedCallbackData
    {
        public ulong GameId;
        public int Result;
        public ulong SteamIdUser;
    }
}
