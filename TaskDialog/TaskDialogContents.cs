using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TaskDialogContents
    {
        /// <summary>
        /// The start ID for custom buttons. We need to ensure we don't use a ID that
        /// is already used for a common button (TaskDialogResult), so we start with
        /// 100 to be safe.
        /// </summary>
        internal const int CustomButtonStartID = 100;

        /// <summary>
        /// The start ID for radio buttons. This must be at least 1 because 0 already
        /// stands for "no button".
        /// </summary>
        internal const int RadioButtonStartID = 1;


        private TaskDialogCommonButtonCollection commonButtons;

        private TaskDialogCustomButtonCollection customButtons;

        private TaskDialogRadioButtonCollection radioButtons;

        private TaskDialogExpander expander;

        private TaskDialogProgressBar progressBar;

        private TaskDialogCheckBox checkBox;

        private TaskDialog boundTaskDialog;

        private TaskDialogFlags flags;
        private string title;
        private string instruction;
        private string text;
        private string footerText;
        private TaskDialogIcon icon;
        private IntPtr iconHandle;
        private TaskDialogIcon footerIcon;
        private IntPtr footerIconHandle;
        private int width;
        private TaskDialogCommandLinkMode commandLinkMode;
        private TaskDialogStartupLocation startupLocation;

        private bool boundMainIconIsFromHandle;

        private bool boundFooterIconIsFromHandle;


        /// <summary>
        /// Occurs after this instance is bound to a task dialog and the task dialog
        /// has created the GUI elements represented by this
        /// <see cref="TaskDialogContents"/> instance.
        /// </summary>
        /// <remarks>
        /// This will happen after showing or navigating the dialog.
        /// </remarks>
        public event EventHandler Created;

        /// <summary>
        /// Occurs when the task dialog is about to destroy the GUI elements represented
        /// by this <see cref="TaskDialogContents"/> instance and it is about to be
        /// unbound from the task dialog.
        /// </summary>
        /// <remarks>
        /// This will happen before closing or navigating the dialog.
        /// </remarks>
        public event EventHandler Destroying;

        /// <summary>
        /// Occurs when the user presses F1 while the task dialog has focus, or when the
        /// user clicks the <see cref="TaskDialogButtons.Help"/> button.
        /// </summary>
        public event EventHandler Help;

        /// <summary>
        /// 
        /// </summary>
        public event EventHandler<TaskDialogHyperlinkClickedEventArgs> HyperlinkClicked;

        /// <summary>
        /// Occurs approximately every 200 milliseconds while this
        /// <see cref="TaskDialogContents"/> is bound.
        /// </summary>
        public event EventHandler<TaskDialogTimerTickEventArgs> TimerTick;


        /// <summary>
        /// 
        /// </summary>
        public TaskDialogContents()
        {
            // Set default flags/properties.
            this.startupLocation = TaskDialogStartupLocation.CenterParent;

            // Create empty (hidden) controls.
            this.expander = new TaskDialogExpander();
            this.checkBox = new TaskDialogCheckBox();
            this.progressBar = new TaskDialogProgressBar(TaskDialogProgressBarState.None);
        }


        /// <summary>
        /// 
        /// </summary>
        [Category("Controls")]
        public TaskDialogCommonButtonCollection CommonButtons
        {
            get => this.commonButtons ??
                    (this.commonButtons = new TaskDialogCommonButtonCollection());

            set
            {
                this.commonButtons = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Controls")]
        public TaskDialogCustomButtonCollection CustomButtons
        {
            get => this.customButtons ??
                    (this.customButtons = new TaskDialogCustomButtonCollection());

            set
            {
                this.customButtons = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Controls")]
        public TaskDialogRadioButtonCollection RadioButtons
        {
            get => this.radioButtons ??
                    (this.radioButtons = new TaskDialogRadioButtonCollection());
            
            set
            {
                this.radioButtons = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Controls")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TaskDialogCheckBox CheckBox
        {
            get => this.checkBox;

            set
            {
                // We must deny this if we are bound because we need to be able to
                // access the control from the task dialog's callback.
                this.DenyIfBound();

                this.checkBox = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Controls")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TaskDialogExpander Expander
        {
            get => this.expander;

            set
            {
                // We must deny this if we are bound because we need to be able to
                // access the control from the task dialog's callback.
                this.DenyIfBound();

                this.expander = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Category("Controls")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public TaskDialogProgressBar ProgressBar
        {
            get => this.progressBar;

            set
            {
                // We must deny this if we are bound because we need to be able to
                // access the control from the task dialog's callback.
                this.DenyIfBound();

                this.progressBar = value;
            }
        }

        /// <summary>
        /// Gets or sets the title of the task dialog window.
        /// </summary>
        public string Title
        {
            get => this.title;

            set
            {
                // Note: We set the field values after calling the method to ensure
                // it still has the previous value it the method throws.
                this.boundTaskDialog?.UpdateTitle(value);
                this.title = value;
            }
        }

        /// <summary>
        /// Gets or sets the main instruction text.
        /// </summary>
        /// <remarks>
        /// This text can be changed while the dialog is shown.
        /// </remarks>
        public string Instruction
        {
            get => this.instruction;

            set
            {
                this.boundTaskDialog?.UpdateTextElement(
                        TaskDialogTextElement.MainInstruction,
                        value);
                this.instruction = value;
            }
        }

        /// <summary>
        /// Gets or sets the dialog's primary text content.
        /// </summary>
        /// <remarks>
        /// This text can be changed while the dialog is shown.
        /// </remarks>
        public string Text
        {
            get => this.text;

            set
            {
                this.boundTaskDialog?.UpdateTextElement(
                        TaskDialogTextElement.Content,
                        value);
                this.text = value;
            }
        }

        /// <summary>
        /// Gets or sets the text to be displayed in the dialog's footer area.
        /// </summary>
        /// <remarks>
        /// This text can be changed while the dialog is shown.
        /// </remarks>
        public string FooterText
        {
            get => this.footerText;

            set
            {
                this.boundTaskDialog?.UpdateTextElement(
                        TaskDialogTextElement.Footer,
                        value);
                this.footerText = value;
            }
        }

        /// <summary>
        /// Gets or sets the main icon, if <see cref="IconHandle"/> is
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <remarks>
        /// This icon can be changed while the dialog is shown.
        /// </remarks>
        [DefaultValue(TaskDialogIcon.None)]
        public TaskDialogIcon Icon
        {
            get => this.icon;

            set
            {
                if (this.boundTaskDialog != null &&
                        this.boundMainIconIsFromHandle)
                    throw new InvalidOperationException();

                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Main,
                        (IntPtr)value);
                this.icon = value;
            }
        }

        /// <summary>
        /// Gets or sets the handle to the main icon. When this member is not
        /// <see cref="IntPtr.Zero"/>, the <see cref="Icon"/> property will
        /// be ignored.
        /// </summary>
        /// <remarks>
        /// This icon can be changed while the dialog is shown.
        /// </remarks>
        [Browsable(false)]
        public IntPtr IconHandle
        {
            get => this.iconHandle;

            set
            {
                if (this.boundTaskDialog != null &&
                        !this.boundMainIconIsFromHandle)
                    throw new InvalidOperationException();

                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Main,
                        value);
                this.iconHandle = value;
            }
        }

        /// <summary>
        /// Gets or sets the footer icon, if <see cref="FooterIconHandle"/> is
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <remarks>
        /// This icon can be changed while the dialog is shown.
        /// </remarks>
        [DefaultValue(TaskDialogIcon.None)]
        public TaskDialogIcon FooterIcon
        {
            get => this.footerIcon;

            set
            {
                if (this.boundTaskDialog != null &&
                        this.boundFooterIconIsFromHandle == true)
                    throw new InvalidOperationException();

                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Footer,
                        (IntPtr)value);
                this.footerIcon = value;
            }
        }

        /// <summary>
        /// Gets or sets the handle to the footer icon. When this member is not
        /// <see cref="IntPtr.Zero"/>, the <see cref="FooterIcon"/> property will
        /// be ignored.
        /// </summary>
        /// <remarks>
        /// This icon can be changed while the dialog is shown.
        /// </remarks>
        [Browsable(false)]
        public IntPtr FooterIconHandle
        {
            get => this.footerIconHandle;

            set
            {
                if (this.boundTaskDialog != null &&
                        !this.boundFooterIconIsFromHandle == false)
                    throw new InvalidOperationException();

                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Footer,
                        value);
                this.footerIconHandle = value;
            }
        }

        /// <summary>
        /// Gets or sets the width in dialog units that the dialog's client area will get
        /// when the dialog is is created or navigated.
        /// If <c>0</c>, the width will be automatically calculated by the system.
        /// </summary>
        [DefaultValue(0)]
        public int Width
        {
            get => this.width;

            set
            {
                this.DenyIfBound();

                this.width = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="TaskDialogCommandLinkMode"/> that specifies how to
        /// display custom buttons.
        /// </summary>
        [DefaultValue(TaskDialogCommandLinkMode.None)]
        public TaskDialogCommandLinkMode CommandLinkMode
        {
            get => this.commandLinkMode;

            set
            {
                this.DenyIfBound();

                this.commandLinkMode = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(TaskDialogStartupLocation.CenterParent)]
        public TaskDialogStartupLocation StartupLocation
        {
            get => this.startupLocation;

            set
            {
                this.DenyIfBound();

                this.startupLocation = value;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(false)]
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
        /// <remarks>
        /// You can intercept cancellation of the dialog without displaying a "Cancel"
        /// button by adding a <see cref="TaskDialogCommonButton"/> with its
        /// <see cref="TaskDialogCommonButton.Visible"/> set to <c>false</c> and specifying
        /// a <see cref="TaskDialogResult.Cancel"/> result.
        /// </remarks>
        [DefaultValue(false)]
        public bool AllowCancel
        {
            get => GetFlag(TaskDialogFlags.AllowDialogCancellation);
            set => SetFlag(TaskDialogFlags.AllowDialogCancellation, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note that once a task dialog has been opened with or has navigated to a
        /// <see cref="TaskDialogContents"/> where this flag is set, it will keep on
        /// subsequent navigations to a new <see cref="TaskDialogContents"/> even when
        /// it doesn't have this flag set.
        /// </remarks>
        [DefaultValue(false)]
        public bool RightToLeftLayout
        {
            get => GetFlag(TaskDialogFlags.RtlLayout);
            set => SetFlag(TaskDialogFlags.RtlLayout, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the task dialog can be minimized
        /// when it is shown modeless.
        /// </summary>
        /// <remarks>
        /// When setting this property to <c>true</c>, <see cref="AllowCancel"/> is
        /// automatically implied.
        /// </remarks>
        [DefaultValue(false)]
        public bool CanBeMinimized
        {
            get => GetFlag(TaskDialogFlags.CanBeMinimized);
            set => SetFlag(TaskDialogFlags.CanBeMinimized, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates if the task dialog should not set
        /// itself as foreground window when showing it.
        /// </summary>
        /// <remarks>
        /// When setting this property to <c>true</c> and then showing the dialog, it
        /// causes the dialog to net set itself as foreground window if the current
        /// foreground window does not belong to the application.
        /// 
        /// Note: This property does not have an effect when navigating the task dialog.
        /// Note: This property only has an effect on Windows 8 and higher.
        /// </remarks>
        [DefaultValue(false)]
        public bool DoNotSetForeground
        {
            get => GetFlag(TaskDialogFlags.NoSetForeground);
            set => SetFlag(TaskDialogFlags.NoSetForeground, value);
        }

        /// <summary>
        /// 
        /// </summary>
        [DefaultValue(false)]
        public bool SizeToContent
        {
            get => GetFlag(TaskDialogFlags.SizeToContent);
            set => SetFlag(TaskDialogFlags.SizeToContent, value);
        }


        internal TaskDialog BoundTaskDialog
        {
            get => this.boundTaskDialog;
        }

        internal bool BoundMainIconIsFromHandle
        {
            get => this.boundMainIconIsFromHandle;
        }

        internal bool BoundFooterIconIsFromHandle
        {
            get => this.boundFooterIconIsFromHandle;
        }


        internal static bool IsNativeStringNullOrEmpty(string str)
        {
            // From a native point of view, the string is empty if its first
            // character is a NUL char.
            return string.IsNullOrEmpty(str) || str[0] == '\0';
        }


        internal void DenyIfBound()
        {
            if (this.boundTaskDialog != null)
                throw new InvalidOperationException(
                        "Cannot set this property or call this method while the " +
                        "contents are bound to a task dialog.");
        }

        internal void Validate(TaskDialog newOwner)
        {
            //// Before assigning button IDs etc., check if the button configs are OK.
            //// This needs to be done before clearing the old button IDs and assigning
            //// the new ones, because it is possible to use the same button
            //// instances after a dialog has been created for Navigate(), where need to
            //// do the check, then release the old buttons, then assign the new
            //// buttons.

            // Check that this contents instance is not already bound to another
            // TaskDialog instance. We don't throw if it is already bound to the
            // same TaskDialog instane that wants to bind now, because that should
            // be OK.
            if (this.boundTaskDialog != null && this.boundTaskDialog != newOwner)
                throw new InvalidOperationException(
                        $"This {nameof(TaskDialogContents)} instance is already bound to " +
                        $"another {nameof(TaskDialog)} instance.");

            // We also need to validate the controls since they could also be assinged to
            // another (bound) TaskDialogContents at the same time.
            // Access the collections using the property to ensure they exist.
            if (this.CommonButtons.BoundTaskDialogContents != null && this.CommonButtons.BoundTaskDialogContents != this ||
                    this.CustomButtons.BoundTaskDialogContents != null && this.CustomButtons.BoundTaskDialogContents != this ||
                    this.RadioButtons.BoundTaskDialogContents != null && this.RadioButtons.BoundTaskDialogContents != this ||
                    this.expander?.BoundTaskDialogContents != null && this.expander.BoundTaskDialogContents != this ||
                    this.progressBar?.BoundTaskDialogContents != null && this.progressBar.BoundTaskDialogContents != this ||
                    this.checkBox?.BoundTaskDialogContents != null && this.checkBox.BoundTaskDialogContents != this)
                throw new InvalidOperationException();
            foreach (var control in (this.CommonButtons as IEnumerable<TaskDialogControl>)
                    .Concat(this.CustomButtons)
                    .Concat(this.RadioButtons))
                if (control.BoundTaskDialogContents != null && control.BoundTaskDialogContents != this)
                    throw new InvalidOperationException();

            if (this.CustomButtons.Count > int.MaxValue - CustomButtonStartID + 1 ||
                    this.RadioButtons.Count > int.MaxValue - RadioButtonStartID + 1)
                throw new InvalidOperationException(
                        "Too many custom buttons or radio buttons have been added.");

            //// Note: This is no longer needed, because we allow non-createable
            //// controls to be added, and the control will check the state by
            //// itself.
            //// Ensure that if we have a checkbox, its text is not null/empty.
            //// Otherwise we will get AccessViolationExceptions when sending a Click message.
            //if (this.checkBox != null &&
            //        IsNativeStringNullOrEmpty(this.checkBox.Text))
            //    throw new InvalidOperationException(
            //        $"When a {nameof(this.CheckBox)} is set, its " +
            //        $"{nameof(this.CheckBox.Text)} must not be null or empty.");

            bool foundDefaultButton = false;
            foreach (var button in (this.CommonButtons as IEnumerable<TaskDialogButton>)
                    .Concat(this.CustomButtons))
            {
                if (button.DefaultButton)
                {
                    if (!foundDefaultButton)
                        foundDefaultButton = true;
                    else
                        throw new InvalidOperationException("Only one button can be set as default button.");
                }
            }

            // For custom and radio buttons, we need to ensure the strings are
            // not null or empty, as otherwise an error would occur when
            // showing/navigating the dialog.
            foreach (var button in this.customButtons)
            {
                if (IsNativeStringNullOrEmpty(button.Text))
                    throw new InvalidOperationException("The text of a custom button must not be null or empty.");
            }

            bool foundCheckedRadioButton = false;
            foreach (var button in this.radioButtons)
            {
                if (IsNativeStringNullOrEmpty(button.Text))
                    throw new InvalidOperationException("The text of a radio button must not be null or empty.");

                if (button.Checked)
                {
                    if (!foundCheckedRadioButton)
                        foundCheckedRadioButton = true;
                    else
                        throw new InvalidOperationException("Only one radio button can be set as checked.");
                }
            }
        }

        internal void Bind(
                TaskDialog owner,
                out TaskDialogFlags flags,
                out TaskDialogButtons buttonFlags,
                out int defaultButtonID,
                out int defaultRadioButtonID)
        {
            //// This method assumes Validate() has already been called.

            this.boundTaskDialog = owner;
            flags = this.flags;
            buttonFlags = GetCommonButtonFlags();

            this.boundMainIconIsFromHandle = this.IconHandle != IntPtr.Zero;
            this.boundFooterIconIsFromHandle = this.FooterIconHandle != IntPtr.Zero;
            if (this.boundMainIconIsFromHandle)
                flags |= TaskDialogFlags.UseHIconMain;
            if (this.boundFooterIconIsFromHandle)
                flags |= TaskDialogFlags.UseHIconFooter;

            if (this.startupLocation == TaskDialogStartupLocation.CenterParent)
                flags |= TaskDialogFlags.PositionRelativeToWindow;

            // Specify the timer flag if an event handler has been added to the timer
            // tick event.
            if (this.TimerTick != null)
                flags |= TaskDialogFlags.CallbackTimer;

            // Only specify the command link flags if there actually are custom buttons;
            // otherwise the dialog will not work.
            if (this.customButtons.Count > 0)
            {
                if (this.commandLinkMode == TaskDialogCommandLinkMode.CommandLinks)
                    flags |= TaskDialogFlags.UseCommandLinks;
                else if (this.commandLinkMode == TaskDialogCommandLinkMode.CommandLinksNoIcon)
                    flags |= TaskDialogFlags.UseCommandLinksNoIcon;
            }

            var commonButtons = this.CommonButtons;
            var customButtons = this.CustomButtons;
            var radioButtons = this.RadioButtons;

            commonButtons.BoundTaskDialogContents = this;
            customButtons.BoundTaskDialogContents = this;
            radioButtons.BoundTaskDialogContents = this;

            // Assign IDs to the buttons based on their index.
            // Note: The collections will be locked while this contents are bound, so we
            // don't need to copy them here.
            defaultButtonID = 0;
            for (int i = 0; i < commonButtons.Count; i++)
            {
                var commonButton = commonButtons[i];
                commonButton.BoundTaskDialogContents = this;
                flags |= commonButton.GetFlags();

                if (commonButton.DefaultButton && defaultButtonID == 0)
                    defaultButtonID = (int)commonButton.Result;
            }

            for (int i = 0; i < customButtons.Count; i++)
            {
                var customButton = customButtons[i];
                customButton.BoundTaskDialogContents = this;
                flags |= customButton.GetFlags();

                customButton.ButtonID = CustomButtonStartID + i;                
                if (customButton.DefaultButton && defaultButtonID == 0)
                    defaultButtonID = customButton.ButtonID;
            }

            defaultRadioButtonID = 0;
            for (int i = 0; i < radioButtons.Count; i++)
            {
                var radioButton = radioButtons[i];
                radioButton.BoundTaskDialogContents = this;
                flags |= radioButton.GetFlags();

                radioButton.RadioButtonID = RadioButtonStartID + i;
                if (radioButton.Checked && defaultRadioButtonID == 0)
                    defaultRadioButtonID = radioButton.RadioButtonID;
                else if (radioButton.Checked)
                    radioButton.Checked = false;
            }

            if (defaultRadioButtonID == 0)
                flags |= TaskDialogFlags.NoDefaultRadioButton;

            if (this.expander != null)
            {
                this.expander.BoundTaskDialogContents = this;
                flags |= this.expander.GetFlags();
            }

            if (this.progressBar != null)
            {
                this.progressBar.BoundTaskDialogContents = this;
                flags |= this.progressBar.GetFlags();
            }

            if (this.checkBox != null)
            {
                this.checkBox.BoundTaskDialogContents = this;
                flags |= this.checkBox.GetFlags();
            }
        }

        internal void Unbind()
        {
            var commonButtons = this.CommonButtons;
            var customButtons = this.CustomButtons;
            var radioButtons = this.RadioButtons;

            for (int i = 0; i < commonButtons.Count; i++)
            {
                var commonButton = commonButtons[i];
                commonButton.BoundTaskDialogContents = null;
            }

            for (int i = 0; i < customButtons.Count; i++)
            {
                var customButton = customButtons[i];
                customButton.BoundTaskDialogContents = null;
                customButton.ButtonID = 0;
                customButton.Handle = IntPtr.Zero;
            }

            for (int i = 0; i < radioButtons.Count; i++)
            {
                var radioButton = radioButtons[i];
                radioButton.BoundTaskDialogContents = null;
                radioButton.RadioButtonID = 0;
                radioButton.Handle = IntPtr.Zero;
            }

            commonButtons.BoundTaskDialogContents = null;
            customButtons.BoundTaskDialogContents = null;
            radioButtons.BoundTaskDialogContents = null;

            if (this.expander != null)
                this.expander.BoundTaskDialogContents = null;
            if (this.progressBar != null)
                this.progressBar.BoundTaskDialogContents = null;
            if (this.checkBox != null)
                this.checkBox.BoundTaskDialogContents = null;

            this.boundTaskDialog = null;
        }

        internal void ApplyInitialization()
        {
            //// Try to find and assign the control handlers for custom and radio
            //// buttons after the dialog was shown or navigated.
            //// TODO: This is not yet enabled.
            //AssignControlHandles();

            foreach (var button in this.CommonButtons)
                button.ApplyInitialization();

            foreach (var button in this.CustomButtons)
                button.ApplyInitialization();

            foreach (var button in this.RadioButtons)
                button.ApplyInitialization();

            this.expander?.ApplyInitialization();
            this.progressBar?.ApplyInitialization();
            this.checkBox?.ApplyInitialization();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        internal protected void OnCreated(EventArgs e)
        {
            this.Created?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        internal protected void OnDestroying(EventArgs e)
        {
            this.Destroying?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        internal protected void OnHelp(EventArgs e)
        {
            this.Help?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        internal protected void OnHyperlinkClicked(TaskDialogHyperlinkClickedEventArgs e)
        {
            this.HyperlinkClicked?.Invoke(this, e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e"></param>
        internal protected void OnTimerTick(TaskDialogTimerTickEventArgs e)
        {
            this.TimerTick?.Invoke(this, e);
        }


        private unsafe void AssignControlHandles()
        {
            const string buttonClassName = "Button";

            // Get all button handles by enumerating the child window handles and
            // then checking if their class equals "Button".
            var buttonHandles = new List<IntPtr>();

            // Add one extra char for the \0 char and another one to check that the
            // string is actually not longer than "Button".
            int classNameBufferLength = buttonClassName.Length + 2;
            char* classNameBuffer = stackalloc char[classNameBufferLength];
            EnumerateWindowHelper.EnumerateChildWindows(
                    this.boundTaskDialog.Handle,
                    hWndChild =>
                    {
                        int result = TaskDialog.NativeMethods.GetClassName(
                                hWndChild,
                                (IntPtr)classNameBuffer,
                                classNameBufferLength);
                        // TODO: Use Span<char> to avoid string allocations
                        if (result > 0 &&
                                result == buttonClassName.Length &&
                                new string(classNameBuffer, 0, result) == buttonClassName)
                            buttonHandles.Add(hWndChild);

                        return true;
                    });

            // Assign the button handles for custom and radio buttons.
            if (buttonHandles.Count >=
                    this.RadioButtons.Count + this.CustomButtons.Count)
            {
                bool hasCommandLinks =
                        this.CommandLinkMode == TaskDialogCommandLinkMode.CommandLinks ||
                        this.CommandLinkMode == TaskDialogCommandLinkMode.CommandLinksNoIcon;

                var customButtons = this.CustomButtons;
                var radioButtons = this.RadioButtons;

                int customButtonOffset = hasCommandLinks ? 0 : radioButtons.Count;
                int radioButtonOffset = hasCommandLinks ? customButtons.Count : 0;

                for (int i = 0; i < customButtons.Count; i++)
                    customButtons[i].Handle = buttonHandles[customButtonOffset + i];
                for (int i = 0; i < radioButtons.Count; i++)
                    radioButtons[i].Handle = buttonHandles[radioButtonOffset + i];
            }
        }

        private TaskDialogButtons GetCommonButtonFlags()
        {
            var flags = default(TaskDialogButtons);

            foreach (var button in this.CommonButtons)
            {
                // Don't include hidden buttons.
                if (!button.Visible)
                    continue;

                switch (button.Result)
                {
                    case TaskDialogResult.OK:
                        flags |= TaskDialogButtons.OK;
                        break;
                    case TaskDialogResult.Cancel:
                        flags |= TaskDialogButtons.Cancel;
                        break;
                    case TaskDialogResult.Abort:
                        flags |= TaskDialogButtons.Abort;
                        break;
                    case TaskDialogResult.Retry:
                        flags |= TaskDialogButtons.Retry;
                        break;
                    case TaskDialogResult.Ignore:
                        flags |= TaskDialogButtons.Ignore;
                        break;
                    case TaskDialogResult.Yes:
                        flags |= TaskDialogButtons.Yes;
                        break;
                    case TaskDialogResult.No:
                        flags |= TaskDialogButtons.No;
                        break;
                    case TaskDialogResult.Close:
                        flags |= TaskDialogButtons.Close;
                        break;
                    case TaskDialogResult.Help:
                        flags |= TaskDialogButtons.Help;
                        break;
                    case TaskDialogResult.TryAgain:
                        flags |= TaskDialogButtons.TryAgain;
                        break;
                    case TaskDialogResult.Continue:
                        flags |= TaskDialogButtons.Continue;
                        break;
                    default:
                        throw new InvalidOperationException(); // should not happen
                }
            }

            return flags;
        }

        private bool GetFlag(TaskDialogFlags flag)
        {
            return (this.flags & flag) == flag;
        }

        private void SetFlag(TaskDialogFlags flag, bool value)
        {
            DenyIfBound();

            if (value)
                this.flags |= flag;
            else
                this.flags &= ~flag;
        }
    }
}
