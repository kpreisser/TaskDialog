using System;
using System.Windows.Forms;
using KPreisser.UI;

namespace TaskDialogExample
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            ShowTaskDialogExample();

            Console.ReadKey();
        }

        private static void ShowTaskDialogExample()
        {
            var dialogPage = new TaskDialogPage()
            {
                Title = "Example 1",
                Instruction = "Hello Task Dialog!   👍",
                Text = "Hi, this is <A HREF=\"link1\">the Content</A>.\nBlah blah blah…",
                Icon = TaskDialogStandardIcon.SecuritySuccessGreenBar,

                Footer =
                {
                    Text = "This is the <A HREF=\"link2\">footer</A>.",
                    Icon = TaskDialogStandardIcon.Warning,
                },

                Expander =
                {
                    Text = "Expanded Information!",
                    ExpandFooterArea = true
                },

                ProgressBar = new TaskDialogProgressBar(),

                CommandLinkMode = TaskDialogCommandLinkMode.CommandLinks,
                EnableHyperlinks = true,
                AllowCancel = true,
                CanBeMinimized = true,
                SizeToContent = true,
            };
            dialogPage.Created += (s, e) =>
            {
                Console.WriteLine("Main Contents created!");
            };
            dialogPage.Destroyed += (s, e) =>
            {
                Console.WriteLine("Main Contents destroyed!");
            };

            dialogPage.Expander.ExpandedChanged += (s, e) =>
            {
                Console.WriteLine("Expander Expanded Changed: " + dialogPage.Expander.Expanded);
            };

            using (var dialog = new TaskDialog(dialogPage))
            {
                dialog.Opened += (s, e) =>
                {
                    Console.WriteLine("Dialog opened!");
                };
                dialog.Shown += (s, e) =>
                {
                    Console.WriteLine("Dialog shown!");
                };
                dialog.Closing += (s, e) =>
                {
                    Console.WriteLine("Dialog closing!");
                };
                dialog.Closed += (s, e) =>
                {
                    Console.WriteLine("Dialog closed!");
                };
                //dialog.Activated += (s, e) =>
                //{
                //    Console.WriteLine("Dialog activated!");
                //};
                //dialog.Deactivated += (s, e) =>
                //{
                //    Console.WriteLine("Dialog deactivated!");
                //};

                dialogPage.ProgressBar.Value = 1;

                TaskDialogStandardButton buttonYes = dialogPage.StandardButtons.Add(TaskDialogResult.Yes);
                buttonYes.Enabled = false;
                TaskDialogStandardButton buttonNo = dialogPage.StandardButtons.Add(TaskDialogResult.No);

                // Add a hidden "Cancel" button so that we can get notified when the user 
                // closes the dialog through the window's X button or with ESC (and could
                // cancel the close operation).
                TaskDialogStandardButton buttonCancelHidden = dialogPage.StandardButtons.Add(TaskDialogResult.Cancel);
                buttonCancelHidden.Visible = false;
                buttonCancelHidden.Click += (s, e) =>
                {
                    Console.WriteLine("Cancel clicked!");
                };

                long timerCount = 2;
                var dialogPageTimer = null as Timer;
                dialogPage.Created += (s, e) =>
                {
                    dialogPageTimer = new Timer()
                    {
                        Enabled = true,
                        Interval = 200
                    };
                    dialogPageTimer.Tick += (s2, e2) =>
                    {
                        // Update the progress bar if value <= 35.
                        if (timerCount <= 35)
                        {
                            dialogPage.ProgressBar.Value = (int)timerCount;
                        }
                        else if (timerCount == 36)
                        {
                            dialogPage.ProgressBar.State = TaskDialogProgressBarState.Paused;
                        }

                        timerCount++;
                    };
                };
                dialogPage.Destroyed += (s, e) =>
                {
                    dialogPageTimer.Dispose();
                    dialogPageTimer = null;
                };

                dialogPage.HyperlinkClicked += (s, e) =>
                {
                    Console.WriteLine("Hyperlink clicked!");
                    TaskDialog.Show(dialog, "Clicked Hyperlink: " + e.Hyperlink, icon: TaskDialogStandardIcon.Information);
                };

                // Create custom buttons that are shown as command links.
                TaskDialogCustomButton button1 = dialogPage.CustomButtons.Add("Change Icon + Enable Buttons  ✔");
                TaskDialogCustomButton button2 = dialogPage.CustomButtons.Add("Disabled Button 🎵🎶", "After enabling, can show a new dialog.");
                TaskDialogCustomButton button3 = dialogPage.CustomButtons.Add("Some Admin Action…", "Navigates to a new dialog page.");
                button3.ElevationRequired = true;

                TaskDialogStandardIcon nextIcon = TaskDialogStandardIcon.SecuritySuccessGreenBar;
                button1.Click += (s, e) =>
                {
                    Console.WriteLine("Button1 clicked!");

                    // Don't close the dialog.
                    e.CancelClose = true;

                    nextIcon++;

                    // Set the icon and the content.
                    dialogPage.Icon = nextIcon;
                    dialogPage.Instruction = "Icon: " + nextIcon;

                    // Enable the "Yes" button and the 3rd button when the checkbox is set.
                    buttonYes.Enabled = true;
                    button2.Enabled = true;
                };

                button2.Enabled = false;
                button2.Click += (s, e) =>
                {
                    Console.WriteLine("Button2 clicked!");

                    // Don't close the main dialog.
                    e.CancelClose = true;

                    // Show a new Taskdialog that shows an incrementing number.
                    var newPage = new TaskDialogPage()
                    {
                        Text = "This is a new non-modal dialog!",
                        Icon = TaskDialogStandardIcon.Information,
                    };

                    TaskDialogStandardButton buttonClose = newPage.StandardButtons.Add(TaskDialogResult.Close);
                    TaskDialogStandardButton buttonContinue = newPage.StandardButtons.Add(TaskDialogResult.Continue);

                    int number = 0;
                    void UpdateNumberText(bool callUpdate = true)
                    {
                        // Update the instruction with the new number.
                        newPage.Instruction = "Hi there!  Number: " + number.ToString();
                    }
                    UpdateNumberText(false);

                    var newPageTimer = null as Timer;
                    newPage.Created += (s2, e2) =>
                    {
                        newPageTimer = new Timer()
                        {
                            Enabled = true,
                            Interval = 200
                        };
                        newPageTimer.Tick += (s3, e3) =>
                        {
                            number++;
                            UpdateNumberText();
                        };
                    };
                    newPage.Destroyed += (s2, e2) =>
                    {
                        newPageTimer.Dispose();
                        newPageTimer = null;
                    };

                    buttonContinue.Click += (s2, e2) =>
                    {
                        Console.WriteLine("New dialog - Continue Button clicked");

                        e2.CancelClose = true;
                        number += 1000;
                        UpdateNumberText();
                    };

                    using (var innerDialog = new TaskDialog(newPage))
                    {
                        TaskDialogButton innerResult = innerDialog.Show();
                        Console.WriteLine("Result of new dialog: " + innerResult);
                    }
                };

                button3.Click += (s, e) =>
                {
                    Console.WriteLine("Button3 clicked!");

                    // Don't close the dialog from the button click.
                    e.CancelClose = true;

                    // Create a new contents instance to which we will navigate the dialog.
                    var newContents = new TaskDialogPage()
                    {
                        Instruction = "Page 2",
                        Text = "Welcome to the second page!",
                        Icon = TaskDialogStandardIcon.SecurityShieldBlueBar,
                        SizeToContent = true,

                        CheckBox =
                        {
                            Text = "I think I agree…"
                        },
                        ProgressBar =
                        {
                            State = TaskDialogProgressBarState.Marquee
                        }
                    };
                    newContents.Created += (s2, e2) =>
                    {
                        Console.WriteLine("New Contents created!");

                        // Set a new icon after navigating the dialog. This allows us to show the
                        // yellow bar from the "SecurityWarningBar" icon with a different icon.
                        newContents.Icon = TaskDialogStandardIcon.Warning;
                    };
                    newContents.Destroyed += (s2, e2) =>
                    {
                        Console.WriteLine("New Contents destroyed!");
                    };

                    TaskDialogStandardButton buttonCancel = newContents.StandardButtons.Add(TaskDialogResult.Cancel);
                    buttonCancel.Enabled = false;
                    buttonCancel.ElevationRequired = true;

                    // Create a custom button that will be shown as regular button.
                    TaskDialogCustomButton customButton = newContents.CustomButtons.Add("My Button :)");

                    // Add radio buttons.
                    TaskDialogRadioButton radioButton1 = newContents.RadioButtons.Add("My Radio Button 1");
                    TaskDialogRadioButton radioButton2 = newContents.RadioButtons.Add("My Radio Button 2");
                    radioButton2.Checked = true;

                    radioButton1.CheckedChanged += (s2, e2) => Console.WriteLine(
                            $"Radio Button 1 CheckedChanged: RB1={radioButton1.Checked}, RB2={radioButton2.Checked}");
                    radioButton2.CheckedChanged += (s2, e2) => Console.WriteLine(
                            $"Radio Button 2 CheckedChanged: RB1={radioButton1.Checked}, RB2={radioButton2.Checked}");

                    newContents.CheckBox.CheckedChanged += (s2, e2) =>
                    {
                        Console.WriteLine("Checkbox CheckedChanged: " + newContents.CheckBox.Checked);

                        buttonCancel.Enabled = newContents.CheckBox.Checked;
                    };

                    // Now navigate the dialog.
                    dialog.Page = newContents;
                };

                TaskDialogButton result = dialog.Show();

                Console.WriteLine("Result of main dialog: " + result);
            }
        }
    }
}

