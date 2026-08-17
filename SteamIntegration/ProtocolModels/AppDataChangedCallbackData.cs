using System.Runtime.InteropServices;

namespace SteamIntegration.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AppDataChangedCallbackData
    {
        public uint Id;
        public bool Result;
    }
}
