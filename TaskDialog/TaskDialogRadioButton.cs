using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogRadioButton : TaskDialogControl
    {
        private string text;

        private int radioButtonID;

        private bool enabled = true;

        private bool @checked;

        private TaskDialogRadioButtonCollection collection;


        /// <summary>
        /// 
        /// </summary>
        public event EventHandler RadioButtonClicked;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogRadioButton()
            : base()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        public bool Enabled
        {
            get => this.enabled;

            set
            {
                this.enabled = value;
                this.boundTaskDialogContents?.BoundTaskDialog.SetRadioButtonEnabled(
                        this.radioButtonID,
                        value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Text
        {
            get => this.text;

            set
            {
                this.boundTaskDialogContents?.DenyIfBound();

                this.text = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool Checked
        {
            get => this.@checked;

            set
            {
                // Unchecking a radio button is not possible in the task dialog.
                if (this.boundTaskDialogContents != null && !value)
                    throw new InvalidOperationException(
                            "Cannot uncheck a radio button while it is bound to a task dialog.");

                if (this.boundTaskDialogContents == null)
                {
                    this.@checked = value;

                    // If we are part of a collection, set the checked value of
                    // all other buttons to False.
                    // Note that this does not handle buttons that are added later to the
                    // collection.
                    if (this.collection != null)
                    {
                        foreach (var radioButton in this.collection)
                            radioButton.@checked = radioButton == this;
                    }
                }
                else
                {
                    // Click the radio button; this should raise the RadioButtonClicked
                    // notification where we will update the "checked" status of all
                    // radio buttons in the collection.
                    this.boundTaskDialogContents.BoundTaskDialog.ClickRadioButton(
                            this.radioButtonID);
                }
            }
        }


        internal int RadioButtonID
        {
            get => this.radioButtonID;
            set => this.radioButtonID = value;
        }

        internal TaskDialogRadioButtonCollection Collection
        {
            get => this.collection;
            set => this.collection = value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text;
        }


        internal void HandleRadioButtonClicked()
        {
            // Update the "checked" state of this and the other radio buttons.
            foreach (var radioButton in this.boundTaskDialogContents.RadioButtons)            
                radioButton.@checked = radioButton == this;

            this.OnRadioButtonClicked(EventArgs.Empty);            
        }

        internal override void ApplyInitialization()
        {
            // Re-set the properties so they will make the necessary calls.
            if (!this.enabled)
                this.Enabled = this.enabled;
        }


        private void OnRadioButtonClicked(EventArgs e)
        {
            this.RadioButtonClicked?.Invoke(this, e);
        }
    }
}
