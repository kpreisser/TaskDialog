using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private class TaskDialogCustomButton : TaskDialogButton, ITaskDialogCustomButton
        {
            private bool elevationRequired = false;


            public event EventHandler<TaskDialogCustomButtonClickedEventArgs> ButtonClicked;


            public TaskDialogCustomButton(
                    TaskDialog origin,
                    string text,
                    bool elevationRequired = false)
                : base(origin, text)
            {
                this.elevationRequired = elevationRequired;
            }

            
            public bool ElevationRequired
            {
                get => this.elevationRequired;

                set
                {
                    // The TaskDialog will set this property on th Created/Navigated event.
                    if (TryVerifyState())
                        this.taskDialog.SetButtonElevationRequiredStateCore(this.ButtonID.Value, value);
                    this.elevationRequired = value;
                }
            }

            
            public override void Click()
            {
                VerifyState();
                this.taskDialog.ClickButton(this.ButtonID.Value);
            }


            internal protected void OnButtonClicked(TaskDialogCustomButtonClickedEventArgs e)
            {
                this.ButtonClicked?.Invoke(this, e);
            }

            protected override void SetEnabledCore(bool enabled)
            {
                if (TryVerifyState())
                    this.taskDialog.SetButtonEnabledCore(this.ButtonID.Value, enabled);
            }
        }
    }
}
