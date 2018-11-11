﻿using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogCommonButtonClickedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        internal TaskDialogCommonButtonClickedEventArgs(TaskDialogResult button)
            : base()
        {
            this.Button = button;
        }

        
        /// <summary>
        /// The <see cref="TaskDialogResult"/> that was clicked.
        /// </summary>
        public TaskDialogResult Button
        {
            get;
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
