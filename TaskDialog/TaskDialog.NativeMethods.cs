using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private static class NativeMethods
        {
            [DllImport("comctl32.dll", CharSet = CharSet.Unicode, EntryPoint = "TaskDialogIndirect", ExactSpelling = true, SetLastError = true)]
            public static extern int TaskDialogIndirect(
                    [In] ref TaskDialogConfig pTaskConfig,
                    [Out] out int pnButton,
                    [Out] out int pnRadioButton,
                    [MarshalAs(UnmanagedType.Bool), Out] out bool pfVerificationFlagChecked);

            [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW", ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr SendMessage(
                    IntPtr windowHandle,
                    int message,
                    IntPtr wparam,
                    IntPtr lparam);
        }
    }
}
