namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITaskDialogButton
    {
        /// <summary>
        /// 
        /// </summary>
        string Text
        {
            get;
        }

        /// <summary>
        /// 
        /// </summary>
        bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        void Click();
    }
}
