using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum TaskDialogButtons : int
    {
        /// <summary>
        /// 
        /// </summary>
        OK = 1 << 0,

        /// <summary>
        /// 
        /// </summary>
        Yes = 1 << 1,

        /// <summary>
        /// 
        /// </summary>
        No = 1 << 2,

        /// <summary>
        /// 
        /// </summary>
        Cancel = 1 << 3,

        /// <summary>
        /// 
        /// </summary>
        Retry = 1 << 4,

        /// <summary>
        /// 
        /// </summary>
        Close = 1 << 5
    }
}
