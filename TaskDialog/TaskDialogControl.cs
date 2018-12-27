using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogControl
    {
        private protected TaskDialogContents boundTaskDialogContents;


        // Disallow inheritance by specifying a private protected constructor.
        private protected TaskDialogControl()
            : base()
        {
        }


        /// <summary>
        /// Gets or sets the object that contains data about the control.
        /// </summary>
        public object Tag
        {
            get;
            set;
        }


        internal TaskDialogContents BoundTaskDialogContents
        {
            get => this.boundTaskDialogContents;
            set => this.boundTaskDialogContents = value;
        }

        /// <summary>
        /// When overridden in a subclass, applies initialization after the task dialog
        /// is displayed or navigated.
        /// </summary>
        internal virtual void ApplyInitialization()
        {
        }

        /// <summary>
        /// When overridden in a subclass, gets additional flags to be specified before
        /// the task dialog is displayed or navigated.
        /// </summary>
        /// <returns></returns>
        internal virtual TaskDialogFlags GetFlags()
        {
            return default;
        }


        private protected void DenyIfNotBound()
        {
            if (this.boundTaskDialogContents == null)
                throw new InvalidOperationException(
                        "This control is not currently bound to a task dialog.");
        }
    }
}
