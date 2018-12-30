using System;
using System.Linq;

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
        /// Occurs when the value of the <see cref="Checked"/> property has changed
        /// while this control is bound to a task dialog.
        /// </summary>
        public event EventHandler CheckedChanged;


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
        public TaskDialogRadioButton(string text)
            : base()
        {
            this.text = text;
        }


        /// <summary>
        /// 
        /// </summary>
        public bool Enabled
        {
            get => this.enabled;

            set
            {
                this.DenyIfBoundAndNotCreatable();
                this.boundTaskDialogContents?.BoundTaskDialog.SetRadioButtonEnabled(
                        this.radioButtonID,
                        value);

                this.enabled = value;
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
                this.DenyIfBound();

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
                this.DenyIfBoundAndNotCreatable();

                // Unchecking a radio button is not possible in the task dialog.
                // TODO: Should we throw only if the new value is different than the
                // old one?
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
                    if (value && this.collection != null)
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


        internal void HandleRadioButtonClicked()
        {
            // First, uncheck the other radio buttons.
            foreach (var radioButton in this.boundTaskDialogContents.RadioButtons
                    .Where(e => e != this))
            {
                if (radioButton.@checked)
                {
                    radioButton.@checked = false;
                    radioButton.OnCheckedChanged(EventArgs.Empty);
                }
            }

            // Then, check the current radio button.
            if (!this.@checked)
            {
                this.@checked = true;
                this.OnCheckedChanged(EventArgs.Empty);
            }           
        }

        internal override void ApplyInitialization()
        {
            // Re-set the properties so they will make the necessary calls.
            if (!this.enabled)
                this.Enabled = this.enabled;
        }

        
        private void OnCheckedChanged(EventArgs e)
        {
            this.CheckedChanged?.Invoke(this, e);
        }
    }
}
