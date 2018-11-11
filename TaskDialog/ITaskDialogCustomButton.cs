using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITaskDialogCustomButton : ITaskDialogButton
    {
        /// <summary>
        /// Occurs when this custom button has been clicked.
        /// </summary>
        event EventHandler<TaskDialogCustomButtonClickedEventArgs> ButtonClicked;

        /// <summary>
        /// Gets or sets a value that indicates if the UAC shield symbol should be
        /// displayed for this custom button.
        /// </summary>
        bool ElevationRequired
        {
            get;
            set;
        }
    }
}
