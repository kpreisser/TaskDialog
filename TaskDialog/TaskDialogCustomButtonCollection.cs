using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogCustomButtonCollection 
        : Collection<TaskDialogCustomButton>
    {
        // HashSet to detect duplicate items.
        private readonly HashSet<TaskDialogCustomButton> itemSet =
                new HashSet<TaskDialogCustomButton>();

        private TaskDialogContents boundTaskDialogContents;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogCustomButtonCollection()
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
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, TaskDialogCustomButton item)
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();
            DenyIfHasOtherCollection(item);

            var oldItem = this[index];
            if (oldItem != item)
            {
                // First, add the new item (which will throw if it is a duplicate entry),
                // then remove the old one.
                if (!this.itemSet.Add(item))
                    throw new ArgumentException();
                this.itemSet.Remove(oldItem);

                oldItem.Collection = null;
                item.Collection = this;
            }

            base.SetItem(index, item);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, TaskDialogCustomButton item)
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();
            DenyIfHasOtherCollection(item);

            if (!this.itemSet.Add(item))
                throw new ArgumentException();

            item.Collection = this;
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
            
            var oldItem = this[index];            
            oldItem.Collection = null;
            this.itemSet.Remove(oldItem);
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

            foreach (var button in this)
                button.Collection = null;

            this.itemSet.Clear();
            base.ClearItems();
        }


        private void DenyIfHasOtherCollection(TaskDialogCustomButton item)
        {
            if (item.Collection != null && item.Collection != this)
                throw new InvalidOperationException(
                        "This control is already part of a different collection.");
        }
    }
}
