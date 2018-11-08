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
        Content = 0x1,

        /// <summary>
        /// 
        /// </summary>
        ExpandedInformation = 0x2,

        /// <summary>
        /// 
        /// </summary>
        Footer = 0x4,

        /// <summary>
        /// 
        /// </summary>
        MainInstruction = 0x8,

        /// <summary>
        /// 
        /// </summary>
        MainIcon = 0x10,

        /// <summary>
        /// 
        /// </summary>
        FooterIcon = 0x20
    }
}
