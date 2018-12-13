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
        Information = ushort.MaxValue - 2,

        /// <summary>
        /// 
        /// </summary>
        Warning = ushort.MaxValue,

        /// <summary>
        /// 
        /// </summary>
        Stop = ushort.MaxValue - 1,

        /// <summary>
        /// 
        /// </summary>
        SecurityShield = ushort.MaxValue - 3,

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

        // TODO: Check if these "NoSound" icons should be included - 
        // note that the Question icon only seems to be available without
        // a sound.

        ///// <summary>
        ///// 
        ///// </summary>
        //InformationNoSound = 81,

        ///// <summary>
        ///// 
        ///// </summary>
        //WarningNoSound = 84,

        ///// <summary>
        ///// 
        ///// </summary>
        //StopNoSound = 98,

        ///// <summary>
        ///// 
        ///// </summary>
        //QuestionNoSound = 99,

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
