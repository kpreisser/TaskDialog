using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogExpander : TaskDialogControl
    {
        private string text;

        private string expandedButtonText;

        private string collapsedButtonText;

        private bool expandFooterArea;

        private bool expanded;


        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TaskDialogBooleanStatusEventArgs> ExpandoButtonClicked;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogExpander()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        public TaskDialogExpander(string text)
            : base()
        {
            this.text = text;
        }

        /// <summary>
        /// Gets or sets the text to be displayed in the dialog's expanded area.
        /// </summary>
        /// <remarks>
        /// This text can be changed while the dialog is shown.
        /// </remarks>
        public string Text
        {
            get => this.text;

            set {
                this.text = value;

                // Update the text if we are bound.
                this.boundTaskDialogContents?.BoundTaskDialog.UpdateTextElement(
                        TaskDialogTextElement.ExpandedInformation,
                        this.text);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExpandedButtonText
        {
            get => this.expandedButtonText;

            set
            {
                this.boundTaskDialogContents?.DenyIfBound();

                this.expandedButtonText = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string CollapsedButtonText
        {
            get => this.collapsedButtonText;

            set
            {
                this.boundTaskDialogContents?.DenyIfBound();

                this.collapsedButtonText = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Expanded
        {
            get => this.expanded;

            set
            {
                // The Task Dialog doesn't provide a message type to click the expando
                // button, so we don't allow to change this property (it will however
                // be updated when we receive an ExpandoButtonClicked notificatin).
                this.boundTaskDialogContents?.DenyIfBound();

                this.expanded = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public bool ExpandFooterArea
        {
            get => this.expandFooterArea;

            set
            {
                this.boundTaskDialogContents?.DenyIfBound();

                this.expandFooterArea = value;
            }
        }


        internal void HandleExpandoButtonClicked(bool expanded)
        {
            this.expanded = expanded;
            this.OnExpandoButtonClicked(
                    new TaskDialogBooleanStatusEventArgs(expanded));
        }

        internal override TaskDialogFlags GetFlags()
        {
            var flags = base.GetFlags();

            if (this.expanded)
                flags |= TaskDialogFlags.ExpandedByDefault;
            if (this.expandFooterArea)
                flags |= TaskDialogFlags.ExpandFooterArea;

            return flags;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        private void OnExpandoButtonClicked(
                TaskDialogBooleanStatusEventArgs e)
        {
            this.ExpandoButtonClicked?.Invoke(this, e);
        }
    }
}
