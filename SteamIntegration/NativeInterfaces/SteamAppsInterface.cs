using System;
using System.Runtime.InteropServices;
using SteamIntegration.InteropFunctionTables;

namespace SteamIntegration.SteamInterfaces
{
    internal class InstalledApplicationCatalogInterface : NativeInterfaceWrapper<InstalledApplicationCatalogFunctionTable>
    {
        #region IsSubscribed
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeIsSubscribedApp(IntPtr self, uint gameId);

        public bool IsSubscribedApp(uint gameId)
        {
            return this.Call<bool, NativeIsSubscribedApp>(this.Functions.IsSubscribedApp, this.ObjectAddress, gameId);
        }
        #endregion

        #region GetCurrentGameLanguage
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetCurrentGameLanguage(IntPtr self);

        public string GetCurrentGameLanguage()
        {
            var languagePointer = this.Call<IntPtr, NativeGetCurrentGameLanguage>(
                this.Functions.GetCurrentGameLanguage,
                this.ObjectAddress);
            return NativeStringMarshaller.PointerToString(languagePointer);
        }
        #endregion
    }
}
