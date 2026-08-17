using System;

namespace SteamIntegration
{
    internal interface INativeInterfaceWrapper
    {
        void SetupFunctions(IntPtr objectAddress);
    }
}
