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
        public string Text
        {
            get => this.text;
            
            set
            {
                // TODO: Maybe not throw here
                this.boundTaskDialogContents?.DenyIfBound();

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
                // TODO: Maybe not throw here
                this.boundTaskDialogContents?.DenyIfBound();

                this.descriptionText = value;
            }
        }


        internal int ButtonID
        {
            get => this.buttonID;
            set => this.buttonID = value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text;
        }


        internal string GetResultingText()
        {
            var contents = this.boundTaskDialogContents;

            var text = this.text;
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
