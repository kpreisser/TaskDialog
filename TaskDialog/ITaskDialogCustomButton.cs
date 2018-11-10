namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITaskDialogCustomButton : ITaskDialogButton
    {
        /// <summary>
        /// 
        /// </summary>
        TaskDialogCustomButtonClickedDelegate ButtonClicked
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        bool ElevationRequired
        {
            get;
            set;
        }
    }
}
