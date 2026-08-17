using System;
using System.Runtime.InteropServices;

namespace SteamIntegration.InteropFunctionTables
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal struct UserAccountFunctionTable
    {
        public IntPtr GetHUserAccount;
        public IntPtr LoggedOn;
        public IntPtr GetSteamID;
        public IntPtr InitiateGameConnection;
        public IntPtr TerminateGameConnection;
        public IntPtr TrackAppUsageEvent;
        public IntPtr GetUserDataFolder;
        public IntPtr StartVoiceRecording;
        public IntPtr StopVoiceRecording;
        public IntPtr GetCompressedVoice;
        public IntPtr DecompressVoice;
        public IntPtr GetAuthSessionTicket;
        public IntPtr BeginAuthSession;
        public IntPtr EndAuthSession;
        public IntPtr CancelAuthTicket;
        public IntPtr UserHasLicenseForApp;
    }
}
