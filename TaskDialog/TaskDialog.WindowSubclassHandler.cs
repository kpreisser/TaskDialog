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
                        // WM_[NC]ACTIVATE messages because that means we have already
                        // taken their viewpoint about the active state, so processing
                        // our message in that case might get the state out-of-sync.
                        if (!this.processedWmActivateMessage &&
                                !this.taskDialog.isWindowActive) {
                            
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
                        this.processedWmActivateMessage = true;

                        // Call the base window procedure before raising our events.
                        var result = base.WndProc(msg, wParam, lParam);

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

                        return result;

                    default:
                        return base.WndProc(msg, wParam, lParam);
                }
            }
        }
    }
}
