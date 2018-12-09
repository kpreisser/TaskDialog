using System.Collections.ObjectModel;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TaskDialogControlCollection<TKey, TItem>
        : KeyedCollection<TKey, TItem>
        where TItem : TaskDialogControl
    {
        private TaskDialogContents boundTaskDialogContents;


        private protected TaskDialogControlCollection()
            : base()
        {
        }


        internal TaskDialogContents BoundTaskDialogContents
        {
            get => this.boundTaskDialogContents;
            set => this.boundTaskDialogContents = value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, TItem item)
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();

            base.SetItem(index, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, TItem item)
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();

            base.InsertItem(index, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();

            base.RemoveItem(index);
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ClearItems()
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();

            base.ClearItems();
        }
    }
}
