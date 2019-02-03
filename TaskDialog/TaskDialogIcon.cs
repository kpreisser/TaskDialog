namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public enum TaskDialogIcon : int
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        Information = ushort.MaxValue - 2, // TD_INFORMATION_ICON

        /// <summary>
        /// 
        /// </summary>
        Warning = ushort.MaxValue, // TD_WARNING_ICON

        /// <summary>
        /// 
        /// </summary>
        Stop = ushort.MaxValue - 1, // TD_ERROR_ICON

        /// <summary>
        /// 
        /// </summary>
        SecurityShield = ushort.MaxValue - 3, // TD_SHIELD_ICON

        /// <summary>
        /// 
        /// </summary>
        SecurityShieldBlueBar = ushort.MaxValue - 4,

        /// <summary>
        /// 
        /// </summary>
        SecurityShieldGrayBar = ushort.MaxValue - 8,

        /// <summary>
        /// 
        /// </summary>
        SecurityWarningYellowBar = ushort.MaxValue - 5,

        /// <summary>
        /// 
        /// </summary>
        SecurityErrorRedBar = ushort.MaxValue - 6,

        /// <summary>
        /// 
        /// </summary>
        SecuritySuccessGreenBar = ushort.MaxValue - 7,

        //// TODO: Check if these "NoSound" icons should be included - 
        //// note that the Question icon only seems to be available without
        //// a sound.
        //// These icons are used from the system's resource module (imageres.dll)
        //// and can be used when specifying NULL in the TaskDialogConfig's hInstance
        //// field (which we currently always do).
        //// For more information, see:
        //// https://docs.microsoft.com/en-us/windows/desktop/Controls/tdm-update-icon
        //// Note: The 32xxx values are taken from WinUser.h (prefix "OIC_").

        ///// <summary>
        ///// 
        ///// </summary>
        //InformationNoSound = 32516, // OIC_INFORMATION

        ///// <summary>
        ///// 
        ///// </summary>
        //WarningNoSound = 32515, // OIC_WARNING

        ///// <summary>
        ///// 
        ///// </summary>
        //StopNoSound = 32513, // OIC_ERROR

        ///// <summary>
        ///// 
        ///// </summary>
        //QuestionNoSound = 32514, // OIC_QUES

        ///// <summary>
        ///// 
        ///// </summary>
        //SecurityQuestion = 104,

        ///// <summary>
        ///// 
        ///// </summary>
        //SecurityError = 105,

        ///// <summary>
        ///// 
        ///// </summary>
        //SecuritySuccess = 106,

        ///// <summary>
        ///// 
        ///// </summary>
        //SecurityWarning = 107
    }
}
