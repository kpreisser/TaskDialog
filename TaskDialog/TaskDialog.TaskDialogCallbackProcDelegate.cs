using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int TaskDialogCallbackProcDelegate(
                IntPtr hWnd,
                TaskDialogNotification notification,
                IntPtr wParam,
                IntPtr lParam,
                IntPtr referenceData);
    }
}
