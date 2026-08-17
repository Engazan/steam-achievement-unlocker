using System;
using System.Runtime.InteropServices;

namespace SteamIntegration
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
    internal struct NativeObjectLayout
    {
        public IntPtr VirtualTable;
    }
}
