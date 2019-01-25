using System;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogCustomButton : TaskDialogButton
    {
        private string text;

        private string descriptionText;

        private int buttonID;

        private IntPtr handle;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogCustomButton()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public TaskDialogCustomButton(string text, string descriptionText = null)
            : base()
        {
            this.text = text;
            this.descriptionText = descriptionText;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// When updating the text while the task dialog is shown, you should not
        /// change the mnemonic in the text because the change will not be
        /// reflected in the dialog.
        /// </remarks>
        public string Text
        {
            get => this.text;
            
            set
            {
                // We can update the text if we are bound and have a handle.
                if (this.boundTaskDialogContents != null)
                {
                    if (this.handle == IntPtr.Zero)
                        this.DenyIfBound();

                    this.boundTaskDialogContents.BoundTaskDialog.UpdateControlText(
                            this.handle,
                            value);
                }

                this.text = value;
            }
        }

        /// <summary>
        /// Gets or sets an additional description text that will be displayed in
        /// a separate line of the command link when
        /// <see cref="TaskDialogContents.CommandLinkMode"/> is set to
        /// <see cref="TaskDialogCommandLinkMode.CommandLinks"/> or
        /// <see cref="TaskDialogCommandLinkMode.CommandLinksNoIcon"/>.
        /// </summary>
        public string DescriptionText
        {
            get => this.descriptionText;

            set
            {
                // We can update the text if we are bound and have a handle.
                if (this.boundTaskDialogContents != null)
                {
                    if (this.handle == IntPtr.Zero)
                        this.DenyIfBound();

                    this.boundTaskDialogContents.BoundTaskDialog.UpdateCommandLinkDescription(
                            this.handle,
                            value);
                }

                this.descriptionText = value;
            }
        }


        internal int ButtonID
        {
            get => this.buttonID;
            set => this.buttonID = value;
        }

        internal IntPtr Handle
        {
            get => this.handle;
            set => this.handle = value;
        }

        internal override bool IsCreatable
        {
            get => base.IsCreatable && !TaskDialogContents.IsNativeStringNullOrEmpty(this.text);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text ?? base.ToString();
        }


        internal string GetResultingText()
        {
            var contents = this.boundTaskDialogContents;

            // Remove LFs from the text. Otherwise, the behavior would not
            // be consistent because when calling the dialog, the first LF separates
            // the normal control text from the description button text, but this is
            // not the case when calling SetWindowText().
            // Therefore, we replace a combined CR+LF with CR, and then also single
            // LFs with CR, because CR is treated as a line break.
            string text = this.text?.Replace("\r\n", "\r").Replace("\n", "\r");

            if ((contents?.CommandLinkMode == TaskDialogCommandLinkMode.CommandLinks ||
                    contents?.CommandLinkMode == TaskDialogCommandLinkMode.CommandLinksNoIcon) && 
                    text != null && this.descriptionText != null)
                text += '\n' + this.descriptionText;

            return text;
        }


        private protected override int GetButtonID()
        {
            return this.buttonID;
        }
    }
}
