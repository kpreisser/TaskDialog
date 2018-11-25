using System;

using KPreisser.UI;

namespace TaskDialogExample
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ShowTaskDialogExample();

            Console.ReadKey();
        }

        private static void ShowTaskDialogExample()
        {
            var dialog = new TaskDialog()
            {
                Title = "Example 1",
                MainInstruction = "Hello Task Dialog!   👍",
                Content = "Hi, this is <A HREF=\"link1\">the Content</A>.\nBlah blah blah…",
                ExpandedInformation = "Expanded Information!",
                Footer = "This is the <A HREF=\"link2\">footer</A>.",

                MainIcon = TaskDialogIcon.SecuritySuccessGreenBar,
                FooterIcon = TaskDialogIcon.QuestionNoSound,

                CommonButtons = TaskDialogButtons.Yes | TaskDialogButtons.No,
                ExpandFooterArea = true,
                ShowProgressBar = true,
                UseCommandLinks = true,
                EnableHyperlinks = true,
                AllowCancel = true,
                CanBeMinimized = true,
                SizeToContent = true,
                UseTimer = true
            };

            dialog.Opened += (s, e) =>
            {
                Console.WriteLine("Dialog opened!");

                // After the dialog is opened, we can update its state.
                // Disable the "Yes" button.
                dialog.SetCommonButtonEnabled(TaskDialogResult.Yes, false);

                // Set the progress bar range and state. We will update its state
                // in the timer event.
                dialog.SetProgressBarPos(1);
            };

            long timerCount = 2;
            dialog.TimerTick += (s, e) =>
            {
                // Update the progress bar if value <= 35.
                if (timerCount <= 35)
                {
                    dialog.SetProgressBarPos((int)timerCount);
                }
                else if (timerCount == 36)
                {
                    dialog.SetProgressBarState(TaskDialogProgressBarState.Paused);
                }

                timerCount++;
            };

            dialog.HyperlinkClicked += (s, e) =>
            {
                Console.WriteLine("Hyperlink clicked!");
                TaskDialog.Show(dialog, "Clicked Hyperlink: " + e.Hyperlink, icon: TaskDialogIcon.InformationNoSound);
            };

            // Create custom buttons that are shown as command links.
            var button1 = dialog.AddCustomButton("Change Icon + Enable Buttons  ✔");
            var button2 = dialog.AddCustomButton("Disabled Button 🎵🎶\nAfter enabling, can show a new dialog.");
            var button3 = dialog.AddCustomButton("Some Admin Action…\nNavigates to a new dialog page.",
                    elevationRequired: true);

            TaskDialogIcon nextIcon = 0;
            button1.ButtonClicked += (s, e) =>
            {
                Console.WriteLine("Button1 clicked!");

                // Don't close the dialog.
                e.CancelClose = true;

                nextIcon++;

                // Set the icon and the content.
                dialog.MainIcon = nextIcon;
                dialog.MainInstruction = "Icon: " + nextIcon;
                // Update these two items.
                dialog.UpdateElements(TaskDialogUpdateElements.MainIcon | TaskDialogUpdateElements.MainInstruction);

                // Enable the "Yes" button and the 3rd button when the checkbox is set.
                dialog.SetCommonButtonEnabled(TaskDialogResult.Yes, true);
                button2.Enabled = true;
            };

            button2.Enabled = false;
            button2.ButtonClicked += (s, e) =>
            {
                Console.WriteLine("Button2 clicked!");

                // Don't close the main dialog.
                e.CancelClose = true;

                // Show a new Taskdialog that shows an incrementing number.
                var innerDialog = new TaskDialog()
                {
                    Content = "This is a new non-modal dialog!",
                    CommonButtons = TaskDialogButtons.Close | TaskDialogButtons.Continue,
                    MainIcon = TaskDialogIcon.Information,
                    UseTimer = true,
                };

                int number = 0;
                void UpdateNumberText(bool callUpdate = true)
                {
                    // Update the instruction with the new number.
                    innerDialog.MainInstruction = "Hi there!  Number: " + number.ToString();

                    if (callUpdate)
                        innerDialog.UpdateElements(TaskDialogUpdateElements.MainInstruction);
                }
                UpdateNumberText(false);

                innerDialog.TimerTick += (s2, e2) =>
                {
                    number++;
                    UpdateNumberText();
                };

                innerDialog.CommonButtonClicked += (s2, e2) =>
                {
                    Console.WriteLine("New dialog - Common Button clicked: " + e2.Button);

                    if (e2.Button == TaskDialogResult.Continue)
                    {
                        e2.CancelClose = true;
                        number += 1000;
                        UpdateNumberText();
                    }
                };

                innerDialog.Show();
                Console.WriteLine("Result of new dialog: " + innerDialog.ResultCommonButton);
            };

            button3.ButtonClicked += (s, e) =>
            {
                Console.WriteLine("Button3 clicked!");

                // Don't close the dialog from the button click.
                e.CancelClose = true;

                // Navigate to a new page.
                // Reset the dialog properties and event handlers.
                dialog.Reset(true);

                dialog.MainInstruction = "Page 2";
                dialog.Content = "Welcome to the second page!";
                dialog.VerificationText = "I think I agree…";
                dialog.MainIcon = TaskDialogIcon.SecurityShieldBlueBar;
                // Set a new icon after creating the dialog. This allows us to show the
                // yellow bar from the "SecurityWarningBar" icon with a different icon.
                dialog.MainUpdateIcon = TaskDialogIcon.Warning;
                dialog.ShowMarqueeProgressBar = true;
                dialog.SizeToContent = true;
                dialog.CommonButtons = TaskDialogButtons.Cancel;

                // Create a custom button that will be shown as regular button.
                var customButton = dialog.AddCustomButton("My Button :)");

                // Add radio buttons.
                var radioButton1 = dialog.AddRadioButton("My Radio Button 1");
                var radioButton2 = dialog.AddRadioButton("My Radio Button 2");

                radioButton1.RadioButtonClicked += (s2, e2) => Console.WriteLine("Radio Button 1 clicked!");
                radioButton2.RadioButtonClicked += (s2, e2) => Console.WriteLine("Radio Button 2 clicked!");

                dialog.VerificationClicked += (s2, e2) =>
                {
                    Console.WriteLine("Verification clicked!");

                    dialog.SetCommonButtonEnabled(TaskDialogResult.Cancel, e2.Status);
                };

                dialog.CommonButtonClicked += (s2, e2) =>
                {
                    Console.WriteLine("Common Button clicked (navigated dialog)!");
                };

                // Now navigate the dialog.
                // Instead of adding a event handler to the Navigated event, we supply a handler
                // in the Navigate() method which will only be called once (so if we do
                // another navigation, we can supply a different handler there).
                dialog.Navigate((s2, e2) =>
                {
                    Console.WriteLine("Dialog navigated!");

                    // Occurs after the dialog navigated (just like "Opened"
                    // occurs after the dialog opened).
                    dialog.SetCommonButtonEnabled(TaskDialogResult.Cancel, false);
                    dialog.SetCommonButtonElevationRequired(TaskDialogResult.Cancel, true);

                    // Enable the marquee progress bar.
                    dialog.SetProgressBarMarquee(true);
                });
            };

            dialog.CommonButtonClicked += (s, e) =>
            {
                Console.WriteLine("Common Button clicked (main dialog)!");
            };

            dialog.Show();

            Console.WriteLine("Result of main dialog: " +
                    (dialog.ResultCustomButton?.Text ?? dialog.ResultCommonButton.ToString()));
        }
    }
}

