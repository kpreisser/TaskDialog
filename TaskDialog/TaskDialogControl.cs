using System;
using System.ComponentModel;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
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
        [TypeConverter(typeof(StringConverter))]
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
        /// Returns a value that indicates if this control can be created in a
        /// task dialog.
        /// </summary>
        internal virtual bool IsCreatable
        {
            get => true;
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

        /// <summary>
        /// When overridden in a subclass, applies initialization after the task dialog
        /// is displayed or navigated.
        /// </summary>
        internal virtual void ApplyInitialization()
        {
        }


        private protected void DenyIfBound()
        {
            this.boundTaskDialogContents?.DenyIfBound();
        }

        private protected void DenyIfNotBound()
        {
            if (this.boundTaskDialogContents == null)
                throw new InvalidOperationException(
                        "This control is not currently bound to a task dialog.");
        }

        private protected void DenyIfBoundAndNotCreatable()
        {
            if (this.boundTaskDialogContents != null && !this.IsCreatable)
                throw new InvalidOperationException("The control has not been created.");
        }
    }
}
