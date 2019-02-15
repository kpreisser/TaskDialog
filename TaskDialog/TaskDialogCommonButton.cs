using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogCommonButton : TaskDialogButton
    {
        private TaskDialogResult result;

        private bool visible = true;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogCommonButton()
            // Use 'OK' by default instead of 'None' (which would not be a valid
            // common button).
            : this(TaskDialogResult.OK)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        public TaskDialogCommonButton(
                TaskDialogResult result)
            : base()
        {
            if (!IsValidCommonButton(result))
                throw new ArgumentException();

            this.result = result;
        }


        /// <summary>
        /// Gets or sets the <see cref="TaskDialogResult"/> which is represented by this
        /// <see cref="TaskDialogCommonButton"/>.
        /// </summary>
        public TaskDialogResult Result
        {
            get => this.result;

            set
            {
                this.DenyIfBound();

                if (!IsValidCommonButton(value))
                    throw new ArgumentException();

                // If we are part of a CommonButtonCollection, we must now notify it
                // that we changed our result.
                (this.Collection as TaskDialogCommonButtonCollection)?.HandleKeyChange(
                        this,
                        value);

                // If this was successful or we are not part of a collection,
                // we can now set the result.
                this.result = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if this <see cref="TaskDialogCommonButton"/>
        /// should be shown when displaying the Task Dialog.
        /// </summary>
        /// <remarks>
        /// Setting this to <c>false</c> allows you to still receive the
        /// <see cref="TaskDialogButton.Click"/> event (e.g. for the
        /// <see cref="TaskDialogResult.Cancel"/> button when
        /// <see cref="TaskDialogContents.AllowCancel"/> is set), or to call the
        /// <see cref="TaskDialogButton.PerformClick"/> method even if the button is not
        /// shown.
        /// </remarks>
        public bool Visible
        {
            get => this.visible;

            set
            {
                this.DenyIfBound();

                this.visible = value;
            }
        }


        internal override bool IsCreatable
        {
            get => base.IsCreatable && this.visible;
        }


        private static bool IsValidCommonButton(
                TaskDialogResult button)
        {
            return button > 0 &&
                    button <= TaskDialogResult.Continue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.result.ToString();
        }


        private protected override int GetButtonID()
        {
            return (int)this.result;
        }
    }
}
