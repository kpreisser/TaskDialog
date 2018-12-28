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
        /// Occurs when the button is clicked.
        /// </summary>
        /// <remarks>
        /// By default, the dialog will be closed after the event handler returns 
        /// (except for the <see cref="TaskDialogResult.Help"/> button which instead
        /// will raise the <see cref="TaskDialogContents.Help"/> event afterwards).
        /// To prevent the dialog from closing, set the
        /// <see cref="TaskDialogButtonClickedEventArgs.CancelClose"/> property to
        /// <c>true</c>.
        /// </remarks>
        public event EventHandler<TaskDialogButtonClickedEventArgs> Click;


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
                {
                    this.boundTaskDialogContents?.BoundTaskDialog.SetButtonEnabled(
                            this.GetButtonID(),
                            value);
                }
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
                {
                    this.boundTaskDialogContents?.BoundTaskDialog.SetButtonElevationRequiredState(
                            this.GetButtonID(),
                            value);
                }
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
                    // Note that this does not handle buttons that are added later to
                    // the collection.
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
        /// Simulates a click on this button.
        /// </summary>
        public void PerformClick()
        {
            DenyIfNotBound();
            this.boundTaskDialogContents.BoundTaskDialog.ClickButton(this.GetButtonID());
        }


        internal bool HandleButtonClicked()
        {
            var e = new TaskDialogButtonClickedEventArgs();
            this.OnClick(e);
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


        private protected void OnClick(TaskDialogButtonClickedEventArgs e)
        {
            this.Click?.Invoke(this, e);
        }

        private protected abstract int GetButtonID();

        private protected virtual bool CanUpdate()
        {
            return true;
        }
    }
}
