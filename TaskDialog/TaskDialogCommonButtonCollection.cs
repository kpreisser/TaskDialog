using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public class TaskDialogCommonButtonCollection 
        : KeyedCollection<TaskDialogResult, TaskDialogCommonButton>
    {
        private TaskDialogContents boundTaskDialogContents;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogCommonButtonCollection()
            : base()
        {
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttons"></param>
        public static implicit operator TaskDialogCommonButtonCollection(TaskDialogButtons buttons)
        {
            var collection = new TaskDialogCommonButtonCollection();

            // Get the button results for the flags.
            foreach (var result in GetResultsForButtonFlags(buttons))
                collection.Add(new TaskDialogCommonButton(result));

            return collection;
        }


        internal TaskDialogContents BoundTaskDialogContents
        {
            get => this.boundTaskDialogContents;
            set => this.boundTaskDialogContents = value;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttons"></param>
        internal static IEnumerable<TaskDialogResult> GetResultsForButtonFlags(
                TaskDialogButtons buttons)
        {
            if ((buttons & TaskDialogButtons.OK) == TaskDialogButtons.OK)
                yield return TaskDialogResult.OK;
            if ((buttons & TaskDialogButtons.Cancel) == TaskDialogButtons.Cancel)
                yield return TaskDialogResult.Cancel;
            if ((buttons & TaskDialogButtons.Abort) == TaskDialogButtons.Abort)
                yield return TaskDialogResult.Abort;
            if ((buttons & TaskDialogButtons.Retry) == TaskDialogButtons.Retry)
                yield return TaskDialogResult.Retry;
            if ((buttons & TaskDialogButtons.Ignore) == TaskDialogButtons.Ignore)
                yield return TaskDialogResult.Ignore;
            if ((buttons & TaskDialogButtons.Yes) == TaskDialogButtons.Yes)
                yield return TaskDialogResult.Yes;
            if ((buttons & TaskDialogButtons.No) == TaskDialogButtons.No)
                yield return TaskDialogResult.No;
            if ((buttons & TaskDialogButtons.Close) == TaskDialogButtons.Close)
                yield return TaskDialogResult.Close;
            if ((buttons & TaskDialogButtons.Help) == TaskDialogButtons.Help)
                yield return TaskDialogResult.Help;
            if ((buttons & TaskDialogButtons.TryAgain) == TaskDialogButtons.TryAgain)
                yield return TaskDialogResult.TryAgain;
            if ((buttons & TaskDialogButtons.Continue) == TaskDialogButtons.Continue)
                yield return TaskDialogResult.Continue;
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


        internal void HandleKeyChange(
                TaskDialogCommonButton button,
                TaskDialogResult newKey)
        {
            this.ChangeItemKey(button, newKey);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void SetItem(int index, TaskDialogCommonButton item)
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();
            DenyIfHasOtherCollection(item);

            var oldItem = this[index];

            // Call the base method first, as it will throw if we would insert a
            // duplicate item.
            base.SetItem(index, item);

            oldItem.Collection = null;
            item.Collection = this;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="item"></param>
        protected override void InsertItem(int index, TaskDialogCommonButton item)
        {
            // Disallow collection modification, so that we don't need to copy it
            // when binding the TaskDialogContents.
            this.boundTaskDialogContents?.DenyIfBound();
            DenyIfHasOtherCollection(item);

            // Call the base method first, as it will throw if we would insert a
            // duplicate item.
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
            oldItem.Collection = null;
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
            base.ClearItems();
        }


        private void DenyIfHasOtherCollection(TaskDialogCommonButton item)
        {
            if (item.Collection != null && item.Collection != this)
                throw new InvalidOperationException(
                        "This control is already part of a different collection.");
        }
    }
}
