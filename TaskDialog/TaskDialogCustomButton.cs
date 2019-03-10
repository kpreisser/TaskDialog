using TaskDialogFlags = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_FLAGS;

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
            : this()
        {
            this.text = text;
            this.descriptionText = descriptionText;
        }


        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get => this.text;
            
            set
            {
                this.DenyIfBound();

                this.text = value;
            }
        }

        /// <summary>
        /// Gets or sets an additional description text that will be displayed in
        /// a separate line of the command link when
        /// <see cref="TaskDialogPage.CommandLinkMode"/> is set to
        /// <see cref="TaskDialogCommandLinkMode.CommandLinks"/> or
        /// <see cref="TaskDialogCommandLinkMode.CommandLinksNoIcon"/>.
        /// </summary>
        public string DescriptionText
        {
            get => this.descriptionText;

            set
            {
                this.DenyIfBound();

                this.descriptionText = value;
            }
        }


        internal override bool IsCreatable
        {
            get => base.IsCreatable && !TaskDialogPage.IsNativeStringNullOrEmpty(this.text);
        }

        internal override int ButtonID
        {
            get => this.buttonID;
        }

        internal new TaskDialogCustomButtonCollection Collection
        {
            get => (TaskDialogCustomButtonCollection)base.Collection;
            set => base.Collection = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text ?? base.ToString();
        }


        internal TaskDialogFlags Bind(TaskDialogPage page, int buttonID)
        {
            var result = this.Bind(page);
            this.buttonID = buttonID;

            return result;
        }

        internal string GetResultingText()
        {
            var page = this.BoundPage;

            // Remove LFs from the text. Otherwise, the dialog would display the
            // part of the text after the LF in the command link note, but for
            // this we have the "DescriptionText" property, so we should ensure that
            // there is not an discrepancy here and that the contents of the "Text"
            // property are not displayed in the command link note.
            // Therefore, we replace a combined CR+LF with CR, and then also single
            // LFs with CR, because CR is treated as a line break.
            string text = this.text?.Replace("\r\n", "\r").Replace("\n", "\r");

            if ((page?.CommandLinkMode == TaskDialogCommandLinkMode.CommandLinks ||
                    page?.CommandLinkMode == TaskDialogCommandLinkMode.CommandLinksNoIcon) && 
                    text != null && this.descriptionText != null)
                text += '\n' + this.descriptionText;

            return text;
        }


        private protected override void UnbindCore()
        {
            this.buttonID = 0;

            base.UnbindCore();
        }
    }
}
