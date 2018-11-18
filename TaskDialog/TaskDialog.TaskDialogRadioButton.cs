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
                this.taskDialog.ClickRadioButtonCore(this.ButtonID.Value);
            }

            internal protected void OnRadioButtonClicked(EventArgs e)
            {
                this.RadioButtonClicked?.Invoke(this, e);
            }

            
            protected override void SetEnabledCore(bool enabled)
            {
                if (TryVerifyState())
                    this.taskDialog.SetRadioButtonEnabledCore(this.ButtonID.Value, enabled);
            }
        }
    }
}
