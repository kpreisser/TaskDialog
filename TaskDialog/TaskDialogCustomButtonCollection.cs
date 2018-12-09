namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogCustomButtonCollection 
        : TaskDialogButtonCollection<TaskDialogCustomButton, TaskDialogCustomButton>
    {
        internal TaskDialogCustomButtonCollection()
            : base()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="descriptionText"></param>
        /// <returns></returns>
        public TaskDialogCustomButton Add(string text, string descriptionText = null)
        {
            var button = new TaskDialogCustomButton()
            {
                Text = text,
                DescriptionText = descriptionText
            };

            this.Add(button);
            return button;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override TaskDialogCustomButton GetKeyForItem(
                TaskDialogCustomButton item)
        {
            return item;
        }
    }
}
