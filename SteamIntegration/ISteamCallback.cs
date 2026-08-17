using System;

namespace SteamIntegration
{
    public interface ISteamCallback
    {
        int Id { get; }
        bool IsServer { get; }
        void Run(IntPtr param);
    }
}
