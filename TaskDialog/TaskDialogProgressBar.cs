using System;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class TaskDialogProgressBar : TaskDialogControl
    {
        private TaskDialogProgressBarState state;

        private (int min, int max) range = (0, 100);

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
        /// Gets or sets the state of the progress bar.
        /// </summary>
        public TaskDialogProgressBarState State
        {
            get => this.state;

            set {
                //// TODO: Verify the enum value is actually valid

                var previousState = this.state;
                this.state = value;

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
        }

        /// <summary>
        /// 
        /// </summary>
        public (int min, int max) Range
        {
            get => this.range;

            set
            {
                if (value.min < 0 || value.min > ushort.MaxValue ||
                    value.max < 0 || value.max > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (value.min > value.max)
                    throw new ArgumentException();
                    
                this.range = value;

                // We only update the TaskDialog if the current state is a non-marquee progress bar.
                if (this.boundTaskDialogContents != null && !ProgressBarStateIsMarquee(this.state))
                {
                    this.boundTaskDialogContents.BoundTaskDialog.SetProgressBarRange(
                            this.range.min,
                            this.range.max);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int Position
        {
            get => this.position;

            set
            {
                if (value < 0 || value > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                this.position = value;

                // We only update the TaskDialog if the current state is a non-marquee progress bar.
                if (this.boundTaskDialogContents != null && !ProgressBarStateIsMarquee(this.state))
                {
                    this.boundTaskDialogContents.BoundTaskDialog.SetProgressBarPos(
                            this.position);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int MarqueeSpeed
        {
            get => this.marqueeSpeed;

            set
            {
                this.marqueeSpeed = value;

                // We only update the TaskDialog if the current state is a marquee progress bar.
                if (this.boundTaskDialogContents != null && ProgressBarStateIsMarquee(this.state))
                    this.State = this.state;
            }
        }


        private static bool ProgressBarStateIsMarquee(TaskDialogProgressBarState state)
        {
            return state == TaskDialogProgressBarState.Marquee ||
                    state == TaskDialogProgressBarState.MarqueeDisabled;
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


        internal override TaskDialogFlags GetFlags()
        {
            var flags = base.GetFlags();

            if (ProgressBarStateIsMarquee(this.state))
                flags |= TaskDialogFlags.ShowMarqueeProgressBar;
            else
                flags |= TaskDialogFlags.ShowProgressBar;

            return flags;
        }

        internal override void ApplyInitialization()
        {
            var taskDialog = this.boundTaskDialogContents.BoundTaskDialog;

            if (this.state == TaskDialogProgressBarState.Marquee)
            {
                this.State = this.state;
            }
            else if (this.state != TaskDialogProgressBarState.MarqueeDisabled)
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
