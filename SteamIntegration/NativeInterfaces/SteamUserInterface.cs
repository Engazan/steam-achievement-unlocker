using System;
using System.Runtime.InteropServices;
using SteamIntegration.InteropFunctionTables;

namespace SteamIntegration.SteamInterfaces
{
    internal class UserAccountInterface : NativeInterfaceWrapper<UserAccountFunctionTable>
    {
        #region IsLoggedIn
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeLoggedOn(IntPtr self);

        public bool IsLoggedIn()
        {
            return this.Call<bool, NativeLoggedOn>(this.Functions.LoggedOn, this.ObjectAddress);
        }
        #endregion

        #region GetSteamID
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeGetSteamId(IntPtr self, out ulong steamId);

        public ulong GetSteamId()
        {
            var call = this.GetFunction<NativeGetSteamId>(this.Functions.GetSteamID);
            ulong steamId;
            call(this.ObjectAddress, out steamId);
            return steamId;
        }
        #endregion
    }
}
