using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogButtonClickedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        internal TaskDialogButtonClickedEventArgs()
            : base()
        {
        }


        /// <summary>
        /// Gets or sets a value that indicates if the dialog should not be closed after
        /// the event handler returns.
        /// </summary>
        public bool CancelClose
        {
            get;
            set;
        }
    }
}
