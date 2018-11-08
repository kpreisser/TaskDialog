using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private delegate int TaskDialogCallbackProcDelegate(
                IntPtr hWnd,
                TaskDialogNotifications notification,
                IntPtr wparam,
                IntPtr lparam,
                IntPtr referenceData);
    }
}
