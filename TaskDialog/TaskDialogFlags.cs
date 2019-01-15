using System;

namespace KPreisser.UI
{
    [Flags]
    internal enum TaskDialogFlags : int
    {
        None = 0,

        EnableHyperlinks = 0x0001,

        UseHIconMain = 0x0002,

        UseHIconFooter = 0x0004,

        AllowDialogCancellation = 0x0008,

        UseCommandLinks = 0x0010,

        UseCommandLinksNoIcon = 0x0020,

        ExpandFooterArea = 0x0040,

        ExpandedByDefault = 0x0080,

        VerificationFlagChecked = 0x0100,

        ShowProgressBar = 0x0200,

        ShowMarqueeProgressBar = 0x0400,

        CallbackTimer = 0x0800,

        PositionRelativeToWindow = 0x1000,

        RtlLayout = 0x2000,

        NoDefaultRadioButton = 0x4000,

        CanBeMinimized = 0x8000,

        // "Don't call SetForegroundWindow() when activating the dialog"
        NoSetForeground = 0x00010000,

        // "used by ShellMessageBox to emulate MessageBox sizing behavior"
        SizeToContent = 0x01000000
    }
}
