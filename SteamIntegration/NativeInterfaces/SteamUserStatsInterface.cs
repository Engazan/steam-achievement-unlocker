using System;
using System.Runtime.InteropServices;
using SteamIntegration.InteropFunctionTables;

namespace SteamIntegration.SteamInterfaces
{
    internal class UserStatisticsInterface : NativeInterfaceWrapper<UserStatisticsFunctionTable>
    {
        #region GetIntegerStat
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetStatInt(IntPtr self, IntPtr name, out int data);

        public bool GetIntegerStat(string name, out int value)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetStatInt>(this.Functions.GetStatInteger);
                return call(this.ObjectAddress, nativeName.Handle, out value);
            }
        }
        #endregion

        #region GetFloatStat
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetStatFloat(IntPtr self, IntPtr name, out float data);

        public bool GetFloatStat(string name, out float value)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetStatFloat>(this.Functions.GetStatFloat);
                return call(this.ObjectAddress, nativeName.Handle, out value);
            }
        }
        #endregion

        #region SetIntegerStat
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetStatInt(IntPtr self, IntPtr name, int data);

        public bool SetIntegerStat(string name, int value)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                return this.Call<bool, NativeSetStatInt>(
                    this.Functions.SetStatInteger,
                    this.ObjectAddress,
                    nativeName.Handle,
                    value);
            }
        }
        #endregion

        #region SetFloatStat
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetStatFloat(IntPtr self, IntPtr name, float data);

        public bool SetFloatStat(string name, float value)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                return this.Call<bool, NativeSetStatFloat>(
                    this.Functions.SetStatFloat,
                    this.ObjectAddress,
                    nativeName.Handle,
                    value);
            }
        }
        #endregion

        #region GetAchievementState
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetAchievement(
            IntPtr self,
            IntPtr name,
            [MarshalAs(UnmanagedType.I1)] out bool isAchieved);

        public bool GetAchievementState(string name, out bool isAchieved)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetAchievement>(this.Functions.GetAchievement);
                return call(this.ObjectAddress, nativeName.Handle, out isAchieved);
            }
        }
        #endregion

        #region SetAchievementState
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetAchievement(IntPtr self, IntPtr name);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeClearAchievement(IntPtr self, IntPtr name);

        public bool SetAchievementState(string name, bool state)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                if (state == false)
                {
                    return this.Call<bool, NativeClearAchievement>(
                        this.Functions.ClearAchievement,
                        this.ObjectAddress,
                        nativeName.Handle);
                }

                return this.Call<bool, NativeSetAchievement>(
                    this.Functions.SetAchievement,
                    this.ObjectAddress,
                    nativeName.Handle);
            }
        }
        #endregion

        #region GetAchievementStateAndUnlockTime
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetAchievementAndUnlockTime(
            IntPtr self,
            IntPtr name,
            [MarshalAs(UnmanagedType.I1)] out bool isAchieved,
            out uint unlockTime);

        public bool GetAchievementStateAndUnlockTime(string name, out bool isAchieved, out uint unlockTime)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetAchievementAndUnlockTime>(this.Functions.GetAchievementAndUnlockTime);
                return call(this.ObjectAddress, nativeName.Handle, out isAchieved, out unlockTime);
            }
        }
        #endregion

        #region StoreStats
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeStoreStats(IntPtr self);

        public bool StoreStats()
        {
            return this.Call<bool, NativeStoreStats>(this.Functions.StoreStats, this.ObjectAddress);
        }
        #endregion

        #region GetAchievementIconHandle
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeGetAchievementIcon(IntPtr self, IntPtr name);

        public int GetAchievementIconHandle(string name)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                return this.Call<int, NativeGetAchievementIcon>(
                    this.Functions.GetAchievementIcon,
                    this.ObjectAddress,
                    nativeName.Handle);
            }
        }
        #endregion

        #region GetAchievementDisplayAttribute
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetAchievementDisplayAttribute(IntPtr self, IntPtr name, IntPtr key);

        public string GetAchievementDisplayAttribute(string name, string key)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            using (var nativeKey = NativeStringMarshaller.StringToStringHandle(key))
            {
                var result = this.Call<IntPtr, NativeGetAchievementDisplayAttribute>(
                    this.Functions.GetAchievementDisplayAttribute,
                    this.ObjectAddress,
                    nativeName.Handle,
                    nativeKey.Handle);
                return NativeStringMarshaller.PointerToString(result);
            }
        }
        #endregion

        #region RequestUserStats
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate SteamCallHandle NativeRequestUserStats(IntPtr self, ulong steamIdUser);

        public SteamCallHandle RequestUserStats(ulong steamIdUser)
        {
            return this.Call<SteamCallHandle, NativeRequestUserStats>(this.Functions.RequestUserStats, this.ObjectAddress, steamIdUser);
        }
        #endregion

        #region ResetAllStats
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeResetAllStats(IntPtr self, [MarshalAs(UnmanagedType.I1)] bool achievementsToo);

        public bool ResetAllStats(bool includeAchievements)
        {
            return this.Call<bool, NativeResetAllStats>(
                this.Functions.ResetAllStats,
                this.ObjectAddress,
                includeAchievements);
        }
        #endregion

        #region Global achievement percentages
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate SteamCallHandle NativeRequestGlobalAchievementPercentages(IntPtr self);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetAchievementAchievedPercent(
            IntPtr self,
            IntPtr name,
            out float percent);

        public SteamCallHandle RequestGlobalAchievementPercentages()
        {
            return this.Call<SteamCallHandle, NativeRequestGlobalAchievementPercentages>(
                this.Functions.RequestGlobalAchievementPercentages,
                this.ObjectAddress);
        }

        public bool GetGlobalAchievementUnlockPercentage(string name, out float percent)
        {
            using (var nativeName = NativeStringMarshaller.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetAchievementAchievedPercent>(
                    this.Functions.GetAchievementAchievedPercent);
                return call(this.ObjectAddress, nativeName.Handle, out percent);
            }
        }
        #endregion
    }
}
