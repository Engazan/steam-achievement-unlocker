using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32;

namespace SteamIntegration
{
    internal static class NativeSteamRuntime
    {
        private struct Native
        {
            [DllImport("kernel32.dll", SetLastError = true, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            internal static extern IntPtr GetProcAddress(IntPtr module, string name);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            internal static extern IntPtr LoadLibraryEx(string path, IntPtr file, uint flags);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool SetDllDirectory(string path);

            internal const uint LoadWithAlteredSearchPath = 8;
        }

        private static Delegate GetExportDelegate<TDelegate>(IntPtr module, string name)
        {
            IntPtr address = Native.GetProcAddress(module, name);
            return address == IntPtr.Zero ? null : Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
        }

        private static TDelegate GetExportFunction<TDelegate>(IntPtr module, string name)
            where TDelegate : class
        {
            return (TDelegate)((object)GetExportDelegate<TDelegate>(module, name));
        }

        private static IntPtr _Handle = IntPtr.Zero;

        public static string GetInstallPath()
        {
            return (string)Registry.GetValue(@"HKEY_LOCAL_MACHINE\Software\Valve\Steam", "InstallPath", null);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
        private delegate IntPtr NativeCreateInterface(string version, IntPtr returnCode);

        private static NativeCreateInterface _CallCreateInterface;

        public static TClass CreateInterface<TClass>(string version)
            where TClass : INativeInterfaceWrapper, new()
        {
            IntPtr address = _CallCreateInterface(version, IntPtr.Zero);

            if (address == IntPtr.Zero)
            {
                return default;
            }

            TClass instance = new();
            instance.SetupFunctions(address);
            return instance;
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamGetCallback(int pipe, out Models.SteamCallbackMessage message, out int call);

        private static NativeSteamGetCallback _CallSteamBGetCallback;

        public static bool GetCallback(int pipe, out Models.SteamCallbackMessage message, out int call)
        {
            return _CallSteamBGetCallback(pipe, out message, out call);
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSteamFreeLastCallback(int pipe);

        private static NativeSteamFreeLastCallback _CallSteamFreeLastCallback;

        public static bool FreeLastCallback(int pipe)
        {
            return _CallSteamFreeLastCallback(pipe);
        }

        public static bool Load()
        {
            if (_Handle != IntPtr.Zero)
            {
                return true;
            }

            string path = GetInstallPath();
            if (path == null)
            {
                return false;
            }

            Native.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));

            path = Path.Combine(path, "steamclient.dll");
            IntPtr module = Native.LoadLibraryEx(path, IntPtr.Zero, Native.LoadWithAlteredSearchPath);
            if (module == IntPtr.Zero)
            {
                return false;
            }

            _CallCreateInterface = GetExportFunction<NativeCreateInterface>(module, "CreateInterface");
            if (_CallCreateInterface == null)
            {
                return false;
            }

            _CallSteamBGetCallback = GetExportFunction<NativeSteamGetCallback>(module, "Steam_BGetCallback");
            if (_CallSteamBGetCallback == null)
            {
                return false;
            }

            _CallSteamFreeLastCallback = GetExportFunction<NativeSteamFreeLastCallback>(module, "Steam_FreeLastCallback");
            if (_CallSteamFreeLastCallback == null)
            {
                return false;
            }

            _Handle = module;
            return true;
        }
    }
}
