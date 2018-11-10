using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private abstract class TaskDialogButton : ITaskDialogButton
        {
            protected readonly TaskDialog taskDialog;


            private bool enabled = true;

            
            public TaskDialogButton(TaskDialog taskDialog, string text)
                : base()
            {
                this.taskDialog = taskDialog;
                this.Text = text;
            }
                        

            public string Text
            {
                get;
            }

            public int? ButtonID
            {
                get;
                set;
            }

            public bool Enabled
            {
                get => this.enabled;

                set
                {
                    SetEnabledCore(value);
                    this.enabled = value;
                }
            }

            
            public abstract void Click();

            
            protected void VerifyState()
            {
                if (!TryVerifyState())
                    throw new InvalidOperationException(
                            "This button is not part of an active task dialog.");
            }

            protected bool TryVerifyState()
            {
                return this.ButtonID.HasValue;
            }

            protected abstract void SetEnabledCore(bool enabled);
        }
    }
}
