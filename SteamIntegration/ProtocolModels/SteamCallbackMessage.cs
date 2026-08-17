using System;
using System.Runtime.InteropServices;

namespace SteamIntegration.Models
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SteamCallbackMessage
    {
        public int User;
        public int Id;
        public IntPtr ParamPointer;
        public int ParamSize;
    }
}
