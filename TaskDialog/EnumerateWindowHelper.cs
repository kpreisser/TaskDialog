using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    internal static partial class EnumerateWindowHelper
    {
        private static readonly EnumChildProcDelegate callbackProcDelegate;

        private static readonly IntPtr callbackProcDelegatePtr;


        static EnumerateWindowHelper()
        {
            // Create a delegate for the callback, and get a function pointer for it.
            // Because this will allocate some memory required to store the native
            // code for the function pointer, we only do this once by using a static
            // function, and identify the actual object instance by using a
            // GCHandle in the reference data field.
            callbackProcDelegate = HandleEnumChildProcCallback;
            callbackProcDelegatePtr = Marshal.GetFunctionPointerForDelegate(
                    callbackProcDelegate);
        }


        public static void EnumerateChildWindows(IntPtr hWndParent, Func<IntPtr, bool> callback)
        {
            var handle = GCHandle.Alloc(callback);
            try
            {
                NativeMethods.EnumChildWindows(
                        hWndParent,
                        callbackProcDelegatePtr,
                        GCHandle.ToIntPtr(handle));
            }
            finally
            {
                handle.Free();
                GC.KeepAlive(callbackProcDelegate);
            }
        }


        private static bool HandleEnumChildProcCallback(IntPtr hWnd, IntPtr lParam)
        {
            var callback = (Func<IntPtr, bool>)GCHandle.FromIntPtr(lParam).Target;
            return callback(hWnd);
        }
    }
}
