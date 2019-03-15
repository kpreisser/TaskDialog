using System;
using System.ComponentModel;

using TaskDialogFlags = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_FLAGS;
using TaskDialogTextElement = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_ELEMENTS;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
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
        public event EventHandler ExpandedChanged;


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
            : this()
        {
            this.text = text;
        }


        /// <summary>
        /// Gets or sets the text to be displayed in the dialog's expanded area.
        /// </summary>
        /// <remarks>
        /// This property can be set while the dialog is shown.
        /// </remarks>
        public string Text
        {
            get => this.text;

            set
            {
                this.DenyIfBoundAndNotCreated();

                // Update the text if we are bound.
                this.BoundPage?.BoundTaskDialog.UpdateTextElement(
                        TaskDialogTextElement.TDE_EXPANDED_INFORMATION,
                        value);

                this.text = value;
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
                this.DenyIfBound();

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
                this.DenyIfBound();

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
                // be updated when we receive an ExpandoButtonClicked notification).
                // TODO: Should we throw only if the new value is different than the
                // old one?
                this.DenyIfBound();

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
                this.DenyIfBound();

                this.expandFooterArea = value;
            }
        }


        internal override bool IsCreatable
        {
            get => base.IsCreatable && !TaskDialogPage.IsNativeStringNullOrEmpty(this.text);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.text ?? base.ToString();
        }


        internal void HandleExpandoButtonClicked(bool expanded)
        {
            this.expanded = expanded;
            this.OnExpandedChanged(EventArgs.Empty);
        }


        private protected override TaskDialogFlags BindCore()
        {
            var flags = base.BindCore();

            if (this.expanded)
                flags |= TaskDialogFlags.TDF_EXPANDED_BY_DEFAULT;
            if (this.expandFooterArea)
                flags |= TaskDialogFlags.TDF_EXPAND_FOOTER_AREA;

            return flags;
        }


        private void OnExpandedChanged(EventArgs e)
        {
            this.ExpandedChanged?.Invoke(this, e);
        }
    }
}
