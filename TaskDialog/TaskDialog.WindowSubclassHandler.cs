using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private class WindowSubclassHandler : UI.WindowSubclassHandler
        {
            private readonly TaskDialog taskDialog;


            public WindowSubclassHandler(TaskDialog taskDialog)
                : base(taskDialog?.hwndDialog ?? throw new ArgumentNullException(nameof(taskDialog)))
            {
                this.taskDialog = taskDialog;
            }


            protected override IntPtr WndProc(int msg, IntPtr wParam, IntPtr lParam)
            {
                // Note: We handle WM_NCACTIVATE instead of WM_ACTIVATE as the latter
                // doesn't seem to be reliable. For example, when you show a TaskDialog
                // with flag TDF_NO_SET_FOREGROUND while no window of the the app
                // currently has focus, then when you activate the TaskDialog, it
                // doesn't get a WM_ACTIVATE message but it gets a WM_NCACTIVATE
                // message. Also, when you e.g. run the event loop in the TDN_CREATED
                // notification (while the TaskDialog window has focus), and during
                // that time you activate another window, then the TaskDialog gets a
                // WM_ACTIVATE message indicating the window is active even though it
                // isn't. However, a System.Windows.Forms apparently has the same
                // problem.
                if (msg == TaskDialogNativeMethods.WM_NCACTIVATE)
                {
                    bool active = ((long)wParam & 0xFFFF) != 0;
                    if (active && !this.taskDialog.isWindowActive)
                    {
                        this.taskDialog.isWindowActive = true;
                        this.taskDialog.OnActivated(EventArgs.Empty);
                    }
                    else if (!active && this.taskDialog.isWindowActive)
                    {
                        this.taskDialog.isWindowActive = false;
                        this.taskDialog.OnDeactivated(EventArgs.Empty);
                    }
                }

                return base.WndProc(msg, wParam, lParam);
            }
        }
    }
}
