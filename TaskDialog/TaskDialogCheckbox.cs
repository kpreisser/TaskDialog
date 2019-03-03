using System;
using System.ComponentModel;

using TaskDialogFlags = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_FLAGS;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class TaskDialogCheckBox : TaskDialogControl
    {
        private string text;

        private bool @checked;


        /// <summary>
        /// 
        /// </summary>
        public event EventHandler CheckedChanged;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogCheckBox()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public TaskDialogCheckBox(string text)
            : this()
        {
            this.text = text;
        }


        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get => this.text;

            set {
                this.DenyIfBound();

                this.text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This property can be set while the dialog is shown.
        /// </remarks>
        public bool Checked
        {
            get => this.@checked;

            set
            {
                this.DenyIfBoundAndNotCreated();

                if (this.BoundTaskDialogContents == null)
                {
                    this.@checked = value;
                }
                else
                {
                    // Click the checkbox which should cause a call to
                    // HandleCheckBoxClicked(), where we will update the checked
                    // state.
                    this.BoundTaskDialogContents.BoundTaskDialog.ClickCheckBox(
                            value);
                }
            }
        }


        internal override bool IsCreatable
        {
            get => base.IsCreatable && !TaskDialogContents.IsNativeStringNullOrEmpty(this.text);
        }


        /// <summary>
        /// 
        /// </summary>
        public void Focus()
        {
            this.DenyIfNotBound();
            this.DenyIfBoundAndNotCreated();

            this.BoundTaskDialogContents.BoundTaskDialog.ClickCheckBox(
                    this.@checked,
                    true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text ?? base.ToString();
        }


        internal void HandleCheckBoxClicked(bool @checked)
        {
            // Only raise the event if the state actually changed.
            if (@checked != this.@checked)
            {
                this.@checked = @checked;
                this.OnCheckedChanged(EventArgs.Empty);
            }
        }


        private protected override TaskDialogFlags GetFlagsCore()
        {
            var flags = base.GetFlagsCore();

            if (this.@checked)
                flags |= TaskDialogFlags.TDF_VERIFICATION_FLAG_CHECKED;

            return flags;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnCheckedChanged(EventArgs e)
        {
            this.CheckedChanged?.Invoke(this, e);
        }
    }
}
