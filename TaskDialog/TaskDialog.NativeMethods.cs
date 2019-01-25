using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        internal static class NativeMethods
        {
            [DllImport("comctl32.dll",
                    EntryPoint = "TaskDialogIndirect",
                    ExactSpelling = true,
                    SetLastError = true)]
            public static extern int TaskDialogIndirect(
                    IntPtr pTaskConfig,
                    [Out] out int pnButton,
                    [Out] out int pnRadioButton,
                    [MarshalAs(UnmanagedType.Bool), Out] out bool pfVerificationFlagChecked);

            [DllImport("user32.dll",
                    EntryPoint = "SendMessageW",
                    ExactSpelling = true, 
                    SetLastError = true)]
            public static extern IntPtr SendMessage(
                    IntPtr hWnd,
                    int message,
                    IntPtr wParam,
                    IntPtr lParam);

            [DllImport("user32",
                    CharSet = CharSet.Unicode,
                    EntryPoint = "SetWindowTextW",
                    ExactSpelling = true,
                    SetLastError = true)]
            public static extern bool SetWindowText(
                    IntPtr hWnd,
                    string lpString);

            [DllImport("user32",
                    EntryPoint = "GetClassNameW",
                    ExactSpelling = true,
                    SetLastError = true)]
            public static extern int GetClassName(
                    IntPtr hWnd,
                    IntPtr lpClassName,
                    int nMaxCount);
        }
    }
}
