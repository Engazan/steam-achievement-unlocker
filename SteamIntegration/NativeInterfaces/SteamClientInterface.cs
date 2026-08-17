using System;
using System.Runtime.InteropServices;
using SteamIntegration.InteropFunctionTables;

namespace SteamIntegration.SteamInterfaces
{
    internal class SteamClientInterface : NativeInterfaceWrapper<SteamClientFunctionTable>
    {
        #region CreateSteamPipe
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeCreateSteamPipe(IntPtr self);

        public int CreateSteamPipe()
        {
            return this.Call<int, NativeCreateSteamPipe>(this.Functions.CreateSteamPipe, this.ObjectAddress);
        }
        #endregion

        #region ReleaseSteamPipe
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeReleaseSteamPipe(IntPtr self, int pipe);

        public bool ReleaseSteamPipe(int pipe)
        {
            return this.Call<bool, NativeReleaseSteamPipe>(this.Functions.ReleaseSteamPipe, this.ObjectAddress, pipe);
        }
        #endregion

        #region CreateLocalUser
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeCreateLocalUser(IntPtr self, ref int pipe, Models.SteamAccountType type);

        public int CreateLocalUser(ref int pipe, Models.SteamAccountType type)
        {
            var call = this.GetFunction<NativeCreateLocalUser>(this.Functions.CreateLocalUser);
            return call(this.ObjectAddress, ref pipe, type);
        }
        #endregion

        #region ConnectToGlobalUser
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeConnectToGlobalUser(IntPtr self, int pipe);

        public int ConnectToGlobalUser(int pipe)
        {
            return this.Call<int, NativeConnectToGlobalUser>(
                this.Functions.ConnectToGlobalUser,
                this.ObjectAddress,
                pipe);
        }
        #endregion

        #region ReleaseUser
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeReleaseUser(IntPtr self, int pipe, int user);

        public void ReleaseUser(int pipe, int user)
        {
            this.Call<NativeReleaseUser>(this.Functions.ReleaseUser, this.ObjectAddress, pipe, user);
        }
        #endregion

        #region SetLocalIPBinding
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeSetLocalIPBinding(IntPtr self, uint host, ushort port);

        public void SetLocalIPBinding(uint host, ushort port)
        {
            this.Call<NativeSetLocalIPBinding>(this.Functions.SetLocalIPBinding, this.ObjectAddress, host, port);
        }
        #endregion

        #region GetIUserAccount
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetIUserAccount(IntPtr self, int user, int pipe, IntPtr version);

        private TClass GetIUserAccount<TClass>(int user, int pipe, string version)
            where TClass : INativeInterfaceWrapper, new()
        {
            using (var nativeVersion = NativeStringMarshaller.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetIUserAccount>(
                    this.Functions.GetIUserAccount,
                    this.ObjectAddress,
                    user,
                    pipe,
                    nativeVersion.Handle);
                TClass result = new();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetUserAccountInterface
        public UserAccountInterface GetUserAccountInterface(int user, int pipe)
        {
            return this.GetIUserAccount<UserAccountInterface>(user, pipe, NativeInterfaceVersions.User);
        }
        #endregion

        #region GetIUserStatistics
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetIUserStatistics(IntPtr self, int user, int pipe, IntPtr version);

        private TClass GetIUserStatistics<TClass>(int user, int pipe, string version)
            where TClass : INativeInterfaceWrapper, new()
        {
            using (var nativeVersion = NativeStringMarshaller.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetIUserStatistics>(
                    this.Functions.GetIUserStatistics,
                    this.ObjectAddress,
                    user,
                    pipe,
                    nativeVersion.Handle);
                TClass result = new();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetUserStatisticsInterface
        public UserStatisticsInterface GetUserStatisticsInterface(int user, int pipe)
        {
            return this.GetIUserStatistics<UserStatisticsInterface>(user, pipe, NativeInterfaceVersions.UserStats);
        }
        #endregion

        #region GetICallbackUtilities
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetICallbackUtilities(IntPtr self, int pipe, IntPtr version);

        private TClass GetICallbackUtilities<TClass>(int pipe, string version)
            where TClass : INativeInterfaceWrapper, new()
        {
            using (var nativeVersion = NativeStringMarshaller.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetICallbackUtilities>(
                    this.Functions.GetICallbackUtilities,
                    this.ObjectAddress,
                    pipe,
                    nativeVersion.Handle);
                TClass result = new();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetCallbackUtilitiesInterface
        public CallbackUtilitiesInterface GetCallbackUtilitiesInterface(int pipe)
        {
            return this.GetICallbackUtilities<CallbackUtilitiesInterface>(pipe, NativeInterfaceVersions.Utils);
        }
        #endregion

        #region GetIInstalledApplicationCatalog
        private delegate IntPtr NativeGetIInstalledApplicationCatalog(int user, int pipe, IntPtr version);

        private TClass GetIInstalledApplicationCatalog<TClass>(int user, int pipe, string version)
            where TClass : INativeInterfaceWrapper, new()
        {
            using (var nativeVersion = NativeStringMarshaller.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetIInstalledApplicationCatalog>(
                    this.Functions.GetIInstalledApplicationCatalog,
                    user,
                    pipe,
                    nativeVersion.Handle);
                TClass result = new();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetApplicationDataInterface
        public ApplicationMetadataInterface GetApplicationDataInterface(int user, int pipe)
        {
            return this.GetIInstalledApplicationCatalog<ApplicationMetadataInterface>(user, pipe, NativeInterfaceVersions.ApplicationData);
        }
        #endregion

        #region GetInstalledApplicationCatalogInterface
        public InstalledApplicationCatalogInterface GetInstalledApplicationCatalogInterface(int user, int pipe)
        {
            return this.GetIInstalledApplicationCatalog<InstalledApplicationCatalogInterface>(user, pipe, NativeInterfaceVersions.Apps);
        }
        #endregion
    }
}
