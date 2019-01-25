# Task Dialog for .NET (Windows)

**Note:** This repository implements the proposed API for https://github.com/dotnet/winforms/issues/146.

The Task Dialog is the successor of a MessageBox and available starting with Windows Vista. For more information,
see [About Task Dialogs](https://docs.microsoft.com/en-us/windows/desktop/Controls/task-dialogs-overview).

This project aims to provide a complete .NET implementation (C#) of the Task Dialog with all the features that
are also available in the native APIs, with all the marshalling and memory management done under the hood.

The project targets .NET Framework 4.7.2 and .NET Standard 2.0.

**Task Dialog Features:**
* Supports all of the native Task Dialog elements (like custom buttons/command links, progress bar, radio buttons, checkbox, expanded area, footer)
* Some dialog elements can be updated while the dialog is opened
* Additionally to standard icons, supports security icons that show a green, yellow, red, gray or blue bar
* Can navigate to a new page (by reconstructing the dialog from current properties)
* Can be shown modal or non-modal
* Exposes its window handle (`hWnd`) through the `Handle` property so that the dialog window can be further manipulated (or used as owner for another window)

![taskdialog-screenshot-1](https://user-images.githubusercontent.com/13289184/48280515-1b3a6e00-e454-11e8-96f3-b22a3bcff22e.png)   ![taskdialog-screenshot-2](https://user-images.githubusercontent.com/13289184/48280347-9cddcc00-e453-11e8-9bc1-605a55e8aaec.png)


## Prerequisites

To use the Task Dialog, your application needs to be compiled with a manifest that contains a dependency to
`Microsoft.Windows.Common-Controls` 6.0.0.0 (otherwise, an
[`EntryPointNotFoundException`](https://docs.microsoft.com/dotnet/api/system.entrypointnotfoundexception)
will occur when trying to show the dialog):
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

You can find a sample manifest file in the [`TaskDialog.Example`](/TaskDialog.Example) project.

Also, please make sure your `Main()` method has the
[`[STAThread]`](https://docs.microsoft.com/dotnet/api/system.stathreadattribute) attribute
(WinForms and WPF projects will have this by default). If you use the Task Dialog from a
different thread than the Main Thread, you will need to set it to
[ApartmentState.STA](https://docs.microsoft.com/dotnet/api/system.threading.apartmentstate).


## Using the Task Dialog

Show a simple dialog:
```c#
    TaskDialogResult result = TaskDialog.Show(
        text: "This is a new dialog!",
        instruction: "Hi there!",
        buttons: TaskDialogButtons.Yes | TaskDialogButtons.No,
        icon: TaskDialogIcon.Information);
```

Show a dialog with command links and a marquee progress bar:
```c#
    TaskDialogContents contents = new TaskDialogContents()
    {
        Instruction = "Hi there!",
        Text = "This is a new dialog!",
        Icon = TaskDialogIcon.Information,
        CommandLinkMode = TaskDialogCommandLinkMode.CommandLinks, // Show command links instead of custom buttons

        ProgressBar = new TaskDialogProgressBar()
        {
            State = TaskDialogProgressBarState.Marquee
        }
    };

    // Create a command link and a "Cancel" common button.
    // Note: Adding a "Cancel" button will automatically show a "X" button in
    // the dialog's title bar, and the user can press ESC to cancel the dialog.
    TaskDialogCustomButton customButton = contents.CustomButtons.Add("My Command Link");
    TaskDialogCommonButton buttonCancel = contents.CommonButtons.Add(TaskDialogResult.Cancel);

    // Show the dialog and check which button the user has clicked.
    using (TaskDialog dialog = new TaskDialog(contents))
    {
        TaskDialogButton result = dialog.Show();
    }
```

Update the dialog's content when clicking one of its buttons:

```c#
    int number = 0;

    TaskDialogContents contents = new TaskDialogContents()
    {
        Instruction = "Update number?",
        Text = $"Current number: {number}",
        Icon = (TaskDialogIcon)99
    };

    var buttonYes = contents.CommonButtons.Add(TaskDialogResult.Yes);
    var buttonClose = contents.CommonButtons.Add(TaskDialogResult.Close);

    // Handle the event when the "Yes" button was clicked.
    buttonYes.Click += (s, e) =>
    {
        // When clicking the "Yes" button, don't close the dialog, but
        // instead increment the number and update the dialog content.
        e.CancelClose = true;

        // Update the content.
        number++;
        contents.Text = $"Current number: {number}";
    };

    using (TaskDialog dialog = new TaskDialog(contents))
    {
        dialog.Show();
    }
```

For a more detailed example of a TaskDialog that uses progress bars, a timer,
hyperlinks, navigation and various event handlers (as shown by the screenshots), please
see the [`TaskDialog.Example`](/TaskDialog.Example/Program.cs) project.


### Non-modal dialog

Be aware that when you show a non-modal Task Dialog by specifying `null` or `IntPtr.Zero` as
owner window, the `TaskDialog.Show()` method will still not return until the dialog is closed;
in contrast to other implementations like `Form.Show()` (WinForms) where `Show()`
displays the window and then returns immediately.

This means that when you simultaneously show multiple non-modal Task Dialogs, the `Show()`
method will occur multiple times in the call stack (as each will run the event loop), and
therefore when you close an older dialog, its corresponding `Show()` method cannot return
until all other (newer) Task Dialogs are also closed. However, the corresponding
`TaskDialog.CommonButtonClicked` and `ITaskDialogCustomButton.ButtonClicked` events will
be called just before the dialog is closed.

E.g. if you repeatedly open a new dialog and then close a previously opened one, the 
call stack will fill with more and more `Show()` calls until all the dialogs are closed.
Note that in that case, the `TimerTick` event will also continue to be called for the
already closed dialogs until their `Show()` method can return.


## Internal details/notes

For the Task Dialog callback, a static delegate is used to avoid the overhead of creating
native functions during runtime for each new Task Dialog instance. A
[`GCHandle`](https://docs.microsoft.com/dotnet/api/system.runtime.interopservices.gchandle)
is used in the callback to map the supplied reference data back to the actual Task Dialog
instance.