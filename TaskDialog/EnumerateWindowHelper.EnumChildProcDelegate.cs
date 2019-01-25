using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    internal static partial class EnumerateWindowHelper
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool EnumChildProcDelegate(
              IntPtr hwnd,
              IntPtr lParam);
    }
}
