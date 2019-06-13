namespace KPreisser.UI
{
    internal class TaskDialogStandardIconContainer : TaskDialogIcon
    {
        public TaskDialogStandardIconContainer(TaskDialogStandardIcon icon)
            : base()
        {
            Icon = icon;
        }
        
        public TaskDialogStandardIcon Icon
        {
            get;
        }
    }
}
