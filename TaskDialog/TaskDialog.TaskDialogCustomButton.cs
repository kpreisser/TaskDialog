using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private class TaskDialogCustomButton : TaskDialogButton, ITaskDialogCustomButton
        {
            private bool buttonElevationRequiredState = false;

            public TaskDialogCustomButton(TaskDialog origin, string text)
                : base(origin, text)
            {
            }

            
            public TaskDialogCustomButtonClickedDelegate ButtonClicked
            {
                get;
                set;
            }

            public bool ButtonElevationRequiredState
            {
                get => this.buttonElevationRequiredState;

                set
                {
                    // The TaskDialog will set this property on th Created/Navigated event.
                    if (TryVerifyState())
                        this.TaskDialog.SetButtonElevationRequiredStateCore(this.ButtonID.Value, value);
                    this.buttonElevationRequiredState = value;
                }
            }

            
            public override void Click()
            {
                VerifyState();
                this.TaskDialog.ClickButton(this.ButtonID.Value);
            }


            protected internal bool OnButtonClicked(EventArgs e)
            {
                return this.ButtonClicked?.Invoke(this, e) ?? true;
            }

            protected override void SetEnabledCore(bool enabled)
            {
                // The Task dialog will set this property on th Created/Navigated event.
                if (TryVerifyState())
                    this.TaskDialog.SetButtonEnabledCore(this.ButtonID.Value, enabled);
            }
        }
    }
}
