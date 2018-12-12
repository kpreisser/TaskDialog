using System;
using System.Collections.Generic;
using System.Linq;

namespace KPreisser.UI
{
    /// <summary>
    /// 
    /// </summary>
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

        private TaskDialogVerificationCheckbox verificationCheckbox;

        private TaskDialog boundTaskDialog;

        private TaskDialogFlags flags;
        private string title;
        private string mainInstruction;
        private string content;
        private string footer;
        private TaskDialogIcon mainIcon;
        private IntPtr mainIconHandle;
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
        }


        /// <summary>
        /// 
        /// </summary>
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
        /// 
        /// </summary>
        public TaskDialogVerificationCheckbox VerificationCheckbox
        {
            get => this.verificationCheckbox;

            set
            {
                // We must deny this if we are bound because we need to be able to
                // access the control from the task dialog's callback.
                this.DenyIfBound();

                this.verificationCheckbox = value;
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
                this.DenyIfBound();

                this.title = value;
            }
        }

        /// <summary>
        /// Gets or sets the main instruction text.
        /// </summary>
        /// <remarks>
        /// This text can be changed while the dialog is shown.
        /// </remarks>
        public string MainInstruction
        {
            get => this.mainInstruction;

            set
            {
                this.mainInstruction = value;
                this.boundTaskDialog?.UpdateTextElement(
                        TaskDialogTextElement.MainInstruction,
                        value);
            }
        }

        /// <summary>
        /// Gets or sets the dialog's primary content.
        /// </summary>
        /// <remarks>
        /// This text can be changed while the dialog is shown.
        /// </remarks>
        public string Content
        {
            get => this.content;

            set
            {
                this.content = value;
                this.boundTaskDialog?.UpdateTextElement(
                        TaskDialogTextElement.Content,
                        value);
            }
        }

        /// <summary>
        /// Gets or sets the text to be displayed in the dialog's footer area.
        /// </summary>
        /// <remarks>
        /// This text can be changed while the dialog is shown.
        /// </remarks>
        public string Footer
        {
            get => this.footer;

            set
            {
                this.footer = value;
                this.boundTaskDialog?.UpdateTextElement(
                        TaskDialogTextElement.Footer,
                        value);
            }
        }

        /// <summary>
        /// Gets or sets the main icon, if <see cref="MainIconHandle"/> is
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <remarks>
        /// This icon can be changed while the dialog is shown.
        /// </remarks>
        public TaskDialogIcon MainIcon
        {
            get => this.mainIcon;

            set
            {
                if (this.boundTaskDialog != null &&
                        this.boundMainIconIsFromHandle)
                    throw new InvalidOperationException();

                this.mainIcon = value;
                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Main,
                        (IntPtr)value);
            }
        }

        /// <summary>
        /// Gets or sets the handle to the main icon. When this member is not
        /// <see cref="IntPtr.Zero"/>, the <see cref="MainIcon"/> property will
        /// be ignored.
        /// </summary>
        /// <remarks>
        /// This icon can be changed while the dialog is shown.
        /// </remarks>
        public IntPtr MainIconHandle
        {
            get => this.mainIconHandle;

            set
            {
                if (this.boundTaskDialog != null &&
                        !this.boundMainIconIsFromHandle)
                    throw new InvalidOperationException();

                this.mainIconHandle = value;
                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Main,
                        value);
            }
        }

        /// <summary>
        /// Gets or sets the footer icon, if <see cref="FooterIconHandle"/> is
        /// <see cref="IntPtr.Zero"/>.
        /// </summary>
        /// <remarks>
        /// This icon can be changed while the dialog is shown.
        /// </remarks>
        public TaskDialogIcon FooterIcon
        {
            get => this.footerIcon;

            set
            {
                if (this.boundTaskDialog != null &&
                        this.boundFooterIconIsFromHandle == true)
                    throw new InvalidOperationException();

                this.footerIcon = value;
                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Footer,
                        (IntPtr)value);
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
        public IntPtr FooterIconHandle
        {
            get => this.footerIconHandle;

            set
            {
                if (this.boundTaskDialog != null &&
                        !this.boundFooterIconIsFromHandle == false)
                    throw new InvalidOperationException();

                this.footerIconHandle = value;
                this.boundTaskDialog?.UpdateIconElement(
                        TaskDialogIconElement.Footer,
                        value);
            }
        }

        /// <summary>
        /// Gets or sets the width in dialog units that the dialog's client area will get
        /// when the dialog is is created or navigated.
        /// If <c>0</c>, the width will be automatically calculated by the system.
        /// </summary>
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
        public bool RightToLeftLayout
        {
            get => GetFlag(TaskDialogFlags.RtlLayout);
            set => SetFlag(TaskDialogFlags.RtlLayout, value);
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the task dialog can be minimized.
        /// </summary>
        public bool CanBeMinimized
        {
            get => GetFlag(TaskDialogFlags.CanBeMinimized);
            set => SetFlag(TaskDialogFlags.CanBeMinimized, value);
        }

        //// Note: This property is currently commented-out because it doesn't seem to have
        //// any effect.

        ///// <summary>
        ///// 
        ///// </summary>
        //public bool DoNotSetForeground
        //{
        //    get => GetFlag(TaskDialogFlags.NoSetForeground);
        //    set => SetFlag(TaskDialogFlags.NoSetForeground, value);
        //}

        /// <summary>
        /// 
        /// </summary>
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
                    this.verificationCheckbox?.BoundTaskDialogContents != null && this.verificationCheckbox.BoundTaskDialogContents != this)
                throw new InvalidOperationException();
            foreach (var control in (this.CommonButtons as IEnumerable<TaskDialogControl>)
                    .Concat(this.CustomButtons)
                    .Concat(this.RadioButtons))
                if (control.BoundTaskDialogContents != null && control.BoundTaskDialogContents != this)
                    throw new InvalidOperationException();

            if (this.customButtons?.Count > int.MaxValue - CustomButtonStartID + 1 ||
                    this.radioButtons?.Count > int.MaxValue - RadioButtonStartID + 1)
                throw new InvalidOperationException(
                        "Too many custom buttons or radio buttons have been added.");

            // Ensure that if we have a verification checkbox, its text is not null/empty.
            // Otherwise we will get AccessViolationExceptions when sending a Click message.
            if (this.verificationCheckbox != null &&
                    (!(this.verificationCheckbox.Text?.Length > 0) ||
                    this.verificationCheckbox.Text[0] == '\0'))
                throw new InvalidOperationException(
                    $"When a {nameof(this.VerificationCheckbox)} is set, its " +
                    $"{nameof(this.VerificationCheckbox.Text)} must not be null or empty.");

            foreach (var button in this.customButtons)
                if (button.Text == null)
                    throw new InvalidOperationException("The text of a custom button must not be null.");
            foreach (var button in this.radioButtons)
                if (button.Text == null)
                    throw new InvalidOperationException("The text of a radio button must not be null.");
        }

        internal void Bind(TaskDialog owner,
                out TaskDialogFlags flags,
                out TaskDialogButtons buttonFlags,
                out int defaultButtonID,
                out int defaultRadioButtonID)
        {
            //// This method assumes Validate() has already been called.

            this.boundTaskDialog = owner;
            flags = this.flags;
            buttonFlags = GetCommonButtonFlags();

            this.boundMainIconIsFromHandle = this.MainIconHandle != IntPtr.Zero;
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

            if (this.verificationCheckbox != null)
            {
                this.verificationCheckbox.BoundTaskDialogContents = this;
                flags |= this.verificationCheckbox.GetFlags();
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
            }

            for (int i = 0; i < radioButtons.Count; i++)
            {
                var radioButton = radioButtons[i];
                radioButton.BoundTaskDialogContents = null;
                radioButton.RadioButtonID = 0;
            }

            commonButtons.BoundTaskDialogContents = null;
            customButtons.BoundTaskDialogContents = null;
            radioButtons.BoundTaskDialogContents = null;

            if (this.expander != null)
                this.expander.BoundTaskDialogContents = null;
            if (this.progressBar != null)
                this.progressBar.BoundTaskDialogContents = null;
            if (this.verificationCheckbox != null)
                this.verificationCheckbox.BoundTaskDialogContents = null;

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
            this.verificationCheckbox?.ApplyInitialization();
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
