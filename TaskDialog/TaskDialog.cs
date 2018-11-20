using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    /// <summary>
    /// A task dialog is the successor of the message box and provides a lot more features.
    /// </summary>
    /// <remarks>
    /// For more information, see:
    /// https://docs.microsoft.com/en-us/windows/desktop/Controls/task-dialogs-overview
    /// 
    /// Note: To use a task dialog, the application needs to be compiled with a manifest
    /// that contains a dependency to Microsoft.Windows.Common-Controls (6.0.0.0),
    /// and the thread needs to use the single-threaded apartment (STA) model.
    /// </remarks>
    public partial class TaskDialog
#if !NET_STANDARD
        : System.Windows.Forms.IWin32Window, System.Windows.Interop.IWin32Window
#endif
    {
        /// <summary>
        /// The start ID for custom buttons. We need to ensure we don't use a ID that
        /// is already used for a common button (TaskDialogResult), so we start with
        /// 100 to be safe.
        /// </summary>
        private const int CustomButtonStartID = 100;

        /// <summary>
        /// The start ID for radio buttons. This must be at least 1 because 0 already
        /// stands for "no button".
        /// </summary>
        private const int RadioButtonStartID = 1;

        // Offset for user message types.
        private const int UserMessageOffset = 0x400;

        private const int HResultOk = 0x0; // S_OK

        private const int HResultFalse = 0x1; // S_FALSE

        private const TaskDialogButtons AllCommonButtons =
                TaskDialogButtons.OK |
                TaskDialogButtons.Yes |
                TaskDialogButtons.No |
                TaskDialogButtons.Cancel |
                TaskDialogButtons.Retry |
                TaskDialogButtons.Close |
                TaskDialogButtons.Abort |
                TaskDialogButtons.Ignore |
                TaskDialogButtons.TryAgain |
                TaskDialogButtons.Continue |
                TaskDialogButtons.Help;


        /// <summary>
        /// The delegate for the dialog callback. We must ensure to prevent this delegate
        /// from being garbage-collected as long as at least one dialog is active.
        /// </summary>
        private static readonly TaskDialogCallbackProcDelegate callbackProcDelegate;

        /// <summary>
        /// The function pointer created from the dialog callback delegate.
        /// Note that the pointer will become invalid once the delegate is
        /// garbage-collected.
        /// </summary>
        private static readonly IntPtr callbackProcDelegatePtr;


        private List<TaskDialogCustomButton> customButtons;

        private List<TaskDialogRadioButton> radioButtons;

        /// <summary>
        /// Window handle of the task dialog.
        /// </summary>
        private IntPtr hwndDialog;

        /// <summary>
        /// The <see cref="IntPtr"/> of a <see cref="GCHandle"/> that represents this
        /// <see cref="TaskDialog"/> instance.
        /// </summary>
        private IntPtr instanceHandlePtr;

        private IntPtr? currentOwnerHwnd;

        private bool currentMainIconIsFromHandle;

        private bool currentFooterIconIsFromHandle;

        private TaskDialogCustomButton[] currentCustomButtons;

        private TaskDialogRadioButton[] currentRadioButtons;

        private bool currentVerificationCheckboxShown;

        private TaskDialogResult resultCommonButton;

        private TaskDialogCustomButton resultCustomButton;

        private TaskDialogRadioButton resultRadioButton;

        private bool resultVerificationFlagChecked;

        /// <summary>
        /// Flags for this task dialog instance. By default,
        /// <see cref="TaskDialogFlags.PositionRelativeToWindow"/> is set.
        /// </summary>
        private TaskDialogFlags flags;

        private bool suppressCommonButtonClickedEvent;


        /// <summary>
        /// Occurs after the task dialog has been created but before it is displayed.
        /// </summary>
        public event EventHandler Opened;

        /// <summary>
        /// Occurs when the task dialog is about to be destroyed.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Occurs after the task dialog has navigated.
        /// </summary>
        public event EventHandler Navigated;

        /// <summary>
        /// Occurs when the user presses F1 while the dialog has focus, or when the
        /// user clicks the <see cref="TaskDialogButtons.Help"/> button.
        /// </summary>
        public event EventHandler Help;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TaskDialogHyperlinkClickedEventArgs> HyperlinkClicked;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TaskDialogBooleanStatusEventArgs> ExpandoButtonClicked;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TaskDialogBooleanStatusEventArgs> VerificationClicked;

        /// <summary>
        /// Occurs when one of the dialog's <see cref="CommonButtons"/> has been
        /// clicked.
        /// </summary>
        public event EventHandler<TaskDialogCommonButtonClickedEventArgs> CommonButtonClicked;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TaskDialogTimerTickEventArgs> TimerTick;


        static TaskDialog()
        {
            // Create a delegate for the callback, and get a function pointer for it. Because
            // this will allocate some memory required to store the native code for the function
            // pointer, we only do this once by using a static function, and identify the actual
            // TaskDialog instance by using a GCHandle in the reference data field.
            callbackProcDelegate = HandleTaskDialogCallback;
            callbackProcDelegatePtr = Marshal.GetFunctionPointerForDelegate(callbackProcDelegate);
        }


        /// <summary>
        /// 
        /// </summary>
        public TaskDialog()
        {
            // TaskDialog is only supported on Windows.
#if NET46
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
#else
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#endif
                throw new PlatformNotSupportedException();

            // Set default values.
            Reset();
        }


        /// <summary>
        /// The window handle of the active dialog, or <see cref="IntPtr.Zero"/>
        /// if the dialog is not active. When showing the dialog, the handle will be
        /// available first in the <see cref="Opened"/> event, and last in the
        /// <see cref="Closing"/> after which you shouldn't use it any more.
        /// </summary>
        public IntPtr Handle
        {
            get => this.hwndDialog;
        }

        /// <summary>
        /// Gets or sets the title of the task dialog window.
        /// </summary>
        public string Title
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the main instruction text.
        /// </summary>
        public string MainInstruction
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the dialog's primary content.
        /// </summary>
        public string Content
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text to be used in the dialog's footer area.
        /// </summary>
        public string Footer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text that is displayed for the verification checkbox.
        /// </summary>
        public string VerificationText
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExpandedInformation
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string ExpandedControlText
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public string CollapsedControlText
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the main icon, if <see cref="MainIconHandle"/> is
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        public TaskDialogIcon MainIcon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the handle to the main icon. When this member is not
        /// <see cref="IntPtr.Zero"/>, the <see cref="MainIcon"/> property will
        /// be ignored.
        /// </summary>
        public IntPtr MainIconHandle
        {
            get;
            set;
        }

        /// <summary>
        /// If specified, after the task dialog is opened or navigated, its main icon will
        /// be updated to the specified one.
        /// Note: This will not always work, e.g. when running on Windows Server Core.
        /// Note: This member will be ignored if <see cref="MainIconHandle"/> is not
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        public TaskDialogIcon MainUpdateIcon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the footer icon, if <see cref="FooterIconHandle"/> is
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        public TaskDialogIcon FooterIcon
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the handle to the footer icon. When this member is not
        /// <see cref="IntPtr.Zero"/>, the <see cref="FooterIcon"/> property will
        /// be ignored.
        /// </summary>
        public IntPtr FooterIconHandle
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the common buttons that are to be displayed in the dialog.
        /// If no common button and no custom button is specified, the dialog will
        /// contain the <see cref="TaskDialogButtons.OK"/> button by default.
        /// </summary>
        public TaskDialogButtons CommonButtons
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the width in dialog units that the dialog's client area will get
        /// when the dialog is is created or navigated.
        /// If <c>0</c>, the width will be automatically calculated by the system.
        /// </summary>
        public int Width
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool EnableHyperlinks
        {
            get => GetFlag(TaskDialogFlags.EnableHyperlinks);
            set => SetFlag(TaskDialogFlags.EnableHyperlinks, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the task dialog can be canceled
        /// by pressing ESC, Alt+F4 or clicking the title bar's close button even if no
        /// <see cref="TaskDialogButtons.Cancel"/> button is specified in
        /// <see cref="CommonButtons"/>.
        /// </summary>
        public bool AllowCancel
        {
            get => GetFlag(TaskDialogFlags.AllowCancel);
            set => SetFlag(TaskDialogFlags.AllowCancel, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to display custom buttons
        /// created with <see cref="AddCustomButton(string, bool)"/> as command links
        /// instead of buttons.
        /// </summary>
        public bool UseCommandLinks
        {
            get => GetFlag(TaskDialogFlags.UseCommandLinks);
            set => SetFlag(TaskDialogFlags.UseCommandLinks, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool UseCommandLinksWithoutIcon
        {
            get => GetFlag(TaskDialogFlags.UseNoIconCommandLinks);
            set => SetFlag(TaskDialogFlags.UseNoIconCommandLinks, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ExpandFooterArea
        {
            get => GetFlag(TaskDialogFlags.ExpandFooterArea);
            set => SetFlag(TaskDialogFlags.ExpandFooterArea, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ExpandedByDefault
        {
            get => GetFlag(TaskDialogFlags.ExpandedByDefault);
            set => SetFlag(TaskDialogFlags.ExpandedByDefault, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool VerificationFlagCheckedByDefault
        {
            get => GetFlag(TaskDialogFlags.CheckVerificationFlag);
            set => SetFlag(TaskDialogFlags.CheckVerificationFlag, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a progress bar will be shown in the
        /// task dialog. After the dialog is created/navigated, you can modify the progress bar
        /// properties by calling <see cref="SetProgressBarState(TaskDialogProgressBarState)"/>,
        /// <see cref="SetProgressBarRange(int, int)"/> and <see cref="SetProgressBarPos(int)"/>,
        /// or you can switch it to a marquee progress bar with
        /// <see cref="SwitchProgressBarMode(bool)"/>.
        /// </summary>
        public bool ShowProgressBar
        {
            get => GetFlag(TaskDialogFlags.ShowProgressBar);
            set => SetFlag(TaskDialogFlags.ShowProgressBar, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a marquee progress bar will be
        /// shown in the task dialog. After the dialog is created/navigated, you can enable
        /// marquee by calling <see cref="SetProgressBarMarquee(bool, int)"/>, or you can
        /// switch to a regular progress bar with <see cref="SwitchProgressBarMode(bool)"/>.
        /// </summary>
        public bool ShowMarqueeProgressBar
        {
            get => GetFlag(TaskDialogFlags.ShowMarqueeProgressBar);
            set => SetFlag(TaskDialogFlags.ShowMarqueeProgressBar, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="TimerTick"/> 
        /// event should be raised approximately every 200 milliseconds while the dialog
        /// is active.
        /// </summary>
        public bool UseTimer
        {
            get => GetFlag(TaskDialogFlags.UseTimer);
            set => SetFlag(TaskDialogFlags.UseTimer, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool PositionRelativeToWindow
        {
            get => GetFlag(TaskDialogFlags.PositionRelativeToWindow);
            set => SetFlag(TaskDialogFlags.PositionRelativeToWindow, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool RightToLeftLayout
        {
            get => GetFlag(TaskDialogFlags.RightToLeftLayout);
            set => SetFlag(TaskDialogFlags.RightToLeftLayout, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether no radio button will be selected
        /// by default.
        /// </summary>
        public bool NoDefaultRadioButton
        {
            get => GetFlag(TaskDialogFlags.NoDefaultRadioButton);
            set => SetFlag(TaskDialogFlags.NoDefaultRadioButton, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the task dialog can be minimized.
        /// </summary>
        public bool CanBeMinimized
        {
            get => GetFlag(TaskDialogFlags.CanBeMinimized);
            set => SetFlag(TaskDialogFlags.CanBeMinimized, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool DoNotSetForeground
        {
            get => GetFlag(TaskDialogFlags.NoSetForeground);
            set => SetFlag(TaskDialogFlags.NoSetForeground, value);
        }

        /// <summary>
        /// 
        /// </summary>
        public bool SizeToContent
        {
            get => GetFlag(TaskDialogFlags.SizeToContent);
            set => SetFlag(TaskDialogFlags.SizeToContent, value);
        }

        /// <summary>
        /// The default custom button. If null, the <see cref="DefaultCommonButton"/>
        /// will be used.
        /// </summary>
        public ITaskDialogCustomButton DefaultCustomButton
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public TaskDialogResult DefaultCommonButton
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public ITaskDialogRadioButton DefaultRadioButton
        {
            get;
            set;
        }

        /// <summary>
        /// If <see cref="ResultCustomButton"/> is null, this field contains the
        /// <see cref="TaskDialogResult"/> of the common buttons that was pressed.
        /// </summary>
        public TaskDialogResult ResultCommonButton
        {
            get => this.resultCommonButton;
        }

        /// <summary>
        /// If not null, contains the custom button that was pressed. Otherwise, 
        /// <see cref="ResultCommonButton"/> contains the common button that was pressed.
        /// </summary>
        public ITaskDialogCustomButton ResultCustomButton
        {
            get => this.resultCustomButton;
        }

        /// <summary>
        /// 
        /// </summary>
        public ITaskDialogRadioButton ResultRadioButton
        {
            get => this.resultRadioButton;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool ResultVerificationFlagChecked
        {
            get => this.resultVerificationFlagChecked;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                string content,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = 0)
        {
            return Show(IntPtr.Zero, content, instruction, title, buttons, icon);
        }

#if !NET_STANDARD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        /// <param name="content"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                System.Windows.Window owner,
                string content,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = 0)
        {
            return Show(GetWindowHandle(owner), content, instruction, title, buttons, icon);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        /// <param name="content"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                System.Windows.Interop.IWin32Window owner,
                string content,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = 0)
        {
            return Show(GetWindowHandle(owner), content, instruction, title, buttons, icon);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        /// <param name="content"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                System.Windows.Forms.IWin32Window owner,
                string content,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = 0)
        {
            return Show(GetWindowHandle(owner), content, instruction, title, buttons, icon);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        /// <param name="content"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                TaskDialog owner,
                string content,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = 0)
        {
            return Show(owner.Handle, content, instruction, title, buttons, icon);
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwndOwner">
        /// The window handle of the owner, or <see cref="IntPtr.Zero"/> to show a non-modal
        /// dialog.
        /// </param>
        /// <param name="content"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                IntPtr hwndOwner,
                string content, string
                instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = 0)
        {
            var dialog = new TaskDialog()
            {
                Content = content,
                MainInstruction = instruction,
                Title = title,
                CommonButtons = buttons,
                MainIcon = icon
            };
            dialog.Show(hwndOwner);

            return dialog.ResultCommonButton;
        }


        private static void FreeConfig(IntPtr ptrToFree)
        {
            Marshal.FreeHGlobal(ptrToFree);
        }

        private static bool IsValidCommonButton(
                TaskDialogResult button,
                bool allowNone = false)
        {
            return (allowNone ? (button >= 0) : (button > 0)) &&
                    button <= TaskDialogResult.Continue;
        }

#if !NET_STANDARD
        private static IntPtr GetWindowHandle(System.Windows.Window window)
        {
            return new System.Windows.Interop.WindowInteropHelper(window).Handle;
        }

        private static IntPtr GetWindowHandle(System.Windows.Interop.IWin32Window window)
        {
            return window.Handle;
        }

        private static IntPtr GetWindowHandle(System.Windows.Forms.IWin32Window window)
        {
            return window.Handle;
        }
#endif

        private static int HandleTaskDialogCallback(
                IntPtr hWnd,
                TaskDialogNotification notification,
                IntPtr wParam,
                IntPtr lParam,
                IntPtr referenceData)
        {
            // Get the instance from the GCHandle pointer.
            var instance = (TaskDialog)GCHandle.FromIntPtr(referenceData).Target;
            instance.hwndDialog = hWnd;

            switch (notification)
            {
                case TaskDialogNotification.Created:
                    instance.ApplyButtonInitialization();
                    instance.OnOpened(EventArgs.Empty);
                    break;

                case TaskDialogNotification.Destroyed:
                    instance.OnClosing(EventArgs.Empty);

                    // Clear the dialog handle, because according to the docs, we must not 
                    // continue to send any notifications to the dialog after the callback
                    // function has returned from being called with the 'Destroyed'
                    // notification.
                    // Note: When multiple dialogs are shown (so Show() will occur multiple
                    // times in the call stack) and a previously opened dialog is closed,
                    // the Destroyed notification for the closed dialog will only occur after
                    // the newer dialogs are also closed.
                    instance.hwndDialog = IntPtr.Zero;
                    break;

                case TaskDialogNotification.Navigated:
                    instance.ApplyButtonInitialization();
                    instance.OnNavigated(EventArgs.Empty);
                    break;

                case TaskDialogNotification.HyperlinkClicked:
                    string link = Marshal.PtrToStringUni(lParam);
                    instance.OnHyperlinkClicked(new TaskDialogHyperlinkClickedEventArgs(link));
                    break;

                case TaskDialogNotification.ButtonClicked:
                    if (instance.suppressCommonButtonClickedEvent)
                        return HResultOk;

                    int buttonID = wParam.ToInt32();

                    // Check if the button is part of the custom buttons.
                    bool cancelClose;
                    if (buttonID >= CustomButtonStartID)
                    {
                        var eventArgs = new TaskDialogCustomButtonClickedEventArgs();
                        instance.currentCustomButtons[buttonID - CustomButtonStartID]
                                .OnButtonClicked(eventArgs);
                        cancelClose = eventArgs.CancelClose;
                    }
                    else
                    {
                        var eventArgs = new TaskDialogCommonButtonClickedEventArgs(
                                (TaskDialogResult)buttonID);
                        instance.OnCommonButtonClicked(eventArgs);
                        cancelClose = eventArgs.CancelClose;
                    }

                    return cancelClose ? HResultFalse : HResultOk;

                case TaskDialogNotification.RadioButtonClicked:
                    int radioButtonID = wParam.ToInt32();

                    var radioButton = instance.currentRadioButtons
                            [radioButtonID - RadioButtonStartID];
                    radioButton.OnRadioButtonClicked(EventArgs.Empty);
                    break;

                case TaskDialogNotification.ExpandoButtonClicked:
                    instance.OnExpandoButtonClicked(new TaskDialogBooleanStatusEventArgs(
                            wParam != IntPtr.Zero));
                    break;

                case TaskDialogNotification.VerificationClicked:
                    instance.OnVerificationClicked(new TaskDialogBooleanStatusEventArgs(
                            wParam != IntPtr.Zero));
                    break;

                case TaskDialogNotification.Help:
                    instance.OnHelp(EventArgs.Empty);
                    break;

                case TaskDialogNotification.Timer:
                    // Note: The documentation specifies that wParam contains a DWORD,
                    // which might mean that on 64-bit platforms the highest bit (63)
                    // will be zero even if the DWORD has its highest bit (31) set. In
                    // that case, IntPtr.ToInt32() would throw an OverflowException.
                    // Therefore, we use .ToInt64() and then convert it to an int.
                    int ticks = IntPtr.Size == 8 ?
                            unchecked((int)wParam.ToInt64()) :
                            wParam.ToInt32();
                    var tickEventArgs = new TaskDialogTimerTickEventArgs(ticks);
                    instance.OnTimerTick(tickEventArgs);

                    return tickEventArgs.ResetTickCount ? HResultFalse : HResultOk;
            }

            // Note: Previously, the code caught exceptions and returned
            // Marshal.GetHRForException(), so that the TaskDialog would be closed on an
            // unhandled exception and we could rethrow it after TaskDialogIndirect() returns.
            // However, this causes the debugger to not break at the original exception
            // location, and it is probably not desired that the Dialog is actually destroyed
            // because this would be inconsistent with the case when an unhandled exception
            // occurs in a different WndProc function not handled by the TaskDialog
            // (e.g. a WinForms/WPF Timer Tick event). Therefore, we don't catch
            // Exceptions any more.
            // Note: Currently, this means that a NRE will occur in the callback after
            // TaskDialog.Show() returns due to an unhandled exception because the
            // TaskDialog is still displayed (see comment in Show()).
            // We could solve this by not freeing the GCHandle if the dialog's
            // window handle is set, and instead free it in the Destroyed event.
            return HResultOk;
        }


        /// <summary>
        /// Creates and returns a new custom button with the specified text, and adds
        /// it to this <see cref="TaskDialog"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="elevationRequired"></param>
        /// <returns>
        /// The <see cref="ITaskDialogCustomButton"/> instance representing the custom button.
        /// </returns>
        public ITaskDialogCustomButton AddCustomButton(string text, bool elevationRequired = false)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var button = new TaskDialogCustomButton(this, text, elevationRequired);
            (this.customButtons ?? (this.customButtons = new List<TaskDialogCustomButton>()))
                    .Add(button);

            return button;
        }

        /// <summary>
        /// Creates and returns a new radio button with the specified text, and adds
        /// it to this <see cref="TaskDialog"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>
        /// The <see cref="ITaskDialogRadioButton"/> instance representing the radio button.
        /// </returns>
        public ITaskDialogRadioButton AddRadioButton(string text)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            var button = new TaskDialogRadioButton(this, text);
            (this.radioButtons ?? (this.radioButtons = new List<TaskDialogRadioButton>()))
                    .Add(button);

            return button;
        }

        /// <summary>
        /// Removes the specified custom button that was added with
        /// <see cref="AddCustomButton(string, bool)"/>.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool RemoveCustomButton(ITaskDialogCustomButton button)
        {
            return this.customButtons?.Remove(button as TaskDialogCustomButton) ?? false;
        }

        /// <summary>
        /// Removes the specified radio button that was aded with
        /// <see cref="AddRadioButton(string)"/>.
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public bool RemoveRadioButton(ITaskDialogRadioButton button)
        {
            return this.radioButtons?.Remove(button as TaskDialogRadioButton) ?? false;
        }

        /// <summary>
        /// Removes all custom buttons added with
        /// <see cref="AddCustomButton(string, bool)"/>.
        /// </summary>
        public void ClearCustomButtons()
        {
            this.customButtons?.Clear();
        }

        /// <summary>
        /// Removes all radio buttons added with
        /// <see cref="AddRadioButton(string)"/>.
        /// </summary>
        public void ClearRadioButtons()
        {
            this.radioButtons?.Clear();
        }

        /// <summary>
        /// Resets all properties to their default values, e.g. for calling <see cref="Navigate"/>
        /// with new values. You can specify to also clear the event handlers (except
        /// <see cref="Opened"/> and <see cref="Closing"/>).
        /// </summary>
        /// <param name="clearEventHandlers">
        /// <c>true</c> to also clear the event handlers
        /// (except <see cref="Opened"/>, <see cref="Closing"/>).</param>
        public void Reset(bool clearEventHandlers = false)
        {
            this.flags = TaskDialogFlags.PositionRelativeToWindow;

            this.Title =
                    this.MainInstruction =
                    this.Content =
                    this.Footer =
                    this.VerificationText =
                    this.ExpandedInformation =
                    this.ExpandedControlText =
                    this.CollapsedControlText = null;
            this.MainIcon =
                    this.MainUpdateIcon =
                    this.FooterIcon = default;
            this.MainIconHandle =
                    this.FooterIconHandle = default;
            this.CommonButtons = default;
            this.DefaultCommonButton = default;
            this.DefaultCustomButton = null;
            this.DefaultRadioButton = null;
            this.Width = default;

            ClearCustomButtons();
            ClearRadioButtons();

            if (clearEventHandlers)
            {
                this.Navigated = null;
                this.Help = null;
                this.HyperlinkClicked = null;
                this.ExpandoButtonClicked = null;
                this.VerificationClicked = null;
                this.CommonButtonClicked = null;
                this.TimerTick = null;
            }
        }

        /// <summary>
        /// Shows the dialog. After the dialog is created, the <see cref="Opened"/>
        /// event occurs which allows to customize the dialog. When the dialog is about to 
        /// close, the <see cref="Closing"/> event occurs.
        /// 
        /// Starting with the <see cref="Opened"/> event, you can call methods on the active 
        /// task dialog to update its state until the <see cref="Closing"/> event occurs.
        /// </summary>
        public void Show()
        {
            Show(IntPtr.Zero);
        }

#if !NET_STANDARD
        /// <summary>
        /// Shows the dialog. After the dialog is created, the <see cref="Opened"/>
        /// event occurs which allows to customize the dialog. When the dialog is about to 
        /// close, the <see cref="Closing"/> event occurs.
        /// 
        /// Starting with the <see cref="Opened"/> event, you can call methods on the active 
        /// task dialog to update its state until the <see cref="Closing"/> event occurs.
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        public void Show(System.Windows.Window owner)
        {
            Show(GetWindowHandle(owner));
        }

        /// <summary>
        /// Shows the dialog. After the dialog is created, the <see cref="Opened"/>
        /// event occurs which allows to customize the dialog. When the dialog is about to 
        /// close, the <see cref="Closing"/> event occurs.
        /// 
        /// Starting with the <see cref="Opened"/> event, you can call methods on the active 
        /// task dialog to update its state until the <see cref="Closing"/> event occurs.
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        public void Show(System.Windows.Interop.IWin32Window owner)
        {
            Show(GetWindowHandle(owner));
        }

        /// <summary>
        /// Shows the dialog. After the dialog is created, the <see cref="Opened"/>
        /// event occurs which allows to customize the dialog. When the dialog is about to 
        /// close, the <see cref="Closing"/> event occurs.
        /// 
        /// Starting with the <see cref="Opened"/> event, you can call methods on the active 
        /// task dialog to update its state until the <see cref="Closing"/> event occurs.
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        public void Show(System.Windows.Forms.IWin32Window owner)
        {
            Show(GetWindowHandle(owner));
        }
#endif

        /// <summary>
        /// Shows the dialog. After the dialog is created, the <see cref="Opened"/>
        /// event occurs which allows to customize the dialog. When the dialog is about to 
        /// close, the <see cref="Closing"/> event occurs.
        /// 
        /// Starting with the <see cref="Opened"/> event, you can call methods on the active 
        /// task dialog to update its state until the <see cref="Closing"/> event occurs.
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a non-modal dialog.</param>
        public void Show(TaskDialog owner)
        {
            Show(owner.Handle);
        }

        /// <summary>
        /// Shows the dialog. After the dialog is created, the <see cref="Opened"/>
        /// event occurs which allows to customize the dialog. When the dialog is about to 
        /// close, the <see cref="Closing"/> event occurs.
        /// 
        /// Starting with the <see cref="Opened"/> event, you can call methods on the active 
        /// task dialog to update its state until the <see cref="Closing"/> event occurs.
        /// </summary>
        /// <param name="hwndOwner">
        /// The window handle of the owner, or <see cref="IntPtr.Zero"/> to show a non-modal
        /// dialog.
        /// </param>
        public void Show(IntPtr hwndOwner)
        {
            // Recursive Show() is not possible because we would incorrectly handle notifications.
            if (this.instanceHandlePtr != IntPtr.Zero)
                throw new InvalidOperationException(
                        "Cannot recursively show the same task dialog instance.");

            // Validate the config.
            CheckConfig();

            // Allocate a GCHandle which we will use for the callback data.
            var instanceHandle = GCHandle.Alloc(this);
            try
            {
                this.instanceHandlePtr = GCHandle.ToIntPtr(instanceHandle);
                this.currentOwnerHwnd = hwndOwner;

                // Clear the previous result properties.
                this.resultVerificationFlagChecked = default;
                this.resultCommonButton = default;
                this.resultCustomButton = null;
                this.resultRadioButton = null;

                AcquireCurrentConfig();
                AllocateConfig(
                       out var ptrToFree,
                       out var ptrTaskDialogConfig);
                try
                {
                    int ret = NativeMethods.TaskDialogIndirect(
                                ptrTaskDialogConfig,
                                out int resultButtonID,
                                out int resultRadioButtonID,
                                out this.resultVerificationFlagChecked);

                    //// Note: If a exception occurs here when hwndDialog is not 0, it means the TaskDialogIndirect
                    //// run the event loop and called a WndProc e.g. from a window, whose event handler threw an
                    //// exception. In that case we cannot catch and marshal it to a HResult, so the CLR will
                    //// manipulate the managed stack so that it doesn't contain the transition to and from native
                    //// code. However, the TaskDialog still calls our TaskDialogCallbackProc (by dispatching
                    //// messages to the WndProc) when the current event handler from WndProc returns, but as
                    //// we already freed the GCHandle, a NRE will occur.

                    //// This is OK because the same issue occurs when using a message box with WPF or WinForms:
                    //// If do MessageBox.Show() wrapped in a try/catch on a button click, and before calling
                    //// .Show() create and start a timer which stops and throws an exception on its Tick event,
                    //// the application will crash with an AccessViolationException as soon as you close
                    //// the MessageBox.

                    // Marshal.ThrowExceptionForHR will use the IErrorInfo on the current thread if it exists,
                    // in which case it ignores the error code. Therefore we only call it if the HResult is not
                    // OK to avoid incorrect exceptions being thrown.
                    if (ret != HResultOk)
                        Marshal.ThrowExceptionForHR(ret);

                    // Set the result fields.
                    if (resultButtonID >= CustomButtonStartID)
                    {
                        this.resultCustomButton = this.currentCustomButtons
                                [resultButtonID - CustomButtonStartID];
                        this.resultCommonButton = 0;
                    }
                    else
                    {
                        this.resultCustomButton = null;
                        this.resultCommonButton = (TaskDialogResult)resultButtonID;
                    }

                    // Note that even if we have radio buttons, it could be that the user
                    // didn't select one.
                    this.resultRadioButton = resultRadioButtonID >= RadioButtonStartID ?
                            this.currentRadioButtons[resultRadioButtonID - RadioButtonStartID] :
                            null;
                }
                finally
                {
                    // Clear the handles and free the memory.
                    this.currentOwnerHwnd = null;
                    FreeConfig(ptrToFree);
                    ReleaseCurrentConfig();

                    // We need to ensure the callback delegate is not garbage-collected
                    // as long as TaskDialogIndirect doesn't return, by calling GC.KeepAlive().
                    // 
                    // This is not an exaggeration, as the comment for GC.KeepAlive() says
                    // the following:
                    // The JIT is very aggressive about keeping an 
                    // object's lifetime to as small a window as possible, to the point
                    // where a 'this' pointer isn't considered live in an instance method
                    // unless you read a value from the instance.
                    //
                    // Note: As this is a static field, in theory we would not need to call
                    // GC.KeepAlive() here, but we still do it to be safe.
                    GC.KeepAlive(callbackProcDelegate);
                }
            }
            finally
            {
                this.instanceHandlePtr = IntPtr.Zero;
                instanceHandle.Free();
            }
        }

        //// Messages that can be sent to the dialog while it is active.

        /// <summary>
        /// While the dialog is active, closes the dialog with a 
        /// <see cref="TaskDialogResult.Cancel"/> result.
        /// </summary>
        public void Close(bool suppressCommonButtonClickedEvent = true)
        {
            if (suppressCommonButtonClickedEvent)
                this.suppressCommonButtonClickedEvent = true;
            try
            {
                // Send a click button message with the cancel result.
                ClickCommonButton(TaskDialogResult.Cancel);
            }
            finally
            {
                if (suppressCommonButtonClickedEvent)
                    this.suppressCommonButtonClickedEvent = false;
            }
        }

        /// <summary>
        /// While the dialog is active, recreates the dialog from the current properties.
        /// After the dialog is recreated, the <see cref="Navigated"/> event occurs which allows
        /// you to further customize the dialog (just like with the <see cref="Opened"/> event
        /// after calling <see cref="Show(IntPtr)"/>). However, instead of handling the
        /// <see cref="Navigated"/> you can also supply a handler to the
        /// <paramref name="navigatedHandler"/> parameter that will only be called for this
        /// specific navigation (instead of for all navigations).
        /// </summary>
        /// <remarks>
        /// Note that you should not call this method in the <see cref="Opened"/> event
        /// because the task dialog is not yet displayed in that state.
        /// </remarks>
        /// <param name="navigatedHandler">
        /// A handler that will be called after the dialog is navigated/recreated.
        /// </param>
        public void Navigate(EventHandler navigatedHandler = null)
        {
            // Before checking the config and acquiring it, ensure the dialog is actually
            // active.
            DenyIfDialogNotActive();

            // Validate the config.
            CheckConfig();

            // Check if we need to add the navigated event handler.
            var internalNavigatedHandler = null as EventHandler;
            if (navigatedHandler != null)
            {
                internalNavigatedHandler = (sender, e) =>
                {
                    this.Navigated -= internalNavigatedHandler;
                    navigatedHandler(sender, e);
                };
                this.Navigated += internalNavigatedHandler;
            }
            try
            {
                // We can now release the current config and apply the new one.
                ReleaseCurrentConfig();
                AcquireCurrentConfig();
                AllocateConfig(
                        out var ptrToFree,
                        out var ptrTaskDialogConfig);
                try
                {
                    // Note: If the task dialog cannot be recreated with the new contents,
                    // the dialog will close and TaskDialogIndirect() returns with an error
                    // code.
                    SendTaskDialogMessage(
                            TaskDialogMessage.NavigatePage,
                            0,
                            ptrTaskDialogConfig);
                }
                finally
                {
                    // We can now free the memory because SendMessage does not return
                    // until the message has been processed.
                    FreeConfig(ptrToFree);
                }
            }
            catch
            {
                if (internalNavigatedHandler != null)
                    this.Navigated -= internalNavigatedHandler;

                throw;
            }
        }

        /// <summary>
        /// While the dialog is active, enables or disables the specified common button.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="enable"></param>
        public void SetCommonButtonEnabled(TaskDialogResult button, bool enable)
        {
            if (!IsValidCommonButton(button))
                throw new ArgumentException("An invalid common button was specified.");

            SetButtonEnabledCore((int)button, enable);
        }

        /// <summary>
        /// While the dialog is active, enables or disables the UAC shield symbol for the
        /// specified common button.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="requiresElevation"></param>
        public void SetCommonButtonElevationRequired(
                TaskDialogResult button,
                bool requiresElevation)
        {
            if (!IsValidCommonButton(button))
                throw new ArgumentException("An invalid common button was specified.");

            SetButtonElevationRequiredStateCore((int)button, requiresElevation);
        }

        /// <summary>
        /// While the dialog is active, switches the progress bar mode to either a
        /// marquee progress bar or to a regular progress bar. The dialog must have
        /// been created with either <see cref="ShowProgressBar"/> or
        /// <see cref="ShowMarqueeProgressBar"/> set to <c>true</c>.
        /// For a marquee progress bar, you can enable or disable the marquee using
        /// <see cref="SetProgressBarMarquee(bool, int)"/>.
        /// </summary>
        /// <param name="marqueeProgressBar"></param>
        public void SwitchProgressBarMode(bool marqueeProgressBar)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.SetMarqueeProgressBar,
                    marqueeProgressBar ? 1 : 0,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is active, enables or disables progress bar marquee when
        /// an marquee progress bar is displayed.
        /// The dialog must have been created with <see cref="ShowMarqueeProgressBar"/>
        /// set to <c>true</c> or you must call <see cref="SwitchProgressBarMode(bool)"/>
        /// with value <c>true</c> to switch the regular progress bar to a marquee
        /// progress bar.
        /// </summary>
        /// <param name="enableMarquee"></param>
        /// <param name="animationSpeed">
        /// The time in milliseconds between marquee animation updates. If <c>0</c>, the animation
        /// will be updated every 30 milliseconds.
        /// </param>
        public void SetProgressBarMarquee(bool enableMarquee, int animationSpeed = 0)
        {
            if (animationSpeed < 0)
                throw new ArgumentOutOfRangeException(nameof(animationSpeed));

            SendTaskDialogMessage(
                    TaskDialogMessage.SetProgressBarMarquee,
                    enableMarquee ? 1 : 0,
                    (IntPtr)animationSpeed);
        }

        /// <summary>
        /// While the dialog is active, sets the progress bar range. The default range is 0 to 100.
        /// The dialog must have been created with <see cref="ShowProgressBar"/>
        /// set to <c>true</c> or you must call <see cref="SwitchProgressBarMode(bool)"/>
        /// with value <c>false</c> to switch the marquee progress bar to a 
        /// regular progress bar.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public void SetProgressBarRange(int min, int max)
        {
            if (min < 0 || min > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(min));
            if (max < 0 || max > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(max));

            int param = min | (max << 0x10);
            SendTaskDialogMessage(
                    TaskDialogMessage.SetProgressBarRange,
                    0,
                    (IntPtr)param);
        }

        /// <summary>
        /// While the dialog is active, sets the progress bar position.
        /// The dialog must have been created with <see cref="ShowProgressBar"/>
        /// set to <c>true</c> or you must call <see cref="SwitchProgressBarMode(bool)"/>
        /// with value <c>false</c> to switch the marquee progress bar to a 
        /// regular progress bar.
        /// </summary>
        /// <param name="pos"></param>
        public void SetProgressBarPos(int pos)
        {
            if (pos < 0 || pos > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(pos));

            SendTaskDialogMessage(
                    TaskDialogMessage.SetProgressBarPosition,
                    pos,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is active, sets the progress bar state.
        /// The dialog must have been created with <see cref="ShowProgressBar"/>
        /// set to <c>true</c> or you must call <see cref="SwitchProgressBarMode(bool)"/>
        /// with value <c>false</c> to switch the marquee progress bar to a 
        /// regular progress bar.
        /// </summary>
        /// <param name="state"></param>
        public void SetProgressBarState(TaskDialogProgressBarState state)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.SetProgressBarState,
                    (int)state,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is active, updates the specified dialog elements with the
        /// values from the current properties.
        /// Note that when updating the main icon, the bar color will not change.
        /// </summary>
        /// <param name="updateFlags"></param>
        public void UpdateElements(TaskDialogUpdateElements updateFlags)
        {
            CheckUpdateText(
                    updateFlags,
                    TaskDialogUpdateElements.Content,
                    TaskDialogElements.Content,
                    this.Content);
            CheckUpdateText(
                    updateFlags,
                    TaskDialogUpdateElements.ExpandedInformation,
                    TaskDialogElements.ExpandedInformation,
                    this.ExpandedInformation);
            CheckUpdateText(
                    updateFlags,
                    TaskDialogUpdateElements.Footer,
                    TaskDialogElements.Footer,
                    this.Footer);
            CheckUpdateText(
                    updateFlags,
                    TaskDialogUpdateElements.MainInstruction,
                    TaskDialogElements.MainInstruction,
                    this.MainInstruction);
            CheckUpdateIcon(
                    updateFlags,
                    TaskDialogUpdateElements.MainIcon,
                    TaskDialogIconElement.Main,
                    this.currentMainIconIsFromHandle ?
                        this.MainIconHandle : (IntPtr)this.MainIcon);
            CheckUpdateIcon(
                    updateFlags,
                    TaskDialogUpdateElements.FooterIcon,
                    TaskDialogIconElement.Footer,
                    this.currentFooterIconIsFromHandle ?
                        this.FooterIconHandle : (IntPtr)this.FooterIcon);
        }

        /// <summary>
        /// While the dialog is active, sets the verification checkbox to the specified
        /// state.
        /// </summary>
        /// <param name="isChecked"></param>
        /// <param name="focus"></param>
        public void ClickVerification(bool isChecked, bool focus = false)
        {
            // Check if the current dialog actually shows a checkbox; otherwise this would
            // lead to an AccessViolationException.
            if (!this.currentVerificationCheckboxShown)
                throw new InvalidOperationException(
                        "Can only click the verification checkbox if it is shown.");

            SendTaskDialogMessage(
                    TaskDialogMessage.ClickVerification,
                    isChecked ? 1 : 0,
                    (IntPtr)(focus ? 1 : 0));
        }

        /// <summary>
        /// While the dialog is active, clicks the specified common button.
        /// </summary>
        /// <param name="button"></param>
        public void ClickCommonButton(TaskDialogResult button)
        {
            if (!IsValidCommonButton(button))
                throw new ArgumentException("An invalid common button was specified.");

            ClickButtonCore((int)button);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnOpened(EventArgs e)
        {
            this.Opened?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnClosing(EventArgs e)
        {
            this.Closing?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnNavigated(EventArgs e)
        {
            this.Navigated?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnHelp(EventArgs e)
        {
            this.Help?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        protected void OnCommonButtonClicked(TaskDialogCommonButtonClickedEventArgs e)
        {
            this.CommonButtonClicked?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnHyperlinkClicked(TaskDialogHyperlinkClickedEventArgs e)
        {
            this.HyperlinkClicked?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnExpandoButtonClicked(TaskDialogBooleanStatusEventArgs e)
        {
            this.ExpandoButtonClicked?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnVerificationClicked(TaskDialogBooleanStatusEventArgs e)
        {
            this.VerificationClicked?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        protected void OnTimerTick(TaskDialogTimerTickEventArgs e)
        {
            this.TimerTick?.Invoke(this, e);
        }


        private void CheckConfig()
        {
            //// Before assigning button IDs etc., check if the button configs are OK.
            //// This needs to be done before clearing the old button IDs and assigning
            //// the new ones, because it is possible to use the same button
            //// instances after a dialog has been created for Navigate(), where need to
            //// do the check, then release the old buttons, then assign the new
            //// buttons.

            if (!IsValidCommonButton(this.DefaultCommonButton, true))
                throw new InvalidOperationException(
                        "An invalid default common button was set.");

            if (this.DefaultCustomButton != null &&
                    !(this.customButtons?.Contains(this.DefaultCustomButton) == true))
                throw new InvalidOperationException(
                        $"The default custom button must have been added with " +
                        $"{nameof(AddCustomButton)}().");

            if (this.DefaultRadioButton != null &&
                    !(this.radioButtons?.Contains(this.DefaultRadioButton) == true))
                throw new InvalidOperationException(
                        $"The default radio button must have been added with " +
                        $"{nameof(AddRadioButton)}().");

            if ((this.UseCommandLinks || this.UseCommandLinksWithoutIcon) &&
                    !(this.customButtons?.Count > 0))
                throw new InvalidOperationException(
                        $"When enabling {nameof(this.UseCommandLinks)} or " +
                        $"{nameof(this.UseCommandLinksWithoutIcon)}, at " +
                        $"least one custom button needs to be added " +
                        $"with {nameof(AddCustomButton)}().");

            if (this.customButtons?.Count > int.MaxValue - CustomButtonStartID ||
                    this.radioButtons?.Count > int.MaxValue - RadioButtonStartID)
                throw new InvalidOperationException(
                        "Too many custom buttons or radio buttons have been added.");

            if ((this.CommonButtons & ~AllCommonButtons) != 0)
                throw new InvalidOperationException("Invalid common buttons.");
        }

        private void AcquireCurrentConfig()
        {
            //// This method assumes CheckConfig() has already been called.

            // The verification checkbox will only be displayed if the string is not empty.
            this.currentVerificationCheckboxShown = this.VerificationText?.Length > 0 &&
                    this.VerificationText[0] != '\0';
            this.currentMainIconIsFromHandle = this.MainIconHandle != IntPtr.Zero;
            this.currentFooterIconIsFromHandle = this.FooterIconHandle != IntPtr.Zero;

            // Assign IDs to the buttons based on their index.
            if (this.customButtons?.Count > 0)
            {
                var buttons = this.currentCustomButtons = this.customButtons.ToArray();
                for (int i = 0; i < buttons.Length; i++)
                    buttons[i].ButtonID = CustomButtonStartID + i;
            }
            else
            {
                this.currentCustomButtons = null;
            }

            if (this.radioButtons?.Count > 0)
            {
                var radioButtons = this.currentRadioButtons = this.radioButtons.ToArray();
                for (int i = 0; i < radioButtons.Length; i++)
                    radioButtons[i].ButtonID = RadioButtonStartID + i;
            }
            else
            {
                this.currentRadioButtons = null;
            }
        }

        private void ReleaseCurrentConfig()
        {
            this.currentVerificationCheckboxShown = false;

            if (this.currentCustomButtons != null)
            {
                var buttons = this.currentCustomButtons;
                for (int i = 0; i < buttons.Length; i++)
                    buttons[i].ButtonID = null;

                this.currentCustomButtons = null;
            }

            if (this.currentRadioButtons != null)
            {
                var radioButtons = this.currentRadioButtons;
                for (int i = 0; i < radioButtons.Length; i++)
                    radioButtons[i].ButtonID = null;

                this.currentRadioButtons = null;
            }
        }

        private unsafe void AllocateConfig(
                out IntPtr ptrToFree,
                out IntPtr ptrTaskDialogConfig)
        {
            checked
            {
                var flags = this.flags;
                if (this.currentMainIconIsFromHandle)
                    flags |= TaskDialogFlags.UseMainIconHandle;
                if (this.currentFooterIconIsFromHandle)
                    flags |= TaskDialogFlags.UseFooterIconHandle;

                // First, calculate the necessary memory size we need to allocate for all
                // structs and strings.
                // Note: Each Align() call when calculating the size must correspond with a
                // Align() call when incrementing the pointer.
                // Use a byte pointer so we can use byte-wise pointer arithmetics.
                var sizeToAllocate = (byte*)0;
                sizeToAllocate += sizeof(TaskDialogConfig);
                Align(ref sizeToAllocate);

                // Strings in TasDialogConfig
                sizeToAllocate += SizeOfString(this.Title);
                sizeToAllocate += SizeOfString(this.MainInstruction);
                sizeToAllocate += SizeOfString(this.Content);
                sizeToAllocate += SizeOfString(this.Footer);
                sizeToAllocate += SizeOfString(this.VerificationText);
                sizeToAllocate += SizeOfString(this.ExpandedInformation);
                sizeToAllocate += SizeOfString(this.ExpandedControlText);
                sizeToAllocate += SizeOfString(this.CollapsedControlText);
                Align(ref sizeToAllocate);

                // Buttons array
                if (this.currentCustomButtons?.Length > 0)
                {
                    sizeToAllocate += sizeof(TaskDialogButtonStruct) * this.currentCustomButtons.Length;
                    Align(ref sizeToAllocate);
                    // Strings in buttons array
                    for (int i = 0; i < this.currentCustomButtons.Length; i++)
                        sizeToAllocate += SizeOfString(this.currentCustomButtons[i].Text);
                    Align(ref sizeToAllocate);
                }

                // Radio buttons array
                if (this.currentRadioButtons?.Length > 0)
                {
                    sizeToAllocate += sizeof(TaskDialogButtonStruct) * this.currentRadioButtons.Length;
                    Align(ref sizeToAllocate);
                    // Strings in radio buttons array
                    for (int i = 0; i < this.currentRadioButtons.Length; i++)
                        sizeToAllocate += SizeOfString(this.currentRadioButtons[i].Text);
                    Align(ref sizeToAllocate);
                }

                // Allocate the memory block. We add additional bytes to ensure we can
                // align the pointer to IntPtr.Size.
                ptrToFree = Marshal.AllocHGlobal((IntPtr)(sizeToAllocate + IntPtr.Size - 1));
                try
                {
                    // Align the pointer before using it. This is important since we also
                    // started with an aligned "address" value (0) when calculating the
                    // required allocation size.
                    var currentPtr = (byte*)ptrToFree;
                    Align(ref currentPtr);
                    ptrTaskDialogConfig = (IntPtr)currentPtr;

                    ref var taskDialogConfig = ref *(TaskDialogConfig*)currentPtr;
                    currentPtr += sizeof(TaskDialogConfig);
                    Align(ref currentPtr);

                    // Assign the structure with the constructor syntax, which will
                    // automatically initialize its other members with their default
                    // value.
                    taskDialogConfig = new TaskDialogConfig()
                    {
                        cbSize = sizeof(TaskDialogConfig),
                        hwndParent = this.currentOwnerHwnd.Value,
                        dwFlags = flags,
                        dwCommonButtons = this.CommonButtons,
                        hMainIcon = this.currentMainIconIsFromHandle ?
                                this.MainIconHandle : (IntPtr)this.MainIcon,
                        hFooterIcon = this.currentFooterIconIsFromHandle ?
                                this.FooterIconHandle : (IntPtr)this.FooterIcon,
                        pszWindowTitle = MarshalString(this.Title),
                        pszMainInstruction = MarshalString(this.MainInstruction),
                        pszContent = MarshalString(this.Content),
                        pszFooter = MarshalString(this.Footer),
                        pszVerificationText = MarshalString(this.VerificationText),
                        pszExpandedInformation = MarshalString(this.ExpandedInformation),
                        pszExpandedControlText = MarshalString(this.ExpandedControlText),
                        pszCollapsedControlText = MarshalString(this.CollapsedControlText),
                        nDefaultButton = (this.DefaultCustomButton as TaskDialogCustomButton)?.ButtonID ??
                                (int)this.DefaultCommonButton,
                        nDefaultRadioButton = (this.DefaultRadioButton as TaskDialogRadioButton)?.ButtonID ?? 0,
                        pfCallback = callbackProcDelegatePtr,
                        lpCallbackData = this.instanceHandlePtr,
                        cxWidth = this.Width
                    };
                    Align(ref currentPtr);

                    // Buttons array
                    if (this.currentCustomButtons?.Length > 0)
                    {
                        var customButtonStructs = (TaskDialogButtonStruct*)currentPtr;
                        taskDialogConfig.pButtons = (IntPtr)customButtonStructs;
                        taskDialogConfig.cButtons = this.currentCustomButtons.Length;
                        currentPtr += sizeof(TaskDialogButtonStruct) * this.currentCustomButtons.Length;
                        Align(ref currentPtr);

                        for (int i = 0; i < this.currentCustomButtons.Length; i++)
                        {
                            var currentCustomButton = this.currentCustomButtons[i];
                            customButtonStructs[i] = new TaskDialogButtonStruct()
                            {
                                nButtonID = currentCustomButton.ButtonID.Value,
                                pszButtonText = MarshalString(currentCustomButton.Text)
                            };
                        }
                        Align(ref currentPtr);
                    }

                    // Radio buttons array
                    if (this.currentRadioButtons?.Length > 0)
                    {
                        var customRadioButtonStructs = (TaskDialogButtonStruct*)currentPtr;
                        taskDialogConfig.pRadioButtons = (IntPtr)customRadioButtonStructs;
                        taskDialogConfig.cRadioButtons = this.currentRadioButtons.Length;
                        currentPtr += sizeof(TaskDialogButtonStruct) * this.currentRadioButtons.Length;
                        Align(ref currentPtr);

                        for (int i = 0; i < this.currentRadioButtons.Length; i++)
                        {
                            var currentCustomButton = this.currentRadioButtons[i];
                            customRadioButtonStructs[i] = new TaskDialogButtonStruct()
                            {
                                nButtonID = currentCustomButton.ButtonID.Value,
                                pszButtonText = MarshalString(currentCustomButton.Text)
                            };
                        }
                        Align(ref currentPtr);
                    }

                    Debug.Assert(currentPtr == (long)ptrTaskDialogConfig + sizeToAllocate);
                    

                    IntPtr MarshalString(string str)
                    {
                        if (str == null)
                            return IntPtr.Zero;

                        fixed (char* strPtr = str)
                        {
                            // Copy the string and a NULL character.
                            long bytesToCopy = SizeOfString(str);
                            Buffer.MemoryCopy(
                                    strPtr,
                                    currentPtr,
                                    bytesToCopy,
                                    bytesToCopy - sizeof(char));
                            ((char*)currentPtr)[str.Length] = '\0';

                            var ptrToReturn = currentPtr;
                            currentPtr += bytesToCopy;
                            return (IntPtr)ptrToReturn;
                        }
                    }
                }
                catch
                {
                    Marshal.FreeHGlobal(ptrToFree);
                    throw;
                }


                void Align(ref byte* currentPtr, int? alignment = null)
                {
                    if (alignment <= 0)
                        throw new ArgumentOutOfRangeException(nameof(alignment));

                    // Align the pointer to the next align size. If not specified, we will
                    // use the pointer (register) size.
                    uint add = (uint)(alignment ?? IntPtr.Size) - 1;
                    if (IntPtr.Size == 8)
                        // Note: The latter cast is not redundant, even if VS says so!
                        currentPtr = (byte*)(((ulong)currentPtr + add) & ~(ulong)add);
                    else
                        currentPtr = (byte*)(((uint)currentPtr + add) & ~add);
                }

                long SizeOfString(string str)
                {
                    return str == null ? 0 : ((long)str.Length + 1) * sizeof(char);
                }
            }
        }

        private void ApplyButtonInitialization()
        {
            // Apply current properties of buttons after the dialog has been created.
            if (this.currentCustomButtons != null)
            {
                foreach (var btn in this.currentCustomButtons)
                {
                    if (!btn.Enabled)
                        btn.Enabled = false;
                    if (btn.ElevationRequired)
                        btn.ElevationRequired = true;
                }
            }

            if (this.currentRadioButtons != null)
            {
                foreach (var btn in this.currentRadioButtons)
                {
                    if (!btn.Enabled)
                        btn.Enabled = false;
                }
            }

            // Check if we need to update the icon.
            if (!this.currentMainIconIsFromHandle &&
                    this.MainUpdateIcon != default &&
                    this.MainIcon != this.MainUpdateIcon)
            {
                CheckUpdateIcon(
                        TaskDialogUpdateElements.MainIcon,
                        TaskDialogUpdateElements.MainIcon,
                        TaskDialogIconElement.Main,
                        (IntPtr)this.MainUpdateIcon);
            }
        }

        private bool GetFlag(TaskDialogFlags flag)
        {
            return (this.flags & flag) == flag;
        }

        private void SetFlag(TaskDialogFlags flag, bool value)
        {
            if (value)
                this.flags |= flag;
            else
                this.flags &= ~flag;
        }

        private void DenyIfDialogNotActive()
        {
            if (this.hwndDialog == IntPtr.Zero)
                throw new InvalidOperationException(
                        "Can only update the state of a task dialog while it is active.");
        }

        private void SendTaskDialogMessage(TaskDialogMessage message, int wParam, IntPtr lParam)
        {
            DenyIfDialogNotActive();

            NativeMethods.SendMessage(
                    this.hwndDialog,
                    (int)message,
                    (IntPtr)wParam,
                    lParam);
        }

        private void SetButtonElevationRequiredStateCore(int buttonID, bool requiresElevation)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.SetButtonElevationRequiredState,
                    buttonID,
                    (IntPtr)(requiresElevation ? 1 : 0));
        }

        private void SetButtonEnabledCore(int buttonID, bool enable)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.EnableButton,
                    buttonID,
                    (IntPtr)(enable ? 1 : 0));
        }

        /// <summary>
        /// Enables or disables a radio button of an active task dialog.
        /// </summary>
        /// <param name="radioButtonID"></param>
        /// <param name="enable"></param>
        private void SetRadioButtonEnabledCore(int radioButtonID, bool enable)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.EnableRadioButton,
                    radioButtonID,
                    (IntPtr)(enable ? 1 : 0));
        }

        private void ClickButtonCore(int buttonID)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.ClickButton,
                    buttonID,
                    IntPtr.Zero);
        }

        private void ClickRadioButtonCore(int radioButtonID)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.ClickRadioButton, 
                    radioButtonID,
                    IntPtr.Zero);
        }

        private void CheckUpdateText(
                TaskDialogUpdateElements updateFlags,
                TaskDialogUpdateElements flagToCheck,
                TaskDialogElements element,
                string text)
        {
            if ((updateFlags & flagToCheck) == flagToCheck)
            {
                var strPtr = Marshal.StringToHGlobalUni(text);
                try
                {
                    // Note: SetElementText will resize the dialog while UpdateElementText will
                    // not (which would lead to clipped controls), so we use the former.
                    SendTaskDialogMessage(TaskDialogMessage.SetElementText, (int)element, strPtr);
                }
                finally
                {
                    // We can now free the memory because SendMessage does not return until the
                    // message has been processed.
                    Marshal.FreeHGlobal(strPtr);
                }
            }
        }

        private void CheckUpdateIcon(
                TaskDialogUpdateElements updateFlags,
                TaskDialogUpdateElements flagToCheck,
                TaskDialogIconElement element,
                IntPtr icon)
        {
            if ((updateFlags & flagToCheck) == flagToCheck)
            {
                SendTaskDialogMessage(TaskDialogMessage.UpdateIcon, (int)element, icon);
            }
        }
    }
}
