using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogTimerTickEventArgs : EventArgs
    {
        /// <summary>
        /// 
        /// </summary>
        internal TaskDialogTimerTickEventArgs(int tickCount)
            : base()
        {
            this.TickCount = tickCount;
        }


        /// <summary>
        /// Gets the number of milliseconds that have passed since the dialog was
        /// created/navigated or <see cref="ResetTickCount"/> was set to <c>true</c>.
        /// </summary>
        public int TickCount
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ResetTickCount
        {
            get;
            set;
        }
    }
}
