using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private delegate int TaskDialogCallbackProcDelegate(
                IntPtr hWnd,
                TaskDialogNotification notification,
                IntPtr wParam,
                IntPtr lParam,
                IntPtr referenceData);
    }
}
