using System;
using System.ComponentModel;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public sealed class TaskDialogProgressBar : TaskDialogControl
    {
        private TaskDialogProgressBarState state;

        private TaskDialogProgressBarRange range = (0, 100);

        private int position;

        private int marqueeSpeed;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogProgressBar()
            : base()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public TaskDialogProgressBar(TaskDialogProgressBarState state)
            : base()
        {
            // Use the setter which will validate the value.
            this.State = state;
        }


        /// <summary>
        /// Gets or sets the state of the progress bar.
        /// </summary>
        /// <remarks>
        /// This property can be changed while the dialog is shown. However, it is
        /// not possible to change the state from <see cref="TaskDialogProgressBarState.None"/>
        /// to any other state, and vice versa.
        /// </remarks>
        public TaskDialogProgressBarState State
        {
            get => this.state;

            set {
                this.DenyIfBoundAndNotCreated();

                if (this.boundTaskDialogContents != null && value == TaskDialogProgressBarState.None)
                    throw new InvalidOperationException(
                            "Cannot remove the progress bar while the task dialog is shown.");

                //// TODO: Verify the enum value is actually valid

                var previousState = this.state;
                this.state = value;
                try
                {
                    if (this.boundTaskDialogContents != null)
                    {
                        var taskDialog = this.boundTaskDialogContents.BoundTaskDialog;

                        // Check if we need to switch between a marquee and a non-marquee bar.
                        bool newStateIsMarquee = ProgressBarStateIsMarquee(this.state);
                        bool switchMode = ProgressBarStateIsMarquee(previousState) != newStateIsMarquee;
                        if (switchMode)
                        {
                            // When switching from non-marquee to marquee mode, we first need to
                            // set the state to "Normal"; otherwise the marquee will not show.
                            if (newStateIsMarquee && previousState != TaskDialogProgressBarState.Normal)
                                taskDialog.SetProgressBarState(TaskDialogProgressBarNativeState.Normal);

                            taskDialog.SwitchProgressBarMode(newStateIsMarquee);
                        }

                        // Update the properties.
                        if (newStateIsMarquee)
                        {
                            taskDialog.SetProgressBarMarquee(
                                    this.state == TaskDialogProgressBarState.Marquee,
                                    this.marqueeSpeed);
                        }
                        else
                        {
                            taskDialog.SetProgressBarState(GetNativeProgressBarState(this.state));

                            if (switchMode)
                            {
                                // Also need to set the other properties after switching
                                // the mode.
                                this.Range = this.range;
                                this.Position = this.position;

                                // We need to set the position a secondtime to work reliably if the
                                // state is not "Normal".
                                // See this comment in the TaskDialog implementation of the
                                // Windows API Code Pack 1.1:
                                // "Due to a bug that wasn't fixed in time for RTM of Vista,
                                // second SendMessage is required if the state is non-Normal."
                                // Apparently, this bug is still present in Win10 V1803.
                                if (this.state != TaskDialogProgressBarState.Normal)
                                    this.Position = this.position;
                            }
                        }
                    }
                }
                catch
                {
                    // Revert to the previous state. This could happen if the dialog's
                    // DenyIfDialogNotShownOrWaitingForNavigatedEvent() (called by
                    // one of the Set...() methods) throws.
                    this.state = previousState;
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This property can be changed while the dialog is shown.
        /// </remarks>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TaskDialogProgressBarRange Range
        {
            get => this.range;

            set
            {
                this.DenyIfBoundAndNotCreated();

                if (value.Minimum < 0 || value.Minimum > ushort.MaxValue ||
                    value.Maximum < 0 || value.Maximum > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (value.Minimum > value.Maximum)
                    throw new ArgumentException();

                var previousRange = this.range;
                this.range = value;
                try
                {
                    // We only update the TaskDialog if the current state is a non-marquee progress bar.
                    if (this.boundTaskDialogContents != null && !ProgressBarStateIsMarquee(this.state))
                    {
                        this.boundTaskDialogContents.BoundTaskDialog.SetProgressBarRange(
                                this.range.Minimum,
                                this.range.Maximum);
                    }
                }
                catch
                {
                    this.range = previousRange;
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This property can be changed while the dialog is shown.
        /// </remarks>
        public int Position
        {
            get => this.position;

            set
            {
                this.DenyIfBoundAndNotCreated();

                if (value < 0 || value > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                int previousPosition = this.position;
                this.position = value;
                try
                {
                    // We only update the TaskDialog if the current state is a non-marquee progress bar.
                    if (this.boundTaskDialogContents != null && !ProgressBarStateIsMarquee(this.state))
                    {
                        this.boundTaskDialogContents.BoundTaskDialog.SetProgressBarPos(
                                this.position);
                    }
                }
                catch
                {
                    this.position = previousPosition;
                    throw;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// This property can be changed while the dialog is shown.
        /// </remarks>
        public int MarqueeSpeed
        {
            get => this.marqueeSpeed;

            set
            {
                this.DenyIfBoundAndNotCreated();

                int previousMarqueeSpeed = this.marqueeSpeed;
                this.marqueeSpeed = value;
                try
                {
                    // We only update the TaskDialog if the current state is a marquee progress bar.
                    if (this.boundTaskDialogContents != null && ProgressBarStateIsMarquee(this.state))
                        this.State = this.state;
                }
                catch
                {
                    this.marqueeSpeed = previousMarqueeSpeed;
                    throw;
                }
            }
        }


        internal override bool IsCreatable
        {
            get => base.IsCreatable && this.state != TaskDialogProgressBarState.None;
        }


        private static bool ProgressBarStateIsMarquee(TaskDialogProgressBarState state)
        {
            return state == TaskDialogProgressBarState.Marquee ||
                    state == TaskDialogProgressBarState.MarqueePaused;
        }

        private static TaskDialogProgressBarNativeState GetNativeProgressBarState(
                TaskDialogProgressBarState state)
        {
            var nativeState = default(TaskDialogProgressBarNativeState);

            switch (state)
            {
                case TaskDialogProgressBarState.Normal:
                    nativeState = TaskDialogProgressBarNativeState.Normal;
                    break;
                case TaskDialogProgressBarState.Paused:
                    nativeState = TaskDialogProgressBarNativeState.Paused;
                    break;
                case TaskDialogProgressBarState.Error:
                    nativeState = TaskDialogProgressBarNativeState.Error;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return nativeState;
        }


        private protected override TaskDialogFlags GetFlagsCore()
        {
            var flags = base.GetFlagsCore();

            if (ProgressBarStateIsMarquee(this.state))
                flags |= TaskDialogFlags.ShowMarqueeProgressBar;
            else
                flags |= TaskDialogFlags.ShowProgressBar;

            return flags;
        }

        private protected override void ApplyInitializationCore()
        {
            var taskDialog = this.boundTaskDialogContents.BoundTaskDialog;

            if (this.state == TaskDialogProgressBarState.Marquee)
            {
                this.State = this.state;
            }
            else if (this.state != TaskDialogProgressBarState.MarqueePaused)
            {
                this.State = this.state;
                this.Range = this.range;
                this.Position = this.position;
                    
                // See comment in property "State" for why we need to set
                // the position it twice.
                if (this.state != TaskDialogProgressBarState.Normal)
                    this.Position = this.position;
            }
        }
    }
}
