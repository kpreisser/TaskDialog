namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private enum TaskDialogMessage : int
        {
            NavigatePage = UserMessageOffset + 101,

            ClickButton = UserMessageOffset + 102,

            SetMarqueeProgressBar = UserMessageOffset + 103,

            SetProgressBarState = UserMessageOffset + 104,

            SetProgressBarRange = UserMessageOffset + 105,

            SetProgressBarPosition = UserMessageOffset + 106,

            SetProgressBarMarquee = UserMessageOffset + 107,

            SetElementText = UserMessageOffset + 108,

            ClickRadioButton = UserMessageOffset + 110,

            EnableButton = UserMessageOffset + 111,

            EnableRadioButton = UserMessageOffset + 112,

            ClickVerification = UserMessageOffset + 113,

            UpdateElementText = UserMessageOffset + 114,

            SetButtonElevationRequiredState = UserMessageOffset + 115,

            UpdateIcon = UserMessageOffset + 116
        }
    }
}
