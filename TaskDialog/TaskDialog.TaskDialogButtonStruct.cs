using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    public partial class TaskDialog
    {
        // Packing is defined as 1 in the C header file ("pack(1)").
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        private struct TaskDialogButtonStruct
        {
            public int ButtonID;
            [MarshalAs(UnmanagedType.LPWStr)]
            public string ButtonText;
        }
    }
}
