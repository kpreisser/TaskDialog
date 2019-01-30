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
        }

        /// <summary>
        /// Gets a value that indicates if this control can be created in a
        /// task dialog.
        /// </summary>
        internal virtual bool IsCreatable
        {
            get => true;
        }

        /// <summary>
        /// Gets or sets a value that indicates if this control was created in
        /// a task dialog.
        /// </summary>
        internal bool IsCreated
        {
            get;
            set;
        }


        internal TaskDialogFlags Bind(TaskDialogContents contents)
        {
            this.boundTaskDialogContents = contents ??
                    throw new ArgumentNullException(nameof(contents));

            // Use the current value of IsCreatable to determine if the control is
            // created. This is important because IsCreatable can change while the
            // control is displayed (e.g. if it depends on the Text property).
            this.IsCreated = this.IsCreatable;

            return this.IsCreated ? this.GetFlagsCore() : default;
        }

        internal virtual void Unbind()
        {
            this.IsCreated = false;
            this.boundTaskDialogContents = null;            
        }

        /// <summary>
        /// Applies initialization after the task dialog is displayed or navigated.
        /// </summary>
        internal void ApplyInitialization()
        {
            // Only apply the initialization if the control is actually created.
            if (this.IsCreated)
                this.ApplyInitializationCore();
        }


        /// <summary>
        /// When overridden in a subclass, gets additional flags to be specified before
        /// the task dialog is displayed or navigated.
        /// </summary>
        /// <remarks>
        /// This method will only be called if <see cref="IsCreatable"/> returns <c>true</c>.
        /// </remarks>
        /// <returns></returns>
        private protected virtual TaskDialogFlags GetFlagsCore()
        {
            return default;
        }

        /// <summary>
        /// When overridden in a subclass, applies initialization after the task dialog
        /// is displayed or navigated.
        /// </summary>
        /// <remarks>
        /// This method will only be called if <see cref="IsCreatable"/> returns <c>true</c>.
        /// </remarks>
        private protected virtual void ApplyInitializationCore()
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

        private protected void DenyIfBoundAndNotCreated()
        {
            if (this.boundTaskDialogContents != null && !this.IsCreated)
                throw new InvalidOperationException("The control has not been created.");
        }
    }
}
