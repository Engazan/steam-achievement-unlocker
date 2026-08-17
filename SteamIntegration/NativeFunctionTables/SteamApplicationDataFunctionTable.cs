using System;
using System.Runtime.InteropServices;

namespace SteamIntegration.InteropFunctionTables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct ApplicationMetadataFunctionTable
    {
        public IntPtr GetAppData;
    }
}
