using System;
using System.Collections.Generic;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TaskDialogButton : TaskDialogControl
    {
        private bool enabled = true;

        private bool defaultButton;

        private bool elevationRequired;

        private IReadOnlyList<TaskDialogButton> collection;


        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TaskDialogButtonClickedEventArgs> ButtonClicked;


        // Disallow inheritance by specifying a private protected constructor.
        private protected TaskDialogButton()
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

                // Check if we can update the button.
                if (CanUpdate())
                    this.boundTaskDialogContents?.BoundTaskDialog.SetButtonEnabled(
                            this.GetButtonID(),
                            value);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ElevationRequired
        {
            get => this.elevationRequired;

            set
            {
                this.elevationRequired = value;
                
                if (CanUpdate())
                    this.boundTaskDialogContents?.BoundTaskDialog.SetButtonElevationRequiredState(
                            this.GetButtonID(),
                            value);
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates if this button will be the default button
        /// in the Task Dialog.
        /// </summary>
        public bool DefaultButton
        {
            get => this.defaultButton;

            set
            {
                this.defaultButton = value;

                if (this.collection != null)
                {
                    // When we are part of a collection, set the defaultButton value of
                    // all other buttons to False.
                    // Note that this does not handle buttons that are added later to the
                    // collection.
                    foreach (var button in this.collection)
                        button.defaultButton = button == this;
                }
            }
        }


        internal IReadOnlyList<TaskDialogButton> Collection
        {
            get => this.collection;
            set => this.collection = value;
        }


        /// <summary>
        /// 
        /// </summary>
        public void Click()
        {
            DenyIfNotBound();
            this.boundTaskDialogContents.BoundTaskDialog.ClickButton(this.GetButtonID());
        }


        internal bool HandleButtonClicked()
        {
            var e = new TaskDialogButtonClickedEventArgs();
            this.OnButtonClicked(e);
            return !e.CancelClose;
        }

        internal override void ApplyInitialization()
        {
            // Re-set the properties so they will make the necessary calls.
            if (!this.enabled)
                this.Enabled = this.enabled;
            if (this.elevationRequired)
                this.ElevationRequired = this.elevationRequired;
        }

        private protected void OnButtonClicked(TaskDialogButtonClickedEventArgs e)
        {
            this.ButtonClicked?.Invoke(this, e);
        }

        private protected abstract int GetButtonID();

        private protected virtual bool CanUpdate()
        {
            return true;
        }
    }
}
