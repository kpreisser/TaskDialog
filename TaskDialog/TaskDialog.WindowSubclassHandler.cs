using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private class WindowSubclassHandler : UI.WindowSubclassHandler
        {
            private readonly TaskDialog taskDialog;


            public WindowSubclassHandler(TaskDialog taskDialog)
                : base(taskDialog?.Handle ?? throw new ArgumentNullException(nameof(taskDialog)))
            {
                this.taskDialog = taskDialog;
            }


            protected override IntPtr WndProc(int msg, IntPtr wParam, IntPtr lParam)
            {
                // TODO
                return base.WndProc(msg, wParam, lParam);
            }
        }
    }
}
