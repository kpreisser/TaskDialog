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
        /// The start ID for custom buttons.
        /// </summary>
        /// <remarks>
        /// We need to ensure we don't use a ID that is already used for a
        /// common button (TaskDialogResult), so we start with 100 to be safe
        /// (100 is also used as first ID in MSDN examples for the task dialog).
        /// </remarks>
        internal const int CustomButtonStartID = 100;

        /// <summary>
        /// The start ID for radio buttons.
        /// </summary>
        /// <remarks>
        /// This must be at least 1 because 0 already stands for "no button".
        /// </remarks>
        internal const int RadioButtonStartID = 1;


        private TaskDialogCommonButtonCollection commonButtons;

        private TaskDialogCustomButtonCollection customButtons;

        private TaskDialogRadioButtonCollection radioButtons;

        private TaskDialogExpander expander;

        private TaskDialogProgressBar progressBar;

        private TaskDialogCheckBox checkBox;

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

        private TaskDialog boundTaskDialog;

        private bool boundIconIsFromHandle;

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
                // We must deny this if we are bound because we need to be able to
                // access the controls from the task dialog's callback.
                this.DenyIfBound();

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
                // We must deny this if we are bound because we need to be able to
                // access the controls from the task dialog's callback.
                this.DenyIfBound();

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
                // We must deny this if we are bound because we need to be able to
                // access the controls from the task dialog's callback.
                this.DenyIfBound();

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
        /// <remarks>
        /// This property can be set while the dialog is shown.
        /// </remarks>
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
        /// This property can be set while the dialog is shown.
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
        /// This property can be set while the dialog is shown.
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
        /// This property can be set while the dialog is shown.
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
        /// This property can be set while the dialog is shown.
        /// </remarks>
        [DefaultValue(TaskDialogIcon.None)]
        public TaskDialogIcon Icon
        {
            get => this.icon;

            set
            {
                // The value must be a integer resource passed through the
                // MAKEINTRESOURCEW macro, which casts the value to a WORD and
                // then to a ULONG_PTR and LPWSTR, so its range is 16 bit (unsigned).
                // Values outside of that range could cause an AccessViolationException
                // since the native implementation would treat it as string pointer
                // and dereference it in order to read the resource name from the
                // string, but we don't support this.
                if (value < ushort.MinValue || (int)value > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (this.boundTaskDialog != null &&
                        this.boundIconIsFromHandle)
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
        /// This property can be set while the dialog is shown.
        /// </remarks>
        [Browsable(false)]
        public IntPtr IconHandle
        {
            get => this.iconHandle;

            set
            {
                if (this.boundTaskDialog != null &&
                        !this.boundIconIsFromHandle)
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
        /// This property can be set while the dialog is shown.
        /// </remarks>
        [DefaultValue(TaskDialogIcon.None)]
        public TaskDialogIcon FooterIcon
        {
            get => this.footerIcon;

            set
            {
                // See comments in property "Icon".
                if (value < ushort.MinValue || (int)value > ushort.MaxValue)
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (this.boundTaskDialog != null &&
                        this.boundFooterIconIsFromHandle)
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
        /// This property can be set while the dialog is shown.
        /// </remarks>
        [Browsable(false)]
        public IntPtr FooterIconHandle
        {
            get => this.footerIconHandle;

            set
            {
                if (this.boundTaskDialog != null &&
                        !this.boundFooterIconIsFromHandle)
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
        /// Indicates that the width of the task dialog is determined by the width
        /// of its content area (similar to Message Box sizing behavior).
        /// </summary>
        /// <remarks>
        /// This flag is ignored if <see cref="Width"/> is not set to <c>0</c>.
        /// </remarks>
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

            // We also need to validate the controls since they could also be assigned to
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
                if (!button.IsCreatable)
                    throw new InvalidOperationException("The text of a custom button must not be null or empty.");
            }

            bool foundCheckedRadioButton = false;
            foreach (var button in this.radioButtons)
            {
                if (!button.IsCreatable)
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
                out IntPtr iconValue,
                out IntPtr footerIconValue,
                out int defaultButtonID,
                out int defaultRadioButtonID)
        {
            //// This method assumes Validate() has already been called.

            this.boundTaskDialog = owner;
            flags = this.flags;

            this.boundIconIsFromHandle = this.iconHandle != IntPtr.Zero;
            if (this.boundIconIsFromHandle)
            {
                flags |= TaskDialogFlags.UseHIconMain;
                iconValue = this.iconHandle;
            }
            else
            {                
                iconValue = (IntPtr)this.icon;
            }

            this.boundFooterIconIsFromHandle = this.footerIconHandle != IntPtr.Zero;
            if (this.boundFooterIconIsFromHandle)
            {
                flags |= TaskDialogFlags.UseHIconFooter;
                footerIconValue = this.footerIconHandle;
            }
            else
            {
                footerIconValue = (IntPtr)this.footerIcon;
            }

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
            buttonFlags = default;
            foreach (var commonButton in commonButtons)
            {
                flags |= commonButton.Bind(this);
                buttonFlags |= commonButton.GetButtonFlag();

                if (commonButton.Visible && commonButton.DefaultButton && defaultButtonID == 0)
                    defaultButtonID = commonButton.ButtonID;
            }

            for (int i = 0; i < customButtons.Count; i++)
            {
                var customButton = customButtons[i];
                flags |= customButton.Bind(this, CustomButtonStartID + i);

                if (customButton.DefaultButton && defaultButtonID == 0)
                    defaultButtonID = customButton.ButtonID;
            }

            defaultRadioButtonID = 0;
            for (int i = 0; i < radioButtons.Count; i++)
            {
                var radioButton = radioButtons[i];
                flags |= radioButton.Bind(this, RadioButtonStartID + i);

                if (radioButton.Checked && defaultRadioButtonID == 0)
                    defaultRadioButtonID = radioButton.RadioButtonID;
                else if (radioButton.Checked)
                    radioButton.Checked = false;
            }

            if (defaultRadioButtonID == 0)
                flags |= TaskDialogFlags.NoDefaultRadioButton;

            if (this.expander != null)
                flags |= this.expander.Bind(this);

            if (this.progressBar != null)
                flags |= this.progressBar.Bind(this);

            if (this.checkBox != null)
                flags |= this.checkBox.Bind(this);
        }

        internal void Unbind()
        {
            var commonButtons = this.CommonButtons;
            var customButtons = this.CustomButtons;
            var radioButtons = this.RadioButtons;

            foreach (var commonButton in commonButtons)
                commonButton.Unbind();
            
            foreach (var customButton in customButtons)
                customButton.Unbind();
            
            foreach (var radioButton in radioButtons)
                radioButton.Unbind();
            
            commonButtons.BoundTaskDialogContents = null;
            customButtons.BoundTaskDialogContents = null;
            radioButtons.BoundTaskDialogContents = null;

            this.expander?.Unbind();
            this.progressBar?.Unbind();
            this.checkBox?.Unbind();

            this.boundTaskDialog = null;
        }

        internal void ApplyInitialization()
        {
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
