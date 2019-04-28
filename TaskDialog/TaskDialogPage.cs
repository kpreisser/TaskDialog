using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using TaskDialogFlags = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_FLAGS;
using TaskDialogIconElement = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_ICON_ELEMENTS;
using TaskDialogTextElement = KPreisser.UI.TaskDialogNativeMethods.TASKDIALOG_ELEMENTS;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class TaskDialogPage
    {
        /// <summary>
        /// The start ID for custom buttons.
        /// </summary>
        /// <remarks>
        /// We need to ensure we don't use a ID that is already used for a
        /// common button (TaskDialogResult), so we start with 100 to be safe
        /// (100 is also used as first ID in MSDN examples for the task dialog).
        /// </remarks>
        private const int CustomButtonStartID = 100;

        /// <summary>
        /// The start ID for radio buttons.
        /// </summary>
        /// <remarks>
        /// This must be at least 1 because 0 already stands for "no button".
        /// </remarks>
        private const int RadioButtonStartID = 1;


        private TaskDialogCommonButtonCollection commonButtons;

        private TaskDialogCustomButtonCollection customButtons;

        private TaskDialogRadioButtonCollection radioButtons;

        private TaskDialogCheckBox checkBox;

        private TaskDialogExpander expander;

        private TaskDialogFooter footer;

        private TaskDialogProgressBar progressBar;

        private TaskDialogFlags flags;
        private string title;
        private string instruction;
        private string text;
        private TaskDialogIcon icon;
        private IntPtr iconHandle;
        private int width;
        private TaskDialogCommandLinkMode commandLinkMode;

        private TaskDialog boundTaskDialog;

        private bool boundIconIsFromHandle;


        /// <summary>
        /// Occurs after this instance is bound to a task dialog and the task dialog
        /// has created the GUI elements represented by this
        /// <see cref="TaskDialogPage"/> instance.
        /// </summary>
        /// <remarks>
        /// This will happen after showing or navigating the dialog.
        /// </remarks>
        public event EventHandler Created;

        /// <summary>
        /// Occurs when the task dialog is about to destroy the GUI elements represented
        /// by this <see cref="TaskDialogPage"/> instance and it is about to be
        /// unbound from the task dialog.
        /// </summary>
        /// <remarks>
        /// This will happen when closing or navigating the dialog.
        /// </remarks>
        public event EventHandler Destroyed;

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
        /// 
        /// </summary>
        public TaskDialogPage()
        {
            // Create empty (hidden) controls.
            this.checkBox = new TaskDialogCheckBox();
            this.expander = new TaskDialogExpander();
            this.footer = new TaskDialogFooter();
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
        public TaskDialogFooter Footer
        {
            get => this.footer;

            set
            {
                // We must deny this if we are bound because we need to be able to
                // access the control from the task dialog's callback.
                this.DenyIfBound();

                this.footer = value;
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
                        TaskDialogTextElement.TDE_MAIN_INSTRUCTION,
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
                        TaskDialogTextElement.TDE_CONTENT,
                        value);

                this.text = value;
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
                        TaskDialogIconElement.TDIE_ICON_MAIN,
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
                        TaskDialogIconElement.TDIE_ICON_MAIN,
                        value);

                this.iconHandle = value;
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
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value));

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
        [DefaultValue(false)]
        public bool EnableHyperlinks
        {
            get => GetFlag(TaskDialogFlags.TDF_ENABLE_HYPERLINKS);
            set => SetFlag(TaskDialogFlags.TDF_ENABLE_HYPERLINKS, value);
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
            get => GetFlag(TaskDialogFlags.TDF_ALLOW_DIALOG_CANCELLATION);
            set => SetFlag(TaskDialogFlags.TDF_ALLOW_DIALOG_CANCELLATION, value);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Note that once a task dialog has been opened with or has navigated to a
        /// <see cref="TaskDialogPage"/> where this flag is set, it will keep on
        /// subsequent navigations to a new <see cref="TaskDialogPage"/> even when
        /// it doesn't have this flag set.
        /// </remarks>
        [DefaultValue(false)]
        public bool RightToLeftLayout
        {
            get => GetFlag(TaskDialogFlags.TDF_RTL_LAYOUT);
            set => SetFlag(TaskDialogFlags.TDF_RTL_LAYOUT, value);
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
            get => GetFlag(TaskDialogFlags.TDF_CAN_BE_MINIMIZED);
            set => SetFlag(TaskDialogFlags.TDF_CAN_BE_MINIMIZED, value);
        }

        // TODO: Move this property to the TaskDialog since it doesn't have an effect
        // on navigation.
        // TODO: Maybe invert the property (like "SetToForeground") so that by default
        // the TDF_NO_SET_FOREGROUND flag is specified (as that is also the default
        // behavior of the MessageBox).
        /// <summary>
        /// Gets or sets a value that indicates if the task dialog should not set
        /// itself as foreground window when showing it.
        /// </summary>
        /// <remarks>
        /// When setting this property to <c>true</c> and then showing the dialog, it
        /// causes the dialog to net set itself as foreground window. This means that
        /// if currently none of the application's windows has focus, the task dialog
        /// doesn't try to "steal" focus (which otherwise can result in the task dialog
        /// window being activated, or the taskbar button for the window flashing
        /// orange). However, if the application already has focus, the task dialog
        /// window will be activated anyway.
        /// 
        /// Note: This property doesn't have an effect when navigating the task dialog.
        /// Note: This property only has an effect on Windows 8 and higher.
        /// </remarks>
        [DefaultValue(false)]
        public bool DoNotSetForeground
        {
            get => GetFlag(TaskDialogFlags.TDF_NO_SET_FOREGROUND);
            set => SetFlag(TaskDialogFlags.TDF_NO_SET_FOREGROUND, value);
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
            get => GetFlag(TaskDialogFlags.TDF_SIZE_TO_CONTENT);
            set => SetFlag(TaskDialogFlags.TDF_SIZE_TO_CONTENT, value);
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
                        "page is bound to a task dialog.");
        }

        internal TaskDialogButton GetBoundButtonByID(int buttonID)
        {
            if (this.boundTaskDialog == null)
                throw new InvalidOperationException();

            if (buttonID == 0)
                return null;

            // Check if the button is part of the custom buttons.
            var button = null as TaskDialogButton;
            if (buttonID >= CustomButtonStartID)
            {
                button = this.customButtons[buttonID - CustomButtonStartID];
            }
            else
            {
                var result = (TaskDialogResult)buttonID;
                if (this.commonButtons.Contains(result))
                    button = this.commonButtons[result];
            }

            return button;
        }

        internal TaskDialogRadioButton GetBoundRadioButtonByID(int buttonID)
        {
            if (this.boundTaskDialog == null)
                throw new InvalidOperationException();

            if (buttonID == 0)
                return null;

            return this.radioButtons[buttonID - RadioButtonStartID];
        }

        internal void Validate(TaskDialog newOwner)
        {
            //// Before assigning button IDs etc., check if the button configs are OK.
            //// This needs to be done before clearing the old button IDs and assigning
            //// the new ones, because it is possible to use the same button
            //// instances after a dialog has been created for Navigate(), where need to
            //// do the check, then release the old buttons, then assign the new
            //// buttons.

            // Check that this page instance is not already bound to another
            // TaskDialog instance. We don't throw if it is already bound to the
            // same TaskDialog instane that wants to bind now, because that should
            // be OK.
            if (this.boundTaskDialog != null && this.boundTaskDialog != newOwner)
                throw new InvalidOperationException(
                        $"This {nameof(TaskDialogPage)} instance is already bound to " +
                        $"another {nameof(TaskDialog)} instance.");

            // We also need to validate the controls since they could also be assigned to
            // another (bound) TaskDialogPage at the same time.
            // Access the collections using the property to ensure they exist.
            if (this.CommonButtons.BoundPage != null && this.CommonButtons.BoundPage != this ||
                    this.CustomButtons.BoundPage != null && this.CustomButtons.BoundPage != this ||
                    this.RadioButtons.BoundPage != null && this.RadioButtons.BoundPage != this ||
                    this.checkBox?.BoundPage != null && this.checkBox.BoundPage != this ||
                    this.expander?.BoundPage != null && this.expander.BoundPage != this ||
                    this.footer?.BoundPage != null && this.footer.BoundPage != this ||
                    this.progressBar?.BoundPage != null && this.progressBar.BoundPage != this)
                throw new InvalidOperationException();

            foreach (var control in (this.CommonButtons as IEnumerable<TaskDialogControl>)
                    .Concat(this.CustomButtons)
                    .Concat(this.RadioButtons))
                if (control.BoundPage != null && control.BoundPage != this)
                    throw new InvalidOperationException();

            if (this.CustomButtons.Count > int.MaxValue - CustomButtonStartID + 1 ||
                     this.RadioButtons.Count > int.MaxValue - RadioButtonStartID + 1)
                throw new InvalidOperationException(
                        "Too many custom buttons or radio buttons have been added.");

            //// Note: This is no longer needed, because we allow non-createable
            //// controls to be added, and the control will check the state by
            //// itself.
            //// Ensure that if we have a checkbox, its text is not null/empty.
            //// Otherwise we will get AccessViolationExceptions when sending a Click
            //// message.
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
                        throw new InvalidOperationException(
                                "Only one button can be set as default button.");
                }
            }

            // For custom and radio buttons, we need to ensure the strings are
            // not null or empty, as otherwise an error would occur when
            // showing/navigating the dialog.
            foreach (var button in this.customButtons)
            {
                if (!button.IsCreatable)
                    throw new InvalidOperationException(
                            "The text of a custom button must not be null or empty.");
            }

            bool foundCheckedRadioButton = false;
            foreach (var button in this.radioButtons)
            {
                if (!button.IsCreatable)
                    throw new InvalidOperationException(
                            "The text of a radio button must not be null or empty.");

                if (button.Checked)
                {
                    if (!foundCheckedRadioButton)
                        foundCheckedRadioButton = true;
                    else
                        throw new InvalidOperationException(
                                "Only one radio button can be set as checked.");
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
            if (this.boundTaskDialog != null)
                throw new InvalidOperationException();

            //// This method assumes Validate() has already been called.

            this.boundTaskDialog = owner;
            flags = this.flags;

            this.boundIconIsFromHandle = this.iconHandle != IntPtr.Zero;
            if (this.boundIconIsFromHandle)
            {
                flags |= TaskDialogFlags.TDF_USE_HICON_MAIN;
                iconValue = this.iconHandle;
            }
            else
            {
                iconValue = (IntPtr)this.icon;
            }

            // Only specify the command link flags if there actually are custom buttons;
            // otherwise the dialog will not work.
            if (this.customButtons.Count > 0)
            {
                if (this.commandLinkMode == TaskDialogCommandLinkMode.CommandLinks)
                    flags |= TaskDialogFlags.TDF_USE_COMMAND_LINKS;
                else if (this.commandLinkMode == TaskDialogCommandLinkMode.CommandLinksNoIcon)
                    flags |= TaskDialogFlags.TDF_USE_COMMAND_LINKS_NO_ICON;
            }

            var commonButtons = this.CommonButtons;
            var customButtons = this.CustomButtons;
            var radioButtons = this.RadioButtons;

            commonButtons.BoundPage = this;
            customButtons.BoundPage = this;
            radioButtons.BoundPage = this;

            // Assign IDs to the buttons based on their index.
            // Note: The collections will be locked while this page is bound, so we
            // don't need to copy them here.
            defaultButtonID = 0;
            buttonFlags = default;
            foreach (var commonButton in commonButtons)
            {
                flags |= commonButton.Bind(this);

                if (commonButton.IsCreated)
                {
                    buttonFlags |= commonButton.GetButtonFlag();

                    if (commonButton.DefaultButton && defaultButtonID == 0)
                        defaultButtonID = commonButton.ButtonID;
                }
            }

            for (int i = 0; i < customButtons.Count; i++)
            {
                var customButton = customButtons[i];
                flags |= customButton.Bind(this, CustomButtonStartID + i);

                if (customButton.IsCreated)
                {
                    if (customButton.DefaultButton && defaultButtonID == 0)
                        defaultButtonID = customButton.ButtonID;
                }
            }

            defaultRadioButtonID = 0;
            for (int i = 0; i < radioButtons.Count; i++)
            {
                var radioButton = radioButtons[i];
                flags |= radioButton.Bind(this, RadioButtonStartID + i);

                if (radioButton.IsCreated)
                {
                    if (radioButton.Checked && defaultRadioButtonID == 0)
                        defaultRadioButtonID = radioButton.RadioButtonID;
                    else if (radioButton.Checked)
                        radioButton.Checked = false;
                }
            }

            if (defaultRadioButtonID == 0)
                flags |= TaskDialogFlags.TDF_NO_DEFAULT_RADIO_BUTTON;

            if (this.checkBox != null)
                flags |= this.checkBox.Bind(this);

            if (this.expander != null)
                flags |= this.expander.Bind(this);

            if (this.footer != null)
                flags |= this.footer.Bind(this, out footerIconValue);
            else
                footerIconValue = default;

            if (this.progressBar != null)
                flags |= this.progressBar.Bind(this);
        }

        internal void Unbind()
        {
            if (this.boundTaskDialog == null)
                throw new InvalidOperationException();

            var commonButtons = this.CommonButtons;
            var customButtons = this.CustomButtons;
            var radioButtons = this.RadioButtons;

            foreach (var commonButton in commonButtons)
                commonButton.Unbind();

            foreach (var customButton in customButtons)
                customButton.Unbind();

            foreach (var radioButton in radioButtons)
                radioButton.Unbind();

            commonButtons.BoundPage = null;
            customButtons.BoundPage = null;
            radioButtons.BoundPage = null;

            this.checkBox?.Unbind();
            this.expander?.Unbind();
            this.footer?.Unbind();
            this.progressBar?.Unbind();

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

            this.checkBox?.ApplyInitialization();
            this.expander?.ApplyInitialization();
            this.footer?.ApplyInitialization();
            this.progressBar?.ApplyInitialization();
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
        internal protected void OnDestroyed(EventArgs e)
        {
            this.Destroyed?.Invoke(this, e);
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
