using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private class WindowSubclassHandler : UI.WindowSubclassHandler
        {
            private readonly TaskDialog taskDialog;

            private bool processedWmActivateMessage;


            public WindowSubclassHandler(TaskDialog taskDialog)
                : base(taskDialog?.hwndDialog ?? throw new ArgumentNullException(nameof(taskDialog)))
            {
                this.taskDialog = taskDialog;
            }


            protected override IntPtr WndProc(int msg, IntPtr wParam, IntPtr lParam)
            {
                switch (msg)
                {
                    case HandleActiveWindowMessage:
                        // The task dialog callback determined that the window was
                        // initially active, so we need to raise the Activated event.
                        // However, we don't do the check if we already processed
                        // WM_ACTIVATE messages because that means we have already
                        // taken their viewpoint about the active state, so processing
                        // our message in that case might get the state out-of-sync.
                        if (!this.processedWmActivateMessage &&
                                !this.taskDialog.isWindowActive)
                        {
                            this.taskDialog.isWindowActive = true;
                            this.taskDialog.OnActivated(EventArgs.Empty);
                        }

                        // Do not forward the message to the base class.
                        return IntPtr.Zero;

                    // Note: We handle WM_ACTIVATE messages (like WinForms does) and
                    // use the corresponding GetActiveWindow() function to determine
                    // if we are initially active. However, apparently "foreground
                    // window" and "active window" are not the same: A window can be
                    // active even if it is not the foreground window (it doesn't have
                    // focus and its title bar doesn't have the "active" colors).
                    // 
                    // For example, when you show a TaskDialog with flag
                    // TDF_NO_SET_FOREGROUND while no window of the the app currently
                    // has focus, then the task dialog will not be the foreground window
                    // (it has an inactive title bar and doesn't get keyboard inputs)
                    // but GetActiveWindow() returns the task dialog window as active
                    // window (and it doesn't get a WM_ACTIVATE message when actually
                    // activating it, presumably because it initially received one
                    // before we subclassed it).
                    // This becomes even clearer when you run the message loop
                    // for a short time in the TDN_CREATED notification and during that
                    // time you activate a window from another app (which means the 
                    // task dialog window gets a WM_ACTIVATE message indicating it is
                    // deactivated), but once the notification handler returns and the
                    // window gets visible, it gets a WM_ACTIVATE message indicating
                    // it is now active although it doesn't have focus and isn't the
                    // foreground window. (This can also be seen with a WinForms Form.)
                    // 
                    // If we actually wanted to use the "foreground window" mechanism
                    // instead of the "active window" mechanism (so only we consider the
                    // window active when it has actually focus and has an active title
                    // bar), we would need to use GetForegroundWindow() instead of
                    // GetActiveWindow(), and handle WM_NCACTIVATE messages instead of
                    // WM_ACTIVATE messages.
                    case TaskDialogNativeMethods.WM_ACTIVATE:
                        this.processedWmActivateMessage = true;

                        // Call the base window procedure before raising our events.
                        var result = base.WndProc(msg, wParam, lParam);

                        bool active = ((long)wParam & 0xFFFF) !=
                                TaskDialogNativeMethods.WA_INACTIVE;
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

                        return result;

                    default:
                        return base.WndProc(msg, wParam, lParam);
                }
            }
        }
    }
}
