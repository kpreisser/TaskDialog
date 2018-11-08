namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        private enum TaskDialogNotifications : int
        {
            Created = 0,

            Navigated = 1,

            ButtonClicked = 2,

            HyperlinkClicked = 3,

            Timer = 4,

            Destroyed = 5,

            RadioButtonClicked = 6,

            Constructed = 7,

            VerificationClicked = 8,

            Help = 9,

            ExpandButtonClicked = 10
        }
    }
}
