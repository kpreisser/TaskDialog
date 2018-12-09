using System;
using System.Collections.Generic;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogCommonButton : TaskDialogButton
    {
        private readonly TaskDialogResult result;

        private bool visible = true;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="button"></param>
        public TaskDialogCommonButton(
                TaskDialogResult button)
            : base()
        {
            if (!IsValidCommonButton(button))
                throw new ArgumentException();

            this.result = button;
        }


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogResult Result
        {
            get => this.result;
        }

        /// <summary>
        /// Gets or sets a value that indicates if this <see cref="TaskDialogCommonButton"/>
        /// should be shown when displaying the Task Dialog.
        /// </summary>
        /// <remarks>
        /// Setting this to <c>false</c> allows you to still receive the
        /// <see cref="TaskDialogButton.ButtonClicked"/> event (e.g. for the
        /// <see cref="TaskDialogResult.Cancel"/> button when
        /// <see cref="TaskDialogContents.AllowCancel"/> is set), or to call the
        /// <see cref="TaskDialogButton.Click"/> method even if the button is not
        /// shown.
        /// </remarks>
        public bool Visible
        {
            get => this.visible;

            set
            {
                this.boundTaskDialogContents?.DenyIfBound();

                this.visible = value;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="buttons"></param>
        internal static IEnumerable<TaskDialogResult> GetResultsForButtonFlags(TaskDialogButtons buttons)
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


        private static bool IsValidCommonButton(
                TaskDialogResult button)
        {
            return button > 0 &&
                    button <= TaskDialogResult.Continue;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.result.ToString();
        }


        internal override TaskDialogFlags GetFlags()
        {
            if (this.visible)
                return base.GetFlags();
            else
                return default;
        }

        internal override void ApplyInitialization()
        {
            if (this.visible)
                base.ApplyInitialization();
        }


        private protected override int GetButtonID()
        {
            return (int)this.result;
        }

        private protected override bool CanUpdate()
        {
            return this.visible;
        }
    }
}
