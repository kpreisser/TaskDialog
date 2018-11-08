using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{   
    public partial class TaskDialog
    {
        // Packing is defined as 1 in the C header file ("pack(1)").
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct TaskDialogConfig
        {
            public int cbSize;
            public IntPtr hwndParent;
            public IntPtr hInstance;
            public TaskDialogFlags dwFlags;
            public TaskDialogButtons dwCommonButtons;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszWindowTitle;
            public IntPtr hMainIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszMainInstruction;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszContent;
            public int cButtons;
            public IntPtr pButtons;
            public int nDefaultButton;
            public int cRadioButtons;
            public IntPtr pRadioButtons;
            public int nDefaultRadioButton;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszVerificationText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszExpandedInformation;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszExpandedControlText;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszCollapsedControlText;
            public IntPtr hFooterIcon;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string pszFooter;
            public IntPtr pfCallback;
            public IntPtr lpCallbackData;
            public int cxWidth;
        }
    }
}
