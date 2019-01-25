using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    internal static partial class EnumerateWindowHelper
    {
        private static class NativeMethods
        {
            [DllImport("user32",
                    EntryPoint = "EnumChildWindows",
                    ExactSpelling = true)]
            public static extern bool EnumChildWindows(
                  IntPtr hWndParent,
                  IntPtr lpEnumFunc,
                  IntPtr lParam);
        }
    }
}
