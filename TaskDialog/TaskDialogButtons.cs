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
        OK = 0x0001,

        /// <summary>
        /// 
        /// </summary>
        Yes = 0x0002,

        /// <summary>
        /// 
        /// </summary>
        No = 0x0004,

        /// <summary>
        /// 
        /// </summary>
        Cancel = 0x0008,

        /// <summary>
        /// 
        /// </summary>
        Retry = 0x0010,

        /// <summary>
        /// 
        /// </summary>
        Close = 0x0020
    }
}
