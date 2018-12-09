namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogCommonButtonCollection 
        : TaskDialogButtonCollection<TaskDialogResult, TaskDialogCommonButton>
    {
        internal TaskDialogCommonButtonCollection()
            : base()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <returns></returns>
        public TaskDialogCommonButton Add(TaskDialogResult result)
        {
            var button = new TaskDialogCommonButton(result);
            this.Add(button);

            return button;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override TaskDialogResult GetKeyForItem(TaskDialogCommonButton item)
        {
            return item.Result;
        }
    }
}
