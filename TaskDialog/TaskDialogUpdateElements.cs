using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [Flags]
    public enum TaskDialogUpdateElements
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Content = 1 << 0,

        /// <summary>
        /// 
        /// </summary>
        ExpandedInformation = 1 << 1,

        /// <summary>
        /// 
        /// </summary>
        Footer = 1 << 2,

        /// <summary>
        /// 
        /// </summary>
        MainInstruction = 1 << 3,

        /// <summary>
        /// 
        /// </summary>
        MainIcon = 1 << 4,

        /// <summary>
        /// 
        /// </summary>
        FooterIcon = 1 << 5
    }
}
