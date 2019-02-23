using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        // Packing is defined as 1 in CommCtrl.h ("pack(1)").
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct TaskDialogButtonStruct
        {
            public int nButtonID;
            public IntPtr pszButtonText;
        }
    }
}
