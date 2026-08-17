using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SteamIntegration
{
    internal abstract class NativeInterfaceWrapper<TFunctionTable> : INativeInterfaceWrapper
    {
        protected IntPtr ObjectAddress;
        protected TFunctionTable Functions;

        public override string ToString()
        {
            return $"Steam Interface<{typeof(TFunctionTable)}> #{this.ObjectAddress.ToInt32():X8}";
        }

        public void SetupFunctions(IntPtr objectAddress)
        {
            this.ObjectAddress = objectAddress;

            var iface = (NativeObjectLayout)Marshal.PtrToStructure(
                this.ObjectAddress,
                typeof(NativeObjectLayout));

            this.Functions = (TFunctionTable)Marshal.PtrToStructure(
                iface.VirtualTable,
                typeof(TFunctionTable));
        }

        private readonly Dictionary<IntPtr, Delegate> _FunctionCache = new();

        protected Delegate GetDelegate<TDelegate>(IntPtr pointer)
        {
            if (this._FunctionCache.TryGetValue(pointer, out var function) == false)
            {
                function = Marshal.GetDelegateForFunctionPointer(pointer, typeof(TDelegate));
                this._FunctionCache[pointer] = function;
            }
            return function;
        }

        protected TDelegate GetFunction<TDelegate>(IntPtr pointer)
            where TDelegate : class
        {
            return (TDelegate)((object)this.GetDelegate<TDelegate>(pointer));
        }

        protected void Call<TDelegate>(IntPtr pointer, params object[] args)
        {
            this.GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
        }

        protected TReturn Call<TReturn, TDelegate>(IntPtr pointer, params object[] args)
        {
            return (TReturn)this.GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
        }
    }
}
