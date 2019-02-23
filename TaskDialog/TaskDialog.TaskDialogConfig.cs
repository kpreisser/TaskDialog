using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{   
    public partial class TaskDialog
    {
        // Packing is defined as 1 in CommCtrl.h ("pack(1)").
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TaskDialogConfig
        {
            public int cbSize;
            public IntPtr hwndParent;
            public IntPtr hInstance;
            public TaskDialogFlags dwFlags;
            public TaskDialogButtons dwCommonButtons;
            public IntPtr pszWindowTitle;
            public IntPtr mainIconUnion;
            public IntPtr pszMainInstruction;
            public IntPtr pszContent;
            public int cButtons;
            public IntPtr pButtons;
            public int nDefaultButton;
            public int cRadioButtons;
            public IntPtr pRadioButtons;
            public int nDefaultRadioButton;
            public IntPtr pszVerificationText;
            public IntPtr pszExpandedInformation;
            public IntPtr pszExpandedControlText;
            public IntPtr pszCollapsedControlText;
            public IntPtr footerIconUnion;
            public IntPtr pszFooter;
            public IntPtr pfCallback;
            public IntPtr lpCallbackData;
            public int cxWidth;
        }
    }
}
