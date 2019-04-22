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
                switch (msg)
                {
                    case CheckActiveWindowMessage:
                        // Check if the dialog was already activated before we subclassed
                        // the window, which means we don't get the WM_ACTIVATE message.
                        // However, because the function returns the active window at the
                        // current time instead of the current thread's point of view (as
                        // described by the processed messages), it could happen that e.g.
                        // the dialog was initially active, but got inactive before the
                        // call to GetForegroundWindow(), which would mean that even
                        // though we determined we are not active, we might later get an
                        // WM_ACTIVATE message indicating that the window is now inactive
                        // (and vice versa).
                        // Therefore, we need to maintain the current active state.
                        var foregroundWindowHandle = TaskDialogNativeMethods.GetForegroundWindow();
                        bool isActive = foregroundWindowHandle != IntPtr.Zero &&
                                foregroundWindowHandle == this.taskDialog.hwndDialog;

                        if (isActive && !this.taskDialog.isWindowActive)
                        {
                            this.taskDialog.isWindowActive = true;
                            this.taskDialog.OnActivated(EventArgs.Empty);
                        }

                        // Do not forward the message to the base class.
                        return IntPtr.Zero;

                    // Note: We handle WM_NCACTIVATE instead of WM_ACTIVATE as the latter
                    // doesn't seem to be reliable. For example, when you show a TaskDialog
                    // with flag TDF_NO_SET_FOREGROUND while no window of the the app
                    // currently has focus, then when you activate the TaskDialog, it
                    // doesn't get a WM_ACTIVATE message but it gets a WM_NCACTIVATE
                    // message (it probably got already a WM_ACTIVATE message specifying
                    // the window is active before we subclassed it). Also, when you e.g.
                    // run the event loop in the TDN_CREATED notification (while the
                    // TaskDialog window has focus), and during that time you activate
                    // another window, then the TaskDialog gets a WM_ACTIVATE message
                    // indicating the window is active even though it isn't. However, a
                    // System.Windows.Forms apparently has the same problem.
                    case TaskDialogNativeMethods.WM_NCACTIVATE:
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

                        break;
                }

                return base.WndProc(msg, wParam, lParam);
            }
        }
    }
}
