using System;
using System.Collections.Generic;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TaskDialogButtonCollection<TKey, TItem>
        : TaskDialogControlCollection<TKey, TItem>
        where TItem : TaskDialogButton
    {
        private protected TaskDialogButtonCollection()
            : base()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, TItem item)
        {
            DenyIfHasOtherCollection(item);

            // Call the base method first which will throw if the collection is
            // already bound.
            var oldItem = this[index];
            base.SetItem(index, item);

            oldItem.Collection = null;
            item.Collection = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, TItem item)
        {
            DenyIfHasOtherCollection(item);

            base.InsertItem(index, item);
            item.Collection = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        protected override void RemoveItem(int index)
        {
            var oldItem = this[index];
            base.RemoveItem(index);
            oldItem.Collection = null;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ClearItems()
        {
            var oldItems = new List<TaskDialogButton>(this);
            base.ClearItems();

            foreach (var button in oldItems)
                button.Collection = null;
        }


        private void DenyIfHasOtherCollection(TItem item)
        {
            if (item.Collection != null && item.Collection != this)
                throw new InvalidOperationException(
                        "This control is already part of a different collection.");
        }
    }
}
