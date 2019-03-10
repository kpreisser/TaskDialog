namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public enum TaskDialogResult : int
    {
        /// <summary>
        /// 
        /// </summary>
        None = 0,

        /// <summary>
        /// 
        /// </summary>
        OK = 1,

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note: Adding a Cancel button will automatically add a close button
        /// to the task dialog's title bar and will allow to close the dialog by
        /// pressing ESC or Alt+F4 (just as if you enabled
        /// <see cref="TaskDialogPage.AllowCancel"/>).
        /// </remarks>
        Cancel = 2,

        /// <summary>
        /// 
        /// </summary>
        Abort = 3,

        /// <summary>
        /// 
        /// </summary>
        Retry = 4,

        /// <summary>
        /// 
        /// </summary>
        Ignore = 5,

        /// <summary>
        /// 
        /// </summary>
        Yes = 6,

        /// <summary>
        /// 
        /// </summary>
        No = 7,

        /// <summary>
        /// 
        /// </summary>
        Close = 8,

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note: Clicking this button will not close the dialog, but will raise the
        /// <see cref="TaskDialogPage.Help"/> event.
        /// </remarks>
        Help = 9,

        /// <summary>
        /// 
        /// </summary>
        TryAgain = 10,

        /// <summary>
        /// 
        /// </summary>
        Continue = 11
    }
}
