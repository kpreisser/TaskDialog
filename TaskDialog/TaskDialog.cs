using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

using TaskDialogNotification = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_NOTIFICATIONS;
using TaskDialogMessage = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_MESSAGES;
using TaskDialogConfig = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOGCONFIG;
using TaskDialogButtonStruct = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_BUTTON;
using TaskDialogTextElement = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_ELEMENTS;
using TaskDialogIconElement = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_ICON_ELEMENTS;

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
    [ToolboxItem(true)]
    [DefaultProperty(nameof(CurrentContents))]
    public partial class TaskDialog : Component
#if !NET_STANDARD
        , System.Windows.Forms.IWin32Window
#endif
    {        
        /// <summary>
        /// The delegate for the callback handler (that calls
        /// <see cref="HandleTaskDialogCallback"/>) from which the native function
        /// pointer <see cref="callbackProcDelegatePtr"/> is created. 
        /// </summary>
        /// <remarks>
        /// We must store this delegate (and prevent it from being garbage-collected)
        /// to ensure the function pointer doesn't become invalid.
        /// </remarks>
        private static readonly TaskDialogNativeMethods.PFTASKDIALOGCALLBACK callbackProcDelegate;

        /// <summary>
        /// The function pointer created from <see cref="callbackProcDelegate"/>.
        /// </summary>
        private static readonly IntPtr callbackProcDelegatePtr;


        /// <summary>
        /// Window handle of the task dialog when it is being shown.
        /// </summary>
        private IntPtr hwndDialog;

        /// <summary>
        /// The <see cref="IntPtr"/> of a <see cref="GCHandle"/> that represents this
        /// <see cref="TaskDialog"/> instance.
        /// </summary>
        private IntPtr instanceHandlePtr;

        private TaskDialogContents currentContents;

        private TaskDialogContents boundContents;

        private bool waitingForNavigatedEvent;

        /// <summary>
        /// Stores a value that indicates if the
        /// <see cref="TaskDialogContents.Created"/> event has been called for the
        /// current <see cref="TaskDialogContents"/>.
        /// </summary>
        /// <remarks>
        /// This is used to prevent raising the 
        /// <see cref="TaskDialogContents.Destroying"/> event without raising the
        /// <see cref="TaskDialogContents.Created"/> event first (e.g. if navigation
        /// fails).
        /// </remarks>
        private bool raisedContentsCreated;

        /// <summary>
        /// A counter which is used to determine whether the dialog has been navigated
        /// while being in a <see cref="TaskDialogNotification.TDN_BUTTON_CLICKED"/> handler.
        /// </summary>
        /// <remarks>
        /// When the dialog navigates within a ButtonClicked handler, the handler should
        /// always return S_FALSE to prevent the dialog from applying the button that
        /// raised the handler as dialog result. Otherwise, this can lead to memory access
        /// problems like <see cref="AccessViolationException"/>s, especially if the
        /// previous dialog contents had radio buttons (but the new ones do not).
        /// 
        /// See the comment in <see cref="HandleTaskDialogCallback"/> for more
        /// information.
        /// 
        /// When the dialog navigates, it sets the <c>navigationIndex</c> to the current
        /// <c>stackCount</c> value, so that the ButtonClicked handler can determine
        /// if the dialog has been navigated after it was called.
        /// Tracking the stack count and navigation index is necessary as there
        /// can be multiple ButtonClicked handlers on the call stack, for example
        /// if a ButtonClicked handler runs the message loop so that new click events
        /// can be processed.
        /// </remarks>
        private (int stackCount, int navigationIndex) buttonClickNavigationCounter;

        //private bool resultCheckBoxChecked;

        private bool suppressButtonClickedEvent;


        /// <summary>
        /// Occurs after the task dialog has been created but before it is displayed.
        /// </summary>
        /// <remarks>
        /// Note: The dialog will not show until this handler returns (even if the
        /// handler would run the message loop).
        /// </remarks>
        public event EventHandler Opened;

        /// <summary>
        /// Occurs when the task dialog is about to be destroyed.
        /// </summary>
        public event EventHandler Closing;

        /// <summary>
        /// Occurs after the task dialog has navigated.
        /// </summary>
        /// <remarks>
        /// Instead of handling this event (which will be called for all navigations
        /// of this dialog), you can also handle the
        /// <see cref="TaskDialogContents.Created"/> event that will only occur after the
        /// dialog navigated to that specific <see cref="TaskDialogContents"/> instance.
        /// </remarks>
        public event EventHandler Navigated;

        //// These events were removed since they are also available in the
        //// TaskDialogContents, and are specific to the contents (not to the dialog).

        ///// <summary>
        ///// Occurs when the user presses F1 while the dialog has focus, or when the
        ///// user clicks the <see cref="TaskDialogButtons.Help"/> button.
        ///// </summary>
        //public event EventHandler Help;

        ///// <summary>
        ///// 
        ///// </summary>
        //public event EventHandler<TaskDialogHyperlinkClickedEventArgs> HyperlinkClicked;

        ///// <summary>
        ///// 
        ///// </summary>
        //public event EventHandler<TaskDialogTimerTickEventArgs> TimerTick;


        static TaskDialog()
        {
            // Create a delegate for the callback, and get a function pointer for it.
            // Because this will allocate some memory required to store the native
            // code for the function pointer, we only do this once by using a static
            // function, and then identify the actual TaskDialog instance by using a
            // GCHandle in the reference data field (like an object pointer).
            callbackProcDelegate = (hWnd, notification, wParam, lParam, referenceData) =>
                    ((TaskDialog)GCHandle.FromIntPtr(referenceData).Target)
                    .HandleTaskDialogCallback(hWnd, notification, wParam, lParam);

            callbackProcDelegatePtr = Marshal.GetFunctionPointerForDelegate(
                    callbackProcDelegate);
        }


        /// <summary>
        /// 
        /// </summary>
        public TaskDialog()
            : base()
        {
            // TaskDialog is only supported on Windows.
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException();
        }

        /// <summary>
        /// 
        /// </summary>
        public TaskDialog(TaskDialogContents contents)
            : this()
        {
            this.currentContents = contents ??
                    throw new ArgumentNullException(nameof(contents));
        }


        /// <summary>
        /// Gets the window handle of the task dialog window, or <see cref="IntPtr.Zero"/>
        /// if the dialog is currently not being shown.
        /// </summary>
        /// <remarks>
        /// When showing the dialog, the handle will be available first when the
        /// <see cref="Opened"/> event occurs, and last when the
        /// <see cref="Closing"/> event occurs after which you shouldn't use it any more.
        /// </remarks>
        [Browsable(false)]
        public IntPtr Handle
        {
            get => this.hwndDialog;
        }

        /// <summary>
        /// Gets or sets the <see cref="TaskDialogContents"/> instance that represents
        /// the contents which this task dialog will display.
        /// </summary>
        /// <remarks>
        /// By setting this property while the task dialog is displayed, it will
        /// recreate its contents from the specified <see cref="TaskDialogContents"/>
        /// ("navigation"). After the dialog is navigated, the <see cref="Navigated"/>
        /// and the <see cref="TaskDialogContents.Created"/> events will occur.
        /// 
        /// Please note that you cannot update the task dialog or its controls
        /// directly after navigating it. You will need to wait for one of the mentioned
        /// events to occur before you can update the dialog or its controls.
        /// </remarks>
        [Category("Contents")]
        [Description("Contains the current contents of the Task Dialog.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TaskDialogContents CurrentContents
        {
            get => this.currentContents ??
                    (this.currentContents = new TaskDialogContents());

            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (this.DialogIsShown)
                {
                    // Try to navigate the dialog. This will validate the new contents
                    // and assign them only if they are OK.
                    Navigate(value);
                }
                else
                {
                    this.currentContents = value;
                }
            }
        }

        ///// <summary>
        ///// If <see cref="ResultCustomButton"/> is null, this field contains the
        ///// <see cref="TaskDialogResult"/> of the common buttons that was pressed.
        ///// </summary>
        //public TaskDialogCommonButton ResultCommonButton
        //{
        //    get => this.resultCommonButton;
        //}

        ///// <summary>
        ///// If not null, contains the custom button that was pressed. Otherwise, 
        ///// <see cref="ResultCommonButton"/> contains the common button that was pressed.
        ///// </summary>
        //public TaskDialogCustomButton ResultCustomButton
        //{
        //    get => this.resultCustomButton;
        //}

        ///// <summary>
        ///// After the <see cref="Show(IntPtr)"/> method returns, will contain the
        ///// <see cref="TaskDialogRadioButton"/> that the user has selected, or <c>null</c>
        ///// if none was selected.
        ///// </summary>
        //public TaskDialogRadioButton ResultRadioButton
        //{
        //    get => this.resultRadioButton;
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        //public bool ResultCheckBoxChecked
        //{
        //    get => this.resultCheckBoxChecked;
        //}


        /// <summary>
        /// Gets a value that indicates whether the native task dialog window has been
        /// created and its handle is available using the <see cref="Handle"/> property.
        /// </summary>
        [Browsable(false)]
        internal bool DialogIsShown
        {
            get => this.hwndDialog != IntPtr.Zero;
        }

        internal bool WaitingForNavigatedEvent
        {
            get => this.waitingForNavigatedEvent;
        }

        /// <summary>
        /// Gets or sets the current count of stack frames that are in the
        /// <see cref="TaskDialogRadioButton.CheckedChanged"/> event for the
        /// current task dialog.
        /// </summary>
        /// <remarks>
        /// This is used by the <see cref="TaskDialogRadioButton.Checked"/> setter
        /// so that it can disallow the change when the count is greater than zero.
        /// Additionally, it is used to deny navigation of the task dialog in that
        /// case.
        /// </remarks>
        internal int RadioButtonClickedStackCount
        {
            get;
            set;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                string text,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = TaskDialogIcon.None)
        {
            return Show(IntPtr.Zero, text, instruction, title, buttons, icon);
        }

#if !NET_STANDARD
        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner">The owner window, or <c>null</c> to show a modeless dialog.</param>
        /// <param name="text"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                System.Windows.Forms.IWin32Window owner,
                string text,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = TaskDialogIcon.None)
        {
            return Show(GetWindowHandle(owner), text, instruction, title, buttons, icon);
        }
#endif

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hwndOwner">
        /// The handle of the owner window, or <see cref="IntPtr.Zero"/> to show a
        /// modeless dialog.
        /// </param>
        /// <param name="text"></param>
        /// <param name="instruction"></param>
        /// <param name="title"></param>
        /// <param name="buttons"></param>
        /// <param name="icon"></param>
        /// <returns></returns>
        public static TaskDialogResult Show(
                IntPtr hwndOwner,
                string text,
                string instruction = null,
                string title = null,
                TaskDialogButtons buttons = TaskDialogButtons.OK,
                TaskDialogIcon icon = TaskDialogIcon.None)
        {
            using (var dialog = new TaskDialog(new TaskDialogContents()
            {
                Text = text,
                Instruction = instruction,
                Title = title,
                Icon = icon,
                CommonButtons = buttons
            }))
            {
                return ((TaskDialogCommonButton)dialog.Show(hwndOwner)).Result;
            }
        }


        private static void FreeConfig(IntPtr ptrToFree)
        {
            Marshal.FreeHGlobal(ptrToFree);
        }

#if !NET_STANDARD
        private static IntPtr GetWindowHandle(System.Windows.Forms.IWin32Window window)
        {
            return window.Handle;
        }
#endif


        /// <summary>
        /// Shows the task dialog.
        /// </summary>
        public TaskDialogButton Show()
        {
            return Show(IntPtr.Zero);
        }

#if !NET_STANDARD
        /// <summary>
        /// Shows the task dialog.
        /// </summary>
        /// <remarks>
        /// Showing the dialog will bind the <see cref="CurrentContents"/> and their
        /// controls until this method returns.
        /// </remarks>
        /// <param name="owner">The owner window, or <c>null</c> to show a modeless dialog.</param>
        public TaskDialogButton Show(System.Windows.Forms.IWin32Window owner)
        {
            return Show(GetWindowHandle(owner));
        }
#endif

        /// <summary>
        /// Shows the task dialog.
        /// </summary>
        /// <remarks>
        /// Showing the dialog will bind the <see cref="CurrentContents"/> and their
        /// controls until this method returns or the dialog is navigated.
        /// </remarks>
        /// <param name="hwndOwner">
        /// The handle of the owner window, or <see cref="IntPtr.Zero"/> to show a
        /// modeless dialog.
        /// </param>
        public TaskDialogButton Show(IntPtr hwndOwner)
        {
            // Recursive Show() is not possible because a TaskDialog instance can only
            // represent a single native dialog.
            if (this.instanceHandlePtr != IntPtr.Zero)
                throw new InvalidOperationException(
                        $"This {nameof(TaskDialog)} instance is already showing a task dialog.");

            // Validate the config.
            this.CurrentContents.Validate(this);

            // Allocate a GCHandle which we will use for the callback data.
            var instanceHandle = GCHandle.Alloc(this);
            try
            {
                this.instanceHandlePtr = GCHandle.ToIntPtr(instanceHandle);

                // Clear the previous result properties.
                //this.resultCheckBoxChecked = default;
                //this.resultCommonButton = default;
                //this.resultCustomButton = null;
                //this.resultRadioButton = null;

                // Bind the contents and allocate the memory.
                BindAndAllocateConfig(
                       hwndOwner,
                       out var ptrToFree,
                       out var ptrTaskDialogConfig);
                try
                {
                    int ret = TaskDialogNativeMethods.TaskDialogIndirect(
                            ptrTaskDialogConfig,
                            out int resultButtonID,
                            out int resultRadioButtonID,
                            out bool resultCheckBoxChecked);

                    //// Note: If a exception occurs here when hwndDialog is not 0, it means the TaskDialogIndirect
                    //// run the event loop and called a WndProc e.g. from a window, whose event handler threw an
                    //// exception. In that case we cannot catch and marshal it to a HResult, so the CLR will
                    //// manipulate the managed stack so that it doesn't contain the transition to and from native
                    //// code. However, the TaskDialog still calls our TaskDialogCallbackProc (by dispatching
                    //// messages to the WndProc) when the current event handler from WndProc returns, but as
                    //// we already freed the GCHandle, a NRE will occur.

                    //// This is OK because the same issue occurs when using a message box with WPF or WinForms:
                    //// If you do MessageBox.Show() wrapped in a try/catch on a button click, and before calling
                    //// .Show() create and start a timer which stops and throws an exception on its Tick event,
                    //// the application will crash with an AccessViolationException as soon as you close
                    //// the MessageBox.

                    // Marshal.ThrowExceptionForHR will use the IErrorInfo on the
                    // current thread if it exists, in which case it ignores the
                    // error code. Therefore we only call it if the HResult is not
                    // a success code to avoid incorrect exceptions being thrown.
                    if (ret < 0)
                        Marshal.ThrowExceptionForHR(ret);

                    TaskDialogButton resultingButton;
                    if (resultButtonID >= TaskDialogContents.CustomButtonStartID)
                    {
                        resultingButton = this.boundContents.CustomButtons
                                [resultButtonID - TaskDialogContents.CustomButtonStartID];
                    }
                    else
                    {
                        var result = (TaskDialogResult)resultButtonID;

                        // Check we have a button with the result that was returned. This might
                        // not always be the case, e.g. when specifying AllowCancel but not
                        // adding a 'Cancel' button. If we don't have such button, we
                        // simply create a new one.
                        if (this.boundContents.CommonButtons.Contains(result))
                            resultingButton = this.boundContents.CommonButtons[result];
                        else
                            resultingButton = new TaskDialogCommonButton(result);
                    }

                    // Return the button that was clicked.
                    return resultingButton;
                }
                finally
                {
                    // Free the memory.
                    FreeConfig(ptrToFree);

                    // Ensure to clear the flag if a navigation did not complete.
                    this.waitingForNavigatedEvent = false;
                    // Also, ensure the window handle and the raisedContentsCreated
                    // flag are is cleared even if the 'Destroyed' notification did
                    // not occur (although that should only happen when there was an
                    // exception).
                    this.hwndDialog = IntPtr.Zero;
                    this.raisedContentsCreated = false;

                    // Unbind the contents. The 'Destroying' event of the TaskDialogContent
                    // will already have been called from the callback.
                    this.boundContents.Unbind();
                    this.boundContents = null;

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

        //// Messages that can be sent to the dialog while it is being shown.

        /// <summary>
        /// Closes the shown task dialog with a 
        /// <see cref="TaskDialogResult.Cancel"/> result.
        /// </summary>
        public void Close()
        {
            this.suppressButtonClickedEvent = true;
            try
            {
                // Send a click button message with the cancel result.
                ClickButton((int)TaskDialogResult.Cancel);
            }
            finally
            {
                this.suppressButtonClickedEvent = false;
            }
        }


        /// <summary>
        /// While the dialog is being shown, switches the progress bar mode to either a
        /// marquee progress bar or to a regular progress bar.
        /// For a marquee progress bar, you can enable or disable the marquee using
        /// <see cref="SetProgressBarMarquee(bool, int)"/>.
        /// </summary>
        /// <param name="marqueeProgressBar"></param>
        internal void SwitchProgressBarMode(bool marqueeProgressBar)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_SET_MARQUEE_PROGRESS_BAR,
                    marqueeProgressBar ? 1 : 0,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is being shown, enables or disables progress bar marquee when
        /// an marquee progress bar is displayed.
        /// </summary>
        /// <param name="enableMarquee"></param>
        /// <param name="animationSpeed">
        /// The time in milliseconds between marquee animation updates. If <c>0</c>, the
        /// animation will be updated every 30 milliseconds.
        /// </param>
        internal void SetProgressBarMarquee(bool enableMarquee, int animationSpeed = 0)
        {
            if (animationSpeed < 0)
                throw new ArgumentOutOfRangeException(nameof(animationSpeed));

            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_SET_PROGRESS_BAR_MARQUEE,
                    enableMarquee ? 1 : 0,
                    (IntPtr)animationSpeed);
        }

        /// <summary>
        /// While the dialog is being shown, sets the progress bar range.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <remarks>
        /// The default range is 0 to 100.
        /// </remarks>
        internal unsafe void SetProgressBarRange(int min, int max)
        {
            if (min < 0 || min > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(min));
            if (max < 0 || max > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(max));

            // Note: The MAKELPARAM macro converts the value to an unsigned int
            // before converting it to a pointer, so we should do the same.
            // However, this means we cannot convert the value directly to an
            // IntPtr; instead we need to first convert it to a pointer type
            // which requires unsafe code.
            // TODO: Use nuint instead of void* when it is available.
            var param = (IntPtr)(void*)unchecked((uint)(min | (max << 0x10)));

            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_SET_PROGRESS_BAR_RANGE,
                    0,
                    param);
        }

        /// <summary>
        /// While the dialog is being shown, sets the progress bar position.
        /// </summary>
        /// <param name="pos"></param>
        internal void SetProgressBarPosition(int pos)
        {
            if (pos < 0 || pos > ushort.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(pos));

            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_SET_PROGRESS_BAR_POS,
                    pos,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is being shown, sets the progress bar state.
        /// </summary>
        /// <param name="state"></param>
        internal void SetProgressBarState(int state)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_SET_PROGRESS_BAR_STATE,
                    state,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is being shown, sets the checkbox to the specified
        /// state.
        /// </summary>
        /// <param name="isChecked"></param>
        /// <param name="focus"></param>
        internal void ClickCheckBox(bool isChecked, bool focus = false)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_CLICK_VERIFICATION,
                    isChecked ? 1 : 0,
                    (IntPtr)(focus ? 1 : 0));
        }

        internal void SetButtonElevationRequiredState(int buttonID, bool requiresElevation)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_SET_BUTTON_ELEVATION_REQUIRED_STATE,
                    buttonID,
                    (IntPtr)(requiresElevation ? 1 : 0));
        }

        internal void SetButtonEnabled(int buttonID, bool enable)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_ENABLE_BUTTON,
                    buttonID,
                    (IntPtr)(enable ? 1 : 0));
        }

        internal void SetRadioButtonEnabled(int radioButtonID, bool enable)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_ENABLE_RADIO_BUTTON,
                    radioButtonID,
                    (IntPtr)(enable ? 1 : 0));
        }

        internal void ClickButton(int buttonID)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_CLICK_BUTTON,
                    buttonID,
                    IntPtr.Zero);
        }

        internal void ClickRadioButton(int radioButtonID)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_CLICK_RADIO_BUTTON,
                    radioButtonID,
                    IntPtr.Zero);
        }

        internal void UpdateTextElement(
                TaskDialogTextElement element,
                string text)
        {
            DenyIfDialogNotShownOrWaitingForNavigatedEvent();

            // Note: Instead of null, we must specify the empty string; otherwise
            // the update would be ignored.
            var textPtr = Marshal.StringToHGlobalUni(text ?? string.Empty);
            try
            {
                // Note: SetElementText will resize the dialog while UpdateElementText
                // will not (which would lead to clipped controls), so we use the
                // former.
                SendTaskDialogMessage(
                        TaskDialogMessage.TDM_SET_ELEMENT_TEXT,
                        (int)element,
                        textPtr);
            }
            finally
            {
                // We can now free the memory because SendMessage does not return
                // until the message has been processed.
                Marshal.FreeHGlobal(textPtr);
            }
        }

        internal void UpdateIconElement(
                TaskDialogIconElement element,
                IntPtr icon)
        {
            // Note: Updating the icon doesn't cause the task dialog to update
            // its size. For example, if you initially didn't specify an icon
            // but later want to set one, the dialog contents might get clipped.
            // To fix this, we might want to call UpdateSize() that forces the
            // task dialog to update its size.
            SendTaskDialogMessage(
                    TaskDialogMessage.TDM_UPDATE_ICON,
                    (int)element,
                    icon);
        }

        internal void UpdateTitle(string title)
        {
            DenyIfDialogNotShownOrWaitingForNavigatedEvent();

            // TODO: Because we use SetWindowText() directly (as there is no task
            // dialog message for setting the title), there is a small discrepancy
            // between specifying an empty title in the TASKDIALOGCONFIG structure
            // and setting an empty title with this method: An empty string (or null)
            // in the TASKDIALOGCONFIG struture causes the dialog title to be the
            // executable name (e.g. "MyApplication.exe"), but using an empty string
            // (or null) with this method causes the window title to be empty.
            // We could replicate the Task Dialog behavior by also using the
            // executable's name as title if the string is null or empty.
            if (!TaskDialogNativeMethods.SetWindowText(this.hwndDialog, title))
                Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="e"></param>
        //protected void OnHelp(EventArgs e)
        //{
        //    this.Help?.Invoke(this, e);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="e"></param>
        //protected void OnHyperlinkClicked(TaskDialogHyperlinkClickedEventArgs e)
        //{
        //    this.HyperlinkClicked?.Invoke(this, e);
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="e"></param>
        //protected void OnTimerTick(TaskDialogTimerTickEventArgs e)
        //{
        //    this.TimerTick?.Invoke(this, e);
        //}


        private int HandleTaskDialogCallback(
                IntPtr hWnd,
                TaskDialogNotification notification,
                IntPtr wParam,
                IntPtr lParam)
        {
            // Set the hWnd as this may be the first time that we get it.
            this.hwndDialog = hWnd;

            switch (notification)
            {
                case TaskDialogNotification.TDN_CREATED:
                    this.boundContents.ApplyInitialization();

                    this.OnOpened(EventArgs.Empty);

                    this.raisedContentsCreated = true;
                    this.boundContents.OnCreated(EventArgs.Empty);
                    break;

                case TaskDialogNotification.TDN_DESTROYED:
                    // Only raise the 'Destroying' event if we previously raised the
                    // 'Created' event.
                    if (this.raisedContentsCreated)
                    {
                        this.raisedContentsCreated = false;
                        this.boundContents.OnDestroying(EventArgs.Empty);
                    }

                    this.OnClosing(EventArgs.Empty);

                    // Clear the dialog handle, because according to the docs, we must not
                    // continue to send any notifications to the dialog after the callback
                    // function has returned from being called with the 'Destroyed'
                    // notification.
                    // Note: When multiple dialogs are shown (so Show() will occur multiple
                    // times in the call stack) and a previously opened dialog is closed,
                    // the Destroyed notification for the closed dialog will only occur after
                    // the newer dialogs are also closed.
                    this.hwndDialog = IntPtr.Zero;
                    break;

                case TaskDialogNotification.TDN_NAVIGATED:
                    this.waitingForNavigatedEvent = false;
                    this.boundContents.ApplyInitialization();

                    this.OnNavigated(EventArgs.Empty);

                    this.raisedContentsCreated = true;
                    this.boundContents.OnCreated(EventArgs.Empty);
                    break;

                case TaskDialogNotification.TDN_HYPERLINK_CLICKED:
                    string link = Marshal.PtrToStringUni(lParam);

                    var eventArgs = new TaskDialogHyperlinkClickedEventArgs(link);
                    //this.OnHyperlinkClicked(eventArgs);
                    this.boundContents.OnHyperlinkClicked(eventArgs);
                    break;

                case TaskDialogNotification.TDN_BUTTON_CLICKED:
                    if (this.suppressButtonClickedEvent)
                        return TaskDialogNativeMethods.S_OK;

                    int buttonID = wParam.ToInt32();

                    // Check if the button is part of the custom buttons.
                    var button = null as TaskDialogButton;
                    if (buttonID >= TaskDialogContents.CustomButtonStartID)
                    {
                        button = this.boundContents.CustomButtons
                                [buttonID - TaskDialogContents.CustomButtonStartID];
                    }
                    else
                    {
                        var result = (TaskDialogResult)buttonID;
                        if (this.boundContents.CommonButtons.Contains(result))
                            button = this.boundContents.CommonButtons[result];
                    }

                    // Note: When the event handler returns true but the dialog was
                    // navigated within the handler, the buttonID of the handler
                    // would be set as the dialog's result even if this ID is from
                    // the dialog contents before the dialog was navigated.
                    // (To fix this, in this case we could cache the button instance
                    // and its ID, so that when Show() returns, it can check if the
                    // button ID equals the last handled button, and use that
                    // instance in that case.)
                    // However, also memory access problems like
                    // AccessViolationExceptions seem to occur in this situation
                    // (especially if the dialog also had radio buttons before the
                    // navigation; these would also be set as result of the dialog),
                    // probably because this scenario isn't an expected usage of
                    // the native TaskDialog.
                    // To fix the memory access problems, we simply always return
                    // S_FALSE when the dialog was navigated within the ButtonClicked
                    // event handler. This also avoids the need to cache the last
                    // handled button instance because it can no longer happen that
                    // TaskDialogIndirect() returns a buttonID that is no longer
                    // present in the navigated TaskDialogContents.
                    bool handlerResult;
                    checked {
                        this.buttonClickNavigationCounter.stackCount++;
                    }
                    try
                    {
                        handlerResult = button?.HandleButtonClicked() ?? true;

                        // Check if our stack frame was set to true, which means the
                        // dialog was navigated while we called the handler. In that
                        // case, we need to return S_FALSE to prevent the dialog from
                        // closing (and applying the previous ButtonID and RadioButtonID
                        // as results).
                        bool wasNavigated = this.buttonClickNavigationCounter.navigationIndex >=
                                this.buttonClickNavigationCounter.stackCount;
                        if (wasNavigated)
                            handlerResult = false;
                    }
                    finally
                    {
                        this.buttonClickNavigationCounter.stackCount--;
                        this.buttonClickNavigationCounter.navigationIndex = Math.Min(
                                this.buttonClickNavigationCounter.navigationIndex,
                                this.buttonClickNavigationCounter.stackCount);
                    }

                    return handlerResult ?
                            TaskDialogNativeMethods.S_OK :
                            TaskDialogNativeMethods.S_FALSE;

                case TaskDialogNotification.TDN_RADIO_BUTTON_CLICKED:
                    int radioButtonID = wParam.ToInt32();

                    var radioButton = this.boundContents.RadioButtons
                            [radioButtonID - TaskDialogContents.RadioButtonStartID];

                    radioButton.HandleRadioButtonClicked();
                    break;

                case TaskDialogNotification.TDN_EXPANDO_BUTTON_CLICKED:
                    this.boundContents.Expander.HandleExpandoButtonClicked(
                            wParam != IntPtr.Zero);
                    break;

                case TaskDialogNotification.TDN_VERIFICATION_CLICKED:
                    this.boundContents.CheckBox.HandleCheckBoxClicked(
                            wParam != IntPtr.Zero);
                    break;

                case TaskDialogNotification.TDN_HELP:
                    //this.OnHelp(EventArgs.Empty);
                    this.boundContents.OnHelp(EventArgs.Empty);
                    break;

                case TaskDialogNotification.TDN_TIMER:
                    // Note: The documentation specifies that wParam contains a DWORD,
                    // which might mean that on 64-bit platforms the highest bit (63)
                    // will be zero even if the DWORD has its highest bit (31) set. In
                    // that case, IntPtr.ToInt32() would throw an OverflowException.
                    // Therefore, we use .ToInt64() and then convert it to an int.
                    int ticks = IntPtr.Size == 8 ?
                            unchecked((int)wParam.ToInt64()) :
                            wParam.ToInt32();

                    var tickEventArgs = new TaskDialogTimerTickEventArgs(ticks);
                    //this.OnTimerTick(tickEventArgs);
                    this.boundContents.OnTimerTick(tickEventArgs);

                    return tickEventArgs.ResetTickCount ?
                            TaskDialogNativeMethods.S_FALSE :
                            TaskDialogNativeMethods.S_OK;
            }

            // Note: Previously, the code caught exceptions and returned
            // Marshal.GetHRForException(), so that the TaskDialog would be closed on an
            // unhandled exception and we could rethrow it after TaskDialogIndirect() returns.
            // However, this causes the debugger to not break at the original exception
            // location, and it is probably not desired that the Dialog is actually destroyed
            // because this would be inconsistent with the case when an unhandled exception
            // occurs in a different WndProc function not handled by the TaskDialog
            // (e.g. a WinForms/WPF Timer Tick event). Additionally, if you had multiple
            // (non-modal) dialogs showing and the exception would occur in the callback
            // of an outer dialog, it would not be rethrown until the inner dialogs
            // were also closed. Therefore, we don't catch exceptions (and return
            // a error HResult) any more.
            // Note: Currently, this means that a NRE will occur in the callback after
            // TaskDialog.Show() returns due to an unhandled exception because the
            // TaskDialog is still displayed (see comment in Show()).
            return TaskDialogNativeMethods.S_OK;
        }

        /// <summary>
        /// While the dialog is being shown, recreates the dialog from the specified
        /// <paramref name="contents"/>.
        /// </summary>
        /// <remarks>
        /// Note that you should not call this method in the <see cref="Opened"/>
        /// event because the task dialog is not yet displayed in that state.
        /// </remarks>
        private void Navigate(TaskDialogContents contents)
        {
            DenyIfDialogNotShownOrWaitingForNavigatedEvent();

            // Don't allow to navigate the dialog when we are in a RadioButtonClicked
            // notification, because the dialog doesn't seem to correctly handle this
            // (e.g. when running the event loop after navigation, an
            // AccessViolationException would occur after the handler returns).
            // See: https://github.com/dotnet/winforms/issues/146#issuecomment-466784079
            if (this.RadioButtonClickedStackCount > 0)
                throw new InvalidOperationException(
                        "Cannot navigate the dialog from within the " +
                        $"{nameof(TaskDialogRadioButton)}.{nameof(TaskDialogRadioButton.CheckedChanged)} " +
                        $"event of one of the radio buttons of the current task dialog.");

            // Validate the config.
            contents.Validate(this);

            // After validation passed, we can now unbind the current contents and
            // bind the new one.
            // Need to raise the "Destroying" event for the current contents. The
            // "Created" event for the new contents will occur from the callback.
            // Note: "this.raisedContentsCreated" should always be true here.
            if (this.raisedContentsCreated)
            {
                this.raisedContentsCreated = false;
                this.boundContents.OnDestroying(EventArgs.Empty);
            }

            this.boundContents.Unbind();
            this.boundContents = null;

            this.currentContents = contents;

            // Note: If this throws an OutOfMemoryException, we leave the previous
            // contents in the unbound state. We could solve this by re-binding the
            // previous contents in case of an exception.
            // Note: We don't need to specify the owner window handle again when
            // navigating.
            BindAndAllocateConfig(
                    IntPtr.Zero,
                    out var ptrToFree,
                    out var ptrTaskDialogConfig);
            try
            {
                // Note: If the task dialog cannot be recreated with the new contents,
                // the dialog will close and TaskDialogIndirect() returns with an
                // error code.
                SendTaskDialogMessage(
                        TaskDialogMessage.TDM_NAVIGATE_PAGE,
                        0,
                        ptrTaskDialogConfig);
            }
            finally
            {
                // We can now free the memory because SendMessage does not return
                // until the message has been processed.
                FreeConfig(ptrToFree);
            }

            // Indicate to the ButtonClicked handlers currently on the stack that
            // the dialog was navigated.
            this.buttonClickNavigationCounter.navigationIndex =
                    this.buttonClickNavigationCounter.stackCount;

            // Also, disallow updates until we received the Navigated event
            // because that messages would be lost.
            this.waitingForNavigatedEvent = true;
        }

        private unsafe void BindAndAllocateConfig(
                IntPtr hwndOwner,
                out IntPtr ptrToFree,
                out IntPtr ptrTaskDialogConfig)
        {
            var contents = this.CurrentContents;
            contents.Bind(
                    this,
                    out var flags,
                    out var commonButtonFlags,
                    out var iconValue,
                    out var footerIconValue,
                    out int defaultButtonID,
                    out int defaultRadioButtonID);
            this.boundContents = contents;
            try
            {
                checked
                {
                    // First, calculate the necessary memory size we need to allocate for all
                    // structs and strings.
                    // Note: Each Align() call when calculating the size must correspond with a
                    // Align() call when incrementing the pointer.
                    // Use a byte pointer so we can use byte-wise pointer arithmetics.
                    var sizeToAllocate = (byte*)0;
                    sizeToAllocate += sizeof(TaskDialogConfig);

                    // Strings in TasDialogConfig
                    Align(ref sizeToAllocate, sizeof(char));
                    sizeToAllocate += SizeOfString(contents.Title);
                    sizeToAllocate += SizeOfString(contents.Instruction);
                    sizeToAllocate += SizeOfString(contents.Text);
                    sizeToAllocate += SizeOfString(contents.CheckBox?.Text);
                    sizeToAllocate += SizeOfString(contents.Expander?.Text);
                    sizeToAllocate += SizeOfString(contents.Expander?.ExpandedButtonText);
                    sizeToAllocate += SizeOfString(contents.Expander?.CollapsedButtonText);
                    sizeToAllocate += SizeOfString(contents.Footer?.Text);

                    // Buttons array
                    if (contents.CustomButtons.Count > 0)
                    {
                        // Note: Theoretically we would not need to align the pointer here
                        // since the packing of the structure is set to 1. Note that this
                        // can cause an unaligned write when assigning the structure (the
                        // same happens with TaskDialogConfig).
                        Align(ref sizeToAllocate);
                        sizeToAllocate += sizeof(TaskDialogButtonStruct) * contents.CustomButtons.Count;

                        // Strings in buttons array
                        Align(ref sizeToAllocate, sizeof(char));
                        for (int i = 0; i < contents.CustomButtons.Count; i++)
                            sizeToAllocate += SizeOfString(contents.CustomButtons[i].GetResultingText());
                    }

                    // Radio buttons array
                    if (contents.RadioButtons.Count > 0)
                    {
                        // See comment above regarding alignment.
                        Align(ref sizeToAllocate);
                        sizeToAllocate += sizeof(TaskDialogButtonStruct) * contents.RadioButtons.Count;

                        // Strings in radio buttons array
                        Align(ref sizeToAllocate, sizeof(char));
                        for (int i = 0; i < contents.RadioButtons.Count; i++)
                            sizeToAllocate += SizeOfString(contents.RadioButtons[i].Text);
                    }

                    // Allocate the memory block. We add additional bytes to ensure we can
                    // align the returned pointer to IntPtr.Size (the biggest align size
                    // that we will use).
                    ptrToFree = Marshal.AllocHGlobal((IntPtr)(sizeToAllocate + (IntPtr.Size - 1)));
                    try
                    {
                        // Align the pointer before using it. This is important since we also
                        // started with an aligned "address" value (0) when calculating the
                        // required allocation size. We must use the same size that we added
                        // as additional size when allocating the memory.
                        var currentPtr = (byte*)ptrToFree;
                        Align(ref currentPtr);
                        ptrTaskDialogConfig = (IntPtr)currentPtr;

                        ref var taskDialogConfig = ref *(TaskDialogConfig*)currentPtr;
                        currentPtr += sizeof(TaskDialogConfig);

                        // Assign the structure with the constructor syntax, which will
                        // automatically initialize its other members with their default
                        // value.
                        Align(ref currentPtr, sizeof(char));
                        taskDialogConfig = new TaskDialogConfig()
                        {
                            cbSize = (uint)sizeof(TaskDialogConfig),
                            hwndParent = hwndOwner,
                            dwFlags = flags,
                            dwCommonButtons = commonButtonFlags,
                            mainIconUnion = iconValue,
                            footerIconUnion = footerIconValue,
                            pszWindowTitle = MarshalString(contents.Title),
                            pszMainInstruction = MarshalString(contents.Instruction),
                            pszContent = MarshalString(contents.Text),
                            pszVerificationText = MarshalString(contents.CheckBox?.Text),
                            pszExpandedInformation = MarshalString(contents.Expander?.Text),
                            pszExpandedControlText = MarshalString(contents.Expander?.ExpandedButtonText),
                            pszCollapsedControlText = MarshalString(contents.Expander?.CollapsedButtonText),
                            pszFooter = MarshalString(contents.Footer?.Text),
                            nDefaultButton = defaultButtonID,
                            nDefaultRadioButton = defaultRadioButtonID,
                            pfCallback = callbackProcDelegatePtr,
                            lpCallbackData = this.instanceHandlePtr,
                            cxWidth = checked((uint)contents.Width)
                        };

                        // Buttons array
                        if (contents.CustomButtons.Count > 0)
                        {
                            Align(ref currentPtr);
                            var customButtonStructs = (TaskDialogButtonStruct*)currentPtr;
                            taskDialogConfig.pButtons = (IntPtr)customButtonStructs;
                            taskDialogConfig.cButtons = (uint)contents.CustomButtons.Count;
                            currentPtr += sizeof(TaskDialogButtonStruct) * contents.CustomButtons.Count;

                            Align(ref currentPtr, sizeof(char));
                            for (int i = 0; i < contents.CustomButtons.Count; i++)
                            {
                                var currentCustomButton = contents.CustomButtons[i];
                                customButtonStructs[i] = new TaskDialogButtonStruct()
                                {
                                    nButtonID = currentCustomButton.ButtonID,
                                    pszButtonText = MarshalString(currentCustomButton.GetResultingText())
                                };
                            }
                        }

                        // Radio buttons array
                        if (contents.RadioButtons.Count > 0)
                        {
                            Align(ref currentPtr);
                            var customRadioButtonStructs = (TaskDialogButtonStruct*)currentPtr;
                            taskDialogConfig.pRadioButtons = (IntPtr)customRadioButtonStructs;
                            taskDialogConfig.cRadioButtons = (uint)contents.RadioButtons.Count;
                            currentPtr += sizeof(TaskDialogButtonStruct) * contents.RadioButtons.Count;

                            Align(ref currentPtr, sizeof(char));
                            for (int i = 0; i < contents.RadioButtons.Count; i++)
                            {
                                var currentCustomButton = contents.RadioButtons[i];
                                customRadioButtonStructs[i] = new TaskDialogButtonStruct()
                                {
                                    nButtonID = currentCustomButton.RadioButtonID,
                                    pszButtonText = MarshalString(currentCustomButton.Text)
                                };
                            }
                        }

                        Debug.Assert(currentPtr == (long)ptrTaskDialogConfig + sizeToAllocate);


                        IntPtr MarshalString(string str)
                        {
                            if (str == null)
                                return IntPtr.Zero;

                            fixed (char* strPtr = str)
                            {
                                // Copy the string. The C# language specification guarantees
                                // that a char* value produced by using the 'fixed'
                                // statement on a string always points to a null-terminated
                                // string, so we don't need to copy a NUL character
                                // separately.
                                long bytesToCopy = SizeOfString(str);
                                Buffer.MemoryCopy(
                                        strPtr,
                                        currentPtr,
                                        bytesToCopy,
                                        bytesToCopy);

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
            catch
            {
                // Unbind the contents, then rethrow the exception.
                this.boundContents.Unbind();
                this.boundContents = null;

                throw;
            }
        }

        private void DenyIfDialogNotShownOrWaitingForNavigatedEvent()
        {
            if (!this.DialogIsShown)
                throw new InvalidOperationException(
                        "Can only update the state of a task dialog while it is shown.");

            if (this.waitingForNavigatedEvent)
                throw new InvalidOperationException(
                        "Cannot update the task dialog directly after navigating it. " +
                        $"Please wait for the {nameof(TaskDialog)}.{nameof(this.Navigated)} " +
                        $"event or for the " +
                        $"{nameof(TaskDialogContents)}.{nameof(TaskDialogContents.Created)} " +
                        $"event to occur.");
        }

        private void SendTaskDialogMessage(TaskDialogMessage message, int wParam, IntPtr lParam)
        {
            DenyIfDialogNotShownOrWaitingForNavigatedEvent();

            TaskDialogNativeMethods.SendMessage(
                    this.hwndDialog,
                    (int)message,
                    // Note: When a negative 32-bit integer is converted to a
                    // 64-bit pointer, the high dword will be set to 0xFFFFFFFF.
                    // This is consistent with the conversion from int to
                    // WPARAM (in C) as shown in the Task Dialog documentation.
                    (IntPtr)wParam,
                    lParam);
        }

        /// <summary>
        /// Forces the task dialog to update its window size according to its contents.
        /// </summary>
        private void UpdateWindowSize()
        {
            // Force the task dialog to update its size by doing an arbitrary
            // update of one of its text elements (as the TDM_SET_ELEMENT_TEXT
            // causes the size/layout to be updated).
            // We use the MainInstruction because it cannot contain hyperlinks
            // (and therefore there is no risk that one of the links loses focus).
            UpdateTextElement(
                    TaskDialogTextElement.TDE_MAIN_INSTRUCTION,
                    this.boundContents.Instruction);
        }
    }
}
