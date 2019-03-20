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
        OK = TaskDialogNativeMethods.IDOK,

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note: Adding a Cancel button will automatically add a close button
        /// to the task dialog's title bar and will allow to close the dialog by
        /// pressing ESC or Alt+F4 (just as if you enabled
        /// <see cref="TaskDialogPage.AllowCancel"/>).
        /// </remarks>
        Cancel = TaskDialogNativeMethods.IDCANCEL,

        /// <summary>
        /// 
        /// </summary>
        Abort = TaskDialogNativeMethods.IDABORT,

        /// <summary>
        /// 
        /// </summary>
        Retry = TaskDialogNativeMethods.IDRETRY,

        /// <summary>
        /// 
        /// </summary>
        Ignore = TaskDialogNativeMethods.IDIGNORE,

        /// <summary>
        /// 
        /// </summary>
        Yes = TaskDialogNativeMethods.IDYES,

        /// <summary>
        /// 
        /// </summary>
        No = TaskDialogNativeMethods.IDNO,

        /// <summary>
        /// 
        /// </summary>
        Close = TaskDialogNativeMethods.IDCLOSE,

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note: Clicking this button will not close the dialog, but will raise the
        /// <see cref="TaskDialogPage.Help"/> event.
        /// </remarks>
        Help = TaskDialogNativeMethods.IDHELP,

        /// <summary>
        /// 
        /// </summary>
        TryAgain = TaskDialogNativeMethods.IDTRYAGAIN,

        /// <summary>
        /// 
        /// </summary>
        Continue = TaskDialogNativeMethods.IDCONTINUE
    }
}
