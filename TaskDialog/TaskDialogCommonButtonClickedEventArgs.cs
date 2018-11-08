using System;

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
        public TaskDialogCommonButtonClickedEventArgs(TaskDialogResult button)
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
    }
}
