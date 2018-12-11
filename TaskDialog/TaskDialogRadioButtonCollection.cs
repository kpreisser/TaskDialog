using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogRadioButtonCollection 
        : KeyedCollection<TaskDialogRadioButton, TaskDialogRadioButton>
    {
        private TaskDialogContents boundTaskDialogContents;


        internal TaskDialogRadioButtonCollection()
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
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();
            DenyIfHasOtherCollection(item);

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
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();
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
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();

            var oldItem = this[index];
            base.RemoveItem(index);
            oldItem.Collection = null;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void ClearItems()
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();

            foreach (var button in this)
                button.Collection = null;
            base.ClearItems();
        }


        private void DenyIfHasOtherCollection(TaskDialogRadioButton item)
        {
            if (item.Collection != null && item.Collection != this)
                throw new InvalidOperationException(
                        "This control is already part of a different collection.");
        }
    }
}
