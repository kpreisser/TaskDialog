using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogHyperlinkClickedEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="hyperlink"></param>
        public TaskDialogHyperlinkClickedEventArgs(string hyperlink)
            : base()
        {
            this.Hyperlink = hyperlink;
        }

        
        /// <summary>
        /// 
        /// </summary>
        public string Hyperlink
        {
            get;
        }
    }
}
