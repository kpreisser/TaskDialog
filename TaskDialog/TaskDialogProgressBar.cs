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
        /// <remarks>
        /// Note that switching between <see cref="TaskDialogProgressBarState.Marquee"/>
        /// and one of <see cref="TaskDialogProgressBarState.Paused"/> or
        /// <see cref="TaskDialogProgressBarState.Error"/> will not always work - the
        /// progress bar might get blank in that case.
        /// </remarks>
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
                            // Also need to set the other properties.
                            this.Range = this.range;
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
                if (this.position > 0)
                    this.Position = this.position;                
            }
        }
    }
}
