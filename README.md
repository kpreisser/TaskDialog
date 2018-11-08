# Task Dialog (Windows)

The Task Dialog is the successor of a MessageBox and available starting with Windows Vista. For more information, see [About Task Dialogs](https://docs.microsoft.com/en-us/windows/desktop/Controls/task-dialogs-overview).

This project provides a comprehensive .NET implementation (C#) of the Task Dialog with all the features that are also available in the native APIs, with all the marshalling and memory management done under the hood.


## Screenshots

![taskdialog-screenshot-1](https://user-images.githubusercontent.com/13289184/48226908-313b2680-e3a1-11e8-9f7f-c8b2dba6f053.png)

![taskdialog-screenshot-2](https://user-images.githubusercontent.com/13289184/48226913-34cead80-e3a1-11e8-80b2-028c3422eacf.png)

## Prerequisites

To use the Task Dialog, your application needs to be compiled with a manifest that contains a dependency to
`Microsoft.Windows.Common-Controls` (6.0.0.0). E.g.:
```xml
<assembly manifestVersion="1.0" xmlns="urn:schemas-microsoft-com:asm.v1">
  <!-- ..... -->
  
  <!-- Enable themes for Windows common controls and dialogs (Windows XP and later) -->
  <dependency>
    <dependentAssembly>
      <assemblyIdentity
          type="win32"
          name="Microsoft.Windows.Common-Controls"
          version="6.0.0.0"
          processorArchitecture="*"
          publicKeyToken="6595b64144ccf1df"
          language="*"
        />
    </dependentAssembly>
  </dependency>
</assembly>
```

You can find a sample manifest file in the [`TaskDialog.Example`](/kpreisser/TaskDialog/tree/master/TaskDialog.Example) project.

## Using the Task Dialog

Show a simple dialog:
```c#
	TaskDialogResult result = TaskDialog.Show(
		content: "This is a new dialog!",
		instruction: "Hi there!",
		title: "My Title",
		buttons: TaskDialogButtons.Yes | TaskDialogButtons.No,
		icon: TaskDialogIcon.Information);
```

Show a more detailed Task Dialog that uses navigation and custom buttons with event handlers
(see the two screenshots above):
```c#
	var dialog = new TaskDialog()
    {
        Title = "Example 1",
        MainInstruction = "Hello Taskdialog!   👍",
        Content = "Hi, this is <A HREF=\"link1\">the Content</A>.\nBlah blah blah…",
        ExpandedInformation = "Expanded Information!",
        Footer = "This is the <A HREF=\"link2\">footer</A>.",

        MainIcon = TaskDialogIcon.SecuritySuccessBar,
        FooterIcon = TaskDialogIcon.RecycleBin,

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
        dialog.SetProgressBarRange(0, 100);
        dialog.SetProgressBarPos(1);
    };

    long timerCount = 2;
    bool stillShowsFirstPage = true;
    dialog.TimerTick += (s, e) =>
    {
        // Don't do anything if the dialog navigated to another page.
        if (!stillShowsFirstPage)
            return;

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
        TaskDialog.Show("Clicked Hyperlink: " + e.Hyperlink, icon: TaskDialogIcon.InformationNoSound);
    };

    // Create custom buttons that are shown as command links.
    var button1 = dialog.AddCustomButton("Change Icon + Enable Buttons ✔");
    var button2 = dialog.AddCustomButton("Some Admin Action…\nNavigates to a new dialog page.");
    var button3 = dialog.AddCustomButton("Disabled Button 🎵🎶\nAfter enabling, can show a new dialog.");

    TaskDialogIcon nextIcon = 0;
    button1.ButtonClicked = (s, e) =>
    {
        Console.WriteLine("Button1 clicked!");

        nextIcon++;

        // Set the icon and the content.
        dialog.MainIcon = nextIcon;
        dialog.MainInstruction = "Icon: " + nextIcon;
        // Update these two items.
        dialog.UpdateElements(TaskDialogUpdateElements.MainIcon | TaskDialogUpdateElements.MainInstruction);

        // Enable the "Yes" button and the 3rd button when the checkbox is set.
        dialog.SetCommonButtonEnabled(TaskDialogResult.Yes, true);
        button3.Enabled = true;

        // Don't close the dialog.
        return false;
    };

    button2.ButtonElevationRequiredState = true;
    button2.ButtonClicked = (s, e) =>
    {
        Console.WriteLine("Button2 clicked!");

        // Navigate to a new page.
        // Reset the dialog properties. Note that the event handlers will NOT be reset.
        dialog.Reset();

        // Ensure the timer doesn't do anything that was intended for the original page.
        stillShowsFirstPage = false;

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

        dialog.Navigated += (s2, e2) =>
        {
            Console.WriteLine("Dialog navigated!");

            // Occurs after the dialog navigated (just like "Opened"
            // occurs after the dialog opened).
            dialog.SetCommonButtonEnabled(TaskDialogResult.Cancel, false);
            dialog.SetButtonElevationRequiredState(TaskDialogResult.Cancel, true);

            // Enable the marquee progress bar.
            dialog.SetProgressBarMarquee(true);
        };

        dialog.VerificationClicked += (s2, e2) =>
        {
            Console.WriteLine("Verification clicked!");

            dialog.SetCommonButtonEnabled(TaskDialogResult.Cancel, e2.Status);
        };

        // Actually navigate the dialog.
        dialog.Navigate();

        // Don't close the dialog from the previous button click.
        return false;
    };

    button3.Enabled = false;
    button3.ButtonClicked = (s, e) =>
    {
        Console.WriteLine("Button3 clicked!");

        // Show a new Taskdialog
        var result = TaskDialog.Show(dialog.Handle,
                content: "This is a new dialog!",
                instruction: "Hi there!",
                title: "My Title",
                buttons: TaskDialogButtons.Close,
                icon: TaskDialogIcon.Information);

        Console.WriteLine("Result of new dialog: " + result);

        return false;
    };

    dialog.CommonButtonClicked = (s, e) =>
    {
        Console.WriteLine("Common Button clicked!");

        return true;
    };

    dialog.Show();

    Console.WriteLine("Result of main dialog: " +
            (dialog.ResultCustomButton?.Text ?? dialog.ResultCommonButton.ToString()));
```