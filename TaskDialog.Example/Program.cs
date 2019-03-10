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
            var dialogPage = new TaskDialogPage()
            {
                Title = "Example 1",
                Instruction = "Hello Task Dialog!   👍",
                Text = "Hi, this is <A HREF=\"link1\">the Content</A>.\nBlah blah blah…",
                Icon = TaskDialogIcon.SecuritySuccessGreenBar,

                Footer =
                {
                    Text = "This is the <A HREF=\"link2\">footer</A>.",
                    Icon = TaskDialogIcon.Warning,
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
                dialog.Closing += (s, e) =>
                {
                    Console.WriteLine("Dialog closing!");
                };

                dialogPage.ProgressBar.Value = 1;

                var buttonYes = dialogPage.CommonButtons.Add(TaskDialogResult.Yes);
                buttonYes.Enabled = false;
                var buttonNo = dialogPage.CommonButtons.Add(TaskDialogResult.No);

                // Add a hidden "Cancel" button so that we can get notified when the user 
                // closes the dialog through the window's X button or with ESC (and could
                // cancel the close operation).
                var buttonCancelHidden = dialogPage.CommonButtons.Add(TaskDialogResult.Cancel);
                buttonCancelHidden.Visible = false;
                buttonCancelHidden.Click += (s, e) =>
                {
                    Console.WriteLine("Cancel clicked!");
                };

                long timerCount = 2;
                dialogPage.TimerTick += (s, e) =>
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

                dialogPage.HyperlinkClicked += (s, e) =>
                {
                    Console.WriteLine("Hyperlink clicked!");
                    TaskDialog.Show(dialog, "Clicked Hyperlink: " + e.Hyperlink, icon: TaskDialogIcon.Information);
                };

                // Create custom buttons that are shown as command links.
                var button1 = dialogPage.CustomButtons.Add("Change Icon + Enable Buttons  ✔");
                var button2 = dialogPage.CustomButtons.Add("Disabled Button 🎵🎶", "After enabling, can show a new dialog.");
                var button3 = dialogPage.CustomButtons.Add("Some Admin Action…", "Navigates to a new dialog page.");
                button3.ElevationRequired = true;

                TaskDialogIcon nextIcon = 0;
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
                    var contents = new TaskDialogPage()
                    {
                        Text = "This is a new non-modal dialog!",
                        Icon = TaskDialogIcon.Information,
                    };

                    var buttonClose = contents.CommonButtons.Add(TaskDialogResult.Close);
                    var buttonContinue = contents.CommonButtons.Add(TaskDialogResult.Continue);

                    int number = 0;
                    void UpdateNumberText(bool callUpdate = true)
                    {
                        // Update the instruction with the new number.
                        contents.Instruction = "Hi there!  Number: " + number.ToString();
                    }
                    UpdateNumberText(false);

                    contents.TimerTick += (s2, e2) =>
                    {
                        number++;
                        UpdateNumberText();
                    };

                    buttonContinue.Click += (s2, e2) =>
                    {
                        Console.WriteLine("New dialog - Continue Button clicked");

                        e2.CancelClose = true;
                        number += 1000;
                        UpdateNumberText();
                    };

                    using (var innerDialog = new TaskDialog(contents))
                    {
                        var innerResult = innerDialog.Show();
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
                        Icon = TaskDialogIcon.SecurityShieldBlueBar,
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
                        newContents.Icon = TaskDialogIcon.Warning;
                    };
                    newContents.Destroyed += (s2, e2) =>
                    {
                        Console.WriteLine("New Contents destroyed!");
                    };

                    var buttonCancel = newContents.CommonButtons.Add(TaskDialogResult.Cancel);
                    buttonCancel.Enabled = false;
                    buttonCancel.ElevationRequired = true;

                    // Create a custom button that will be shown as regular button.
                    var customButton = newContents.CustomButtons.Add("My Button :)");

                    // Add radio buttons.
                    var radioButton1 = newContents.RadioButtons.Add("My Radio Button 1");
                    var radioButton2 = newContents.RadioButtons.Add("My Radio Button 2");
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

                var result = dialog.Show();

                Console.WriteLine("Result of main dialog: " + result);
            }
        }
    }
}

