using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private class TaskDialogRadioButton : TaskDialogButton, ITaskDialogRadioButton
        {
            public event EventHandler RadioButtonClicked;


            public TaskDialogRadioButton(TaskDialog taskDialog, string text)
                : base(taskDialog, text)
            {
            }

            
            public override void Click()
            {
                VerifyState();
                this.TaskDialog.ClickRadioButton(this.ButtonID.Value);
            }

            public void OnRadioButtonClicked(EventArgs e)
            {
                this.RadioButtonClicked?.Invoke(this, e);
            }

            
            protected override void SetEnabledCore(bool enabled)
            {
                // The Task dialog will set this property on th Created/Navigated event.
                if (TryVerifyState())
                    this.TaskDialog.SetRadioButtonEnabled(this.ButtonID.Value, enabled);
            }
        }
    }
}
