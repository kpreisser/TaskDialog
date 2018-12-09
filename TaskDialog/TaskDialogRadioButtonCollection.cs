using System;
using System.Collections.Generic;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogRadioButtonCollection 
        : TaskDialogControlCollection<TaskDialogRadioButton, TaskDialogRadioButton>
    {
        internal TaskDialogRadioButtonCollection()
            : base()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public TaskDialogRadioButton Add(string text)
        {
            var button = new TaskDialogRadioButton()
            {
                Text = text
            };

            this.Add(button);
            return button;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override TaskDialogRadioButton GetKeyForItem(TaskDialogRadioButton item)
        {
            return item;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, TaskDialogRadioButton item)
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
        protected override void InsertItem(int index, TaskDialogRadioButton item)
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
            var oldItems = new List<TaskDialogRadioButton>(this);
            base.ClearItems();

            foreach (var button in oldItems)
                button.Collection = null;
        }


        private void DenyIfHasOtherCollection(TaskDialogRadioButton item)
        {
            if (item.Collection != null && item.Collection != this)
                throw new InvalidOperationException(
                        "This control is already part of a different collection.");
        }
    }
}
