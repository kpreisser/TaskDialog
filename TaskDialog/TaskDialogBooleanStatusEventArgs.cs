using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogBooleanStatusEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="status"></param>
        internal TaskDialogBooleanStatusEventArgs(bool status)
            : base()
        {
            this.Status = status;
        }

        
        /// <summary>
        /// 
        /// </summary>
        public bool Status
        {
            get;
        }
    }
}
