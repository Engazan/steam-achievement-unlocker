using System;
using System.Runtime.InteropServices;
using SteamIntegration.InteropFunctionTables;

namespace SteamIntegration.SteamInterfaces
{
    internal class CallbackUtilitiesInterface : NativeInterfaceWrapper<CallbackUtilitiesFunctionTable>
    {
        #region GetConnectedUniverse
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeGetConnectedUniverse(IntPtr self);

        public int GetConnectedUniverse()
        {
            return this.Call<int, NativeGetConnectedUniverse>(this.Functions.GetConnectedUniverse, this.ObjectAddress);
        }
        #endregion

        #region GetIPCountry
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetIPCountry(IntPtr self);

        public string GetIPCountry()
        {
            var result = this.Call<IntPtr, NativeGetIPCountry>(this.Functions.GetIPCountry, this.ObjectAddress);
            return NativeStringMarshaller.PointerToString(result);
        }
        #endregion

        #region GetImageSize
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetImageSize(IntPtr self, int index, out int width, out int height);

        public bool GetImageSize(int index, out int width, out int height)
        {
            var call = this.GetFunction<NativeGetImageSize>(this.Functions.GetImageSize);
            return call(this.ObjectAddress, index, out width, out height);
        }
        #endregion

        #region GetImageRGBA
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetImageRGBA(IntPtr self, int index, byte[] buffer, int length);

        public bool GetImageRGBA(int index, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            var call = this.GetFunction<NativeGetImageRGBA>(this.Functions.GetImageRGBA);
            return call(this.ObjectAddress, index, data, data.Length);
        }
        #endregion

        #region GetAppID
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate uint NativeGetAppId(IntPtr self);

        public uint GetCurrentAppId()
        {
            return this.Call<uint, NativeGetAppId>(this.Functions.GetAppID, this.ObjectAddress);
        }
        #endregion

        #region API call results
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeIsApiCallCompleted(
            IntPtr self,
            SteamCallHandle callHandle,
            [MarshalAs(UnmanagedType.I1)] out bool failed);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetApiCallResult(
            IntPtr self,
            SteamCallHandle callHandle,
            IntPtr callback,
            int callbackSize,
            int expectedCallbackId,
            [MarshalAs(UnmanagedType.I1)] out bool failed);

        public bool IsApiCallCompleted(SteamCallHandle callHandle, out bool failed)
        {
            var call = this.GetFunction<NativeIsApiCallCompleted>(this.Functions.IsAPICallCompleted);
            return call(this.ObjectAddress, callHandle, out failed);
        }

        public bool GetApiCallResult<T>(
            SteamCallHandle callHandle,
            int expectedCallbackId,
            out T result,
            out bool failed)
            where T : struct
        {
            int callbackSize = Marshal.SizeOf(typeof(T));
            IntPtr callback = Marshal.AllocHGlobal(callbackSize);
            try
            {
                var call = this.GetFunction<NativeGetApiCallResult>(this.Functions.GetAPICallResult);
                bool succeeded = call(
                    this.ObjectAddress,
                    callHandle,
                    callback,
                    callbackSize,
                    expectedCallbackId,
                    out failed);

                result = succeeded
                    ? (T)Marshal.PtrToStructure(callback, typeof(T))
                    : default;
                return succeeded;
            }
            finally
            {
                Marshal.FreeHGlobal(callback);
            }
        }
        #endregion
    }
}
