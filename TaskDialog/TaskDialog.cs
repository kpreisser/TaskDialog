using System;
using System.Diagnostics;
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
        // Offset for user message types.
        private const int UserMessageOffset = 0x400;

        private const int HResultOk = 0x0; // S_OK

        private const int HResultFalse = 0x1; // S_FALSE


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

        private TaskDialogContents currentContents;

        private TaskDialogContents boundContents;

        /// <summary>
        /// Stores the last button with its ID for which the callback returned <c>true</c>
        /// when handling the <see cref="TaskDialogNotification.ButtonClicked"/>
        /// notification, so that when
        /// <see cref="NativeMethods.TaskDialogIndirect(IntPtr, out int, out int, out bool)"/>
        /// returns, the resulting button ID should be the one we stored here.
        /// </summary>
        private (TaskDialogButton button, int originalButtonID) lastHandledResultButton;

        //private bool resultVerificationCheckboxChecked;

        private bool suppressButtonClickedEvent;


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
        /// <remarks>
        /// Instead of handling this event (which will be called for all navigations
        /// of this dialog), you can also handle the
        /// <see cref="TaskDialogContents.Created"/> event that will only occur after the
        /// dialog navigated to that specific <see cref="TaskDialogContents"/> instance.
        /// </remarks>
        public event EventHandler Navigated;

        //// TODO: Maybe remove these events since they are also available in the TaskDialogContents,
        //// and are specific to the contents (not to the dialog).

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
            : this(new TaskDialogContents())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        public TaskDialog(TaskDialogContents contents)
        {
            // TaskDialog is only supported on Windows.
#if NET46
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
#else
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
#endif
                throw new PlatformNotSupportedException();

            this.currentContents = contents ??
                    throw new ArgumentNullException(nameof(contents));
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
        /// Gets or sets the <see cref="TaskDialogContents"/> instance that represents
        /// the contents which this task dialog will display.
        /// </summary>
        /// <remarks>
        /// By setting this property while the task dialog is displayed, it will completely
        /// recreate its contents from the specified <see cref="TaskDialogContents"/>
        /// ("navigation"). After the dialog is navigated, the <see cref="Navigated"/>
        /// and the <see cref="TaskDialogContents.Created"/> events will occur.
        /// </remarks>
        public TaskDialogContents CurrentContents
        {
            get => this.currentContents ??
                    (this.currentContents = new TaskDialogContents());

            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (this.DialogIsActive)
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


        /// <summary>
        /// 
        /// </summary>
        public bool DialogIsActive
        {
            get => this.hwndDialog != IntPtr.Zero;
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
        //public bool ResultVerificationCheckboxChecked
        //{
        //    get => this.resultVerificationCheckboxChecked;
        //}


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
                TaskDialogIcon icon = TaskDialogIcon.None)
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
                TaskDialogIcon icon = TaskDialogIcon.None)
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
                TaskDialogIcon icon = TaskDialogIcon.None)
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
                TaskDialogIcon icon = TaskDialogIcon.None)
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
                TaskDialogIcon icon = TaskDialogIcon.None)
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
                TaskDialogIcon icon = TaskDialogIcon.None)
        {
            var dialog = new TaskDialog()
            {
                currentContents = new TaskDialogContents()
                {
                    Content = content,
                    MainInstruction = instruction,
                    Title = title,
                    MainIcon = icon,
                    CommonButtons = buttons
                }
            };

            return ((TaskDialogCommonButton)dialog.Show(hwndOwner)).Result;
        }


        private static void FreeConfig(IntPtr ptrToFree)
        {
            Marshal.FreeHGlobal(ptrToFree);
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
                    instance.boundContents.ApplyInitialization();

                    instance.OnOpened(EventArgs.Empty);
                    instance.boundContents.OnCreated(EventArgs.Empty);
                    break;

                case TaskDialogNotification.Destroyed:
                    instance.boundContents.OnDestroying(EventArgs.Empty);
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
                    instance.boundContents.ApplyInitialization();

                    instance.OnNavigated(EventArgs.Empty);
                    instance.boundContents.OnCreated(EventArgs.Empty);
                    break;

                case TaskDialogNotification.HyperlinkClicked:
                    string link = Marshal.PtrToStringUni(lParam);

                    var eventArgs = new TaskDialogHyperlinkClickedEventArgs(link);
                    //instance.OnHyperlinkClicked(eventArgs);
                    instance.boundContents.OnHyperlinkClicked(eventArgs);
                    break;

                case TaskDialogNotification.ButtonClicked:
                    if (instance.suppressButtonClickedEvent)
                        return HResultOk;

                    int buttonID = wParam.ToInt32();

                    // Check if the button is part of the custom buttons.
                    var button = null as TaskDialogButton;
                    if (buttonID >= TaskDialogContents.CustomButtonStartID)
                    {
                        button = instance.boundContents.CustomButtons
                                [buttonID - TaskDialogContents.CustomButtonStartID];
                    }
                    else
                    {
                        var result = (TaskDialogResult)buttonID;
                        if (instance.boundContents.CommonButtons.Contains(result))
                            button = instance.boundContents.CommonButtons[result];
                    }

                    bool handlerResult = button?.HandleButtonClicked() ?? true;
                    if (handlerResult)
                    {
                        // Because we will return true (which means the dialog's result
                        // is going to be set to this button ID - except if this is the
                        // "Help" button, but this isn't a problem), we need to cache the
                        // last handled button instance and its original button ID,
                        // because if this was the last ButtonClicked notification, this
                        // will be the ID returned by TaskDialogIndirect.
                        // However, this can be a button that is no longer part of the
                        // TaskDialog when it already navigated, so we cannot simply
                        // search for the returned ID in the button collections.
                        // For example, when you navigate the dialog in a ButtonClicked
                        // event handler (so the dialog now displays new buttons) but
                        // then the click handler (that was called for the previous button)
                        // returns true, the dialog will close and the result will be the
                        // previous button, which however is no longer part of the
                        // collections as we already navigated. Therefore, we set this
                        // field so that when Show() returns, it can check if the
                        // button ID equals the last handled button, and use that
                        // instance in that case.
                        // Note: If the dialog had radio buttons at the time this handler
                        // was initially called, the dialog would also provide the last
                        // selected one even if it now has different/no radio buttons.
                        // However, because this would be complex to do correctly,
                        // we simply don't retrieve the resulting radio button ID,
                        // because the user can check which radio button button was
                        // selected by checking the "Checked" property.
                        instance.lastHandledResultButton = (button, buttonID);
                    }

                    return handlerResult ? HResultOk : HResultFalse;

                case TaskDialogNotification.RadioButtonClicked:
                    int radioButtonID = wParam.ToInt32();

                    var radioButton = instance.boundContents.RadioButtons
                            [radioButtonID - TaskDialogContents.RadioButtonStartID];

                    radioButton.HandleRadioButtonClicked();
                    break;

                case TaskDialogNotification.ExpandoButtonClicked:
                    instance.boundContents.Expander.HandleExpandoButtonClicked(
                            wParam != IntPtr.Zero);
                    break;

                case TaskDialogNotification.VerificationClicked:
                    instance.boundContents.VerificationCheckbox.HandleCheckboxClicked(
                            wParam != IntPtr.Zero);
                    break;

                case TaskDialogNotification.Help:
                    //instance.OnHelp(EventArgs.Empty);
                    instance.boundContents.OnHelp(EventArgs.Empty);
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
                    //instance.OnTimerTick(tickEventArgs);
                    instance.boundContents.OnTimerTick(tickEventArgs);

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
        /// Shows the dialog. After the dialog is created, the <see cref="Opened"/>
        /// event occurs which allows to customize the dialog. When the dialog is about to 
        /// close, the <see cref="Closing"/> event occurs.
        /// 
        /// Starting with the <see cref="Opened"/> event, you can call methods on the active 
        /// task dialog to update its state until the <see cref="Closing"/> event occurs.
        /// </summary>
        public TaskDialogButton Show()
        {
            return Show(IntPtr.Zero);
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
        public TaskDialogButton Show(System.Windows.Window owner)
        {
            return Show(GetWindowHandle(owner));
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
        public TaskDialogButton Show(System.Windows.Interop.IWin32Window owner)
        {
            return Show(GetWindowHandle(owner));
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
        public TaskDialogButton Show(System.Windows.Forms.IWin32Window owner)
        {
            return Show(GetWindowHandle(owner));
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
        public TaskDialogButton Show(TaskDialog owner)
        {
            return Show(owner.Handle);
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
        public TaskDialogButton Show(IntPtr hwndOwner)
        {
            // Recursive Show() is not possible because we would incorrectly handle notifications.
            if (this.instanceHandlePtr != IntPtr.Zero)
                throw new InvalidOperationException(
                        "Cannot recursively show the same task dialog instance.");

            // Validate the config.
            this.CurrentContents.Validate(this);

            // Allocate a GCHandle which we will use for the callback data.
            var instanceHandle = GCHandle.Alloc(this);
            try
            {
                this.instanceHandlePtr = GCHandle.ToIntPtr(instanceHandle);
                this.currentOwnerHwnd = hwndOwner;

                // Clear the previous result properties.
                //this.resultVerificationCheckboxChecked = default;
                //this.resultCommonButton = default;
                //this.resultCustomButton = null;
                //this.resultRadioButton = null;

                // Bind the contents and allocate the memory.
                BindAndAllocateConfig(
                       out var ptrToFree,
                       out var ptrTaskDialogConfig);
                try
                {
                    int ret = NativeMethods.TaskDialogIndirect(
                                ptrTaskDialogConfig,
                                out int resultButtonID,
                                out int resultRadioButtonID,
                                out bool resultVerificationCheckboxChecked);

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

                    // Check if the resulting button ID is the same as the last handled button.
                    // This should always be the case, except when the button ID refers to a
                    // TaskDialogResult for which the user didn't create a button.
                    TaskDialogButton resultingButton;
                    if (this.lastHandledResultButton.button != null &&
                            this.lastHandledResultButton.originalButtonID == resultButtonID)
                    {
                        resultingButton = this.lastHandledResultButton.button;
                    }
                    else if (resultButtonID >= TaskDialogContents.CustomButtonStartID)
                    {
                        // This should normally not happen as we already cached the last
                        // handled button.
                        resultingButton = this.boundContents.CustomButtons
                                [resultButtonID - TaskDialogContents.CustomButtonStartID];
                    }
                    else
                    {
                        var result = (TaskDialogResult)resultButtonID;

                        // Check we have a button with the result that was returned. This might
                        // not always be the case, e.g. when specifying AllowCancel but not
                        // adding a "Cancel" button. If we don't have such button, we
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
                    // Clear the handles and free the memory.
                    this.currentOwnerHwnd = null;
                    FreeConfig(ptrToFree);

                    // Unbind the contents. The 'Destroying' event of the TaskDialogContent
                    // will already have been called from the callback.
                    this.boundContents.Unbind();
                    this.boundContents = null;
                    this.lastHandledResultButton.button = null;

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
        public void Close()
        {
            //if (suppressCommonButtonClickedEvent)
            this.suppressButtonClickedEvent = true;
            try
            {
                // Send a click button message with the cancel result.
                ClickButton((int)TaskDialogResult.Cancel);
            }
            finally
            {
                //if (suppressCommonButtonClickedEvent)
                this.suppressButtonClickedEvent = false;
            }
        }


        /// <summary>
        /// While the dialog is active, switches the progress bar mode to either a
        /// marquee progress bar or to a regular progress bar.
        /// For a marquee progress bar, you can enable or disable the marquee using
        /// <see cref="SetProgressBarMarquee(bool, int)"/>.
        /// </summary>
        /// <param name="marqueeProgressBar"></param>
        internal void SwitchProgressBarMode(bool marqueeProgressBar)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.SetMarqueeProgressBar,
                    marqueeProgressBar ? 1 : 0,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is active, enables or disables progress bar marquee when
        /// an marquee progress bar is displayed.
        /// </summary>
        /// <param name="enableMarquee"></param>
        /// <param name="animationSpeed">
        /// The time in milliseconds between marquee animation updates. If <c>0</c>, the animation
        /// will be updated every 30 milliseconds.
        /// </param>
        internal void SetProgressBarMarquee(bool enableMarquee, int animationSpeed = 0)
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
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        internal void SetProgressBarRange(int min, int max)
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
        /// </summary>
        /// <param name="pos"></param>
        internal void SetProgressBarPos(int pos)
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
        /// </summary>
        /// <param name="state"></param>
        internal void SetProgressBarState(TaskDialogProgressBarNativeState state)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.SetProgressBarState,
                    (int)state,
                    IntPtr.Zero);
        }

        /// <summary>
        /// While the dialog is active, sets the verification checkbox to the specified
        /// state.
        /// </summary>
        /// <param name="isChecked"></param>
        /// <param name="focus"></param>
        internal void ClickVerification(bool isChecked, bool focus = false)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.ClickVerification,
                    isChecked ? 1 : 0,
                    (IntPtr)(focus ? 1 : 0));
        }

        internal void SetButtonElevationRequiredState(int buttonID, bool requiresElevation)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.SetButtonElevationRequiredState,
                    buttonID,
                    (IntPtr)(requiresElevation ? 1 : 0));
        }

        internal void SetButtonEnabled(int buttonID, bool enable)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.EnableButton,
                    buttonID,
                    (IntPtr)(enable ? 1 : 0));
        }

        internal void SetRadioButtonEnabled(int radioButtonID, bool enable)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.EnableRadioButton,
                    radioButtonID,
                    (IntPtr)(enable ? 1 : 0));
        }

        internal void ClickButton(int buttonID)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.ClickButton,
                    buttonID,
                    IntPtr.Zero);
        }

        internal void ClickRadioButton(int radioButtonID)
        {
            SendTaskDialogMessage(
                    TaskDialogMessage.ClickRadioButton,
                    radioButtonID,
                    IntPtr.Zero);
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


        /// <summary>
        /// While the dialog is active, recreates the dialog from the current properties.
        /// </summary>
        /// <remarks>
        /// Note that you should not call this method in the <see cref="Opened"/> event
        /// because the task dialog is not yet displayed in that state.
        /// </remarks>
        private void Navigate(TaskDialogContents contents)
        {
            Debug.Assert(this.DialogIsActive);

            // Validate the config.
            contents.Validate(this);

            // After validation passed, we can now unbind the current contents and
            // bind the new one.
            // Need to raise the "Destroying" event for the current contents. The
            // "Created" event for the new contents will occur from the callback.
            this.boundContents.OnDestroying(EventArgs.Empty);
            this.boundContents.Unbind();
            this.boundContents = null;

            this.currentContents = contents;
            BindAndAllocateConfig(
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

        private unsafe void BindAndAllocateConfig(
                out IntPtr ptrToFree,
                out IntPtr ptrTaskDialogConfig)
        {
            var contents = this.CurrentContents;
            contents.Bind(
                    this,
                    out var flags,
                    out var commonButtonFlags,
                    out int defaultButtonID,
                    out int defaultRadioButtonID);
            this.boundContents = contents;

            checked
            {
                // First, calculate the necessary memory size we need to allocate for all
                // structs and strings.
                // Note: Each Align() call when calculating the size must correspond with a
                // Align() call when incrementing the pointer.
                // Use a byte pointer so we can use byte-wise pointer arithmetics.
                var sizeToAllocate = (byte*)0;
                sizeToAllocate += sizeof(TaskDialogConfig);
                Align(ref sizeToAllocate);

                // Strings in TasDialogConfig
                sizeToAllocate += SizeOfString(contents.Title);
                sizeToAllocate += SizeOfString(contents.MainInstruction);
                sizeToAllocate += SizeOfString(contents.Content);
                sizeToAllocate += SizeOfString(contents.Footer);
                sizeToAllocate += SizeOfString(contents.Expander?.Text);
                sizeToAllocate += SizeOfString(contents.Expander?.ExpandedButtonText);
                sizeToAllocate += SizeOfString(contents.Expander?.CollapsedButtonText);
                sizeToAllocate += SizeOfString(contents.VerificationCheckbox?.Text);
                Align(ref sizeToAllocate);

                // Buttons array
                if (contents.CustomButtons.Count > 0)
                {
                    sizeToAllocate += sizeof(TaskDialogButtonStruct) * contents.CustomButtons.Count;
                    Align(ref sizeToAllocate);
                    // Strings in buttons array
                    for (int i = 0; i < contents.CustomButtons.Count; i++)
                        sizeToAllocate += SizeOfString(contents.CustomButtons[i].GetResultingText());
                    Align(ref sizeToAllocate);
                }

                // Radio buttons array
                if (contents.RadioButtons.Count > 0)
                {
                    sizeToAllocate += sizeof(TaskDialogButtonStruct) * contents.RadioButtons.Count;
                    Align(ref sizeToAllocate);
                    // Strings in radio buttons array
                    for (int i = 0; i < contents.RadioButtons.Count; i++)
                        sizeToAllocate += SizeOfString(contents.RadioButtons[i].Text);
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
                        dwCommonButtons = commonButtonFlags,
                        hMainIcon = contents.BoundMainIconIsFromHandle ?
                                contents.MainIconHandle : (IntPtr)contents.MainIcon,
                        hFooterIcon = contents.BoundFooterIconIsFromHandle ?
                                contents.FooterIconHandle : (IntPtr)contents.FooterIcon,
                        pszWindowTitle = MarshalString(contents.Title),
                        pszMainInstruction = MarshalString(contents.MainInstruction),
                        pszContent = MarshalString(contents.Content),
                        pszFooter = MarshalString(contents.Footer),
                        pszExpandedInformation = MarshalString(contents.Expander?.Text),
                        pszExpandedControlText = MarshalString(contents.Expander?.ExpandedButtonText),
                        pszCollapsedControlText = MarshalString(contents.Expander?.CollapsedButtonText),
                        pszVerificationText = MarshalString(contents.VerificationCheckbox?.Text),
                        nDefaultButton = defaultButtonID,
                        nDefaultRadioButton = defaultRadioButtonID,
                        pfCallback = callbackProcDelegatePtr,
                        lpCallbackData = this.instanceHandlePtr,
                        cxWidth = contents.Width
                    };
                    Align(ref currentPtr);

                    // Buttons array
                    if (contents.CustomButtons.Count > 0)
                    {
                        var customButtonStructs = (TaskDialogButtonStruct*)currentPtr;
                        taskDialogConfig.pButtons = (IntPtr)customButtonStructs;
                        taskDialogConfig.cButtons = contents.CustomButtons.Count;
                        currentPtr += sizeof(TaskDialogButtonStruct) * contents.CustomButtons.Count;
                        Align(ref currentPtr);

                        for (int i = 0; i < contents.CustomButtons.Count; i++)
                        {
                            var currentCustomButton = contents.CustomButtons[i];
                            customButtonStructs[i] = new TaskDialogButtonStruct()
                            {
                                nButtonID = currentCustomButton.ButtonID,
                                pszButtonText = MarshalString(currentCustomButton.GetResultingText())
                            };
                        }
                        Align(ref currentPtr);
                    }

                    // Radio buttons array
                    if (contents.RadioButtons.Count > 0)
                    {
                        var customRadioButtonStructs = (TaskDialogButtonStruct*)currentPtr;
                        taskDialogConfig.pRadioButtons = (IntPtr)customRadioButtonStructs;
                        taskDialogConfig.cRadioButtons = contents.RadioButtons.Count;
                        currentPtr += sizeof(TaskDialogButtonStruct) * contents.RadioButtons.Count;
                        Align(ref currentPtr);

                        for (int i = 0; i < contents.RadioButtons.Count; i++)
                        {
                            var currentCustomButton = contents.RadioButtons[i];
                            customRadioButtonStructs[i] = new TaskDialogButtonStruct()
                            {
                                nButtonID = currentCustomButton.RadioButtonID,
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

        private void DenyIfDialogNotActive()
        {
            if (!this.DialogIsActive)
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

        internal void UpdateTextElement(
                TaskDialogTextElement element,
                string text)
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

        internal void UpdateIconElement(
                TaskDialogIconElement element,
                IntPtr icon)
        {
            SendTaskDialogMessage(TaskDialogMessage.UpdateIcon, (int)element, icon);
        }
    }
}
