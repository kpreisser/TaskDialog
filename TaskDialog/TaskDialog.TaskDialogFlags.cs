using System;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        [Flags]
        private enum TaskDialogFlags : int
        {
            None = 0,

            EnableHyperlinks = 0x0001,

            UseMainIconHandle = 0x0002,

            UseFooterIconHandle = 0x0004,

            AllowCancel = 0x0008,

            UseCommandLinks = 0x0010,

            UseNoIconCommandLinks = 0x0020,

            ExpandFooterArea = 0x0040,

            ExpandedByDefault = 0x0080,

            CheckVerificationFlag = 0x0100,

            ShowProgressBar = 0x0200,

            ShowMarqueeProgressBar = 0x0400,

            /// <summary>
            /// If set, the <see cref="TimerTick"/> event will be raised approximately
            /// every 200 milliseconds while the dialog is active.
            /// </summary>
            UseTimer = 0x0800,

            PositionRelativeToWindow = 0x1000,

            RightToLeftLayout = 0x2000,

            NoDefaultRadioButton = 0x4000,

            CanBeMinimized = 0x8000,

            NoSetForeground = 0x00010000,

            SizeToContent = 0x01000000
        }
    }
}
