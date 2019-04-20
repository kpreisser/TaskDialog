using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace KPreisser.UI
{
    internal class WindowSubclassHandler : IDisposable
    {
        private readonly IntPtr handle;

        private bool opened;

        private bool disposed;

        private IntPtr originalWindowProc;

        /// <summary>
        /// The delegate for the callback handler (that calls
        /// <see cref="WndProc(int, IntPtr, IntPtr)"/> from which the native function
        /// pointer <see cref="windowProcDelegatePtr"/> is created. 
        /// </summary>
        /// <remarks>
        /// We must store this delegate (and prevent it from being garbage-collected)
        /// to ensure the function pointer doesn't become invalid.
        /// 
        /// Note: We create a new delegate (and native function pointer) for each
        /// instance because even though creation will be slower (and requires a
        /// bit of memory to store the native code) it will be faster when the window
        /// procedure is invoked, because otherwise we would need to use a dictionary
        /// to map the hWnd to the instance, as the window procedure doesn't allow
        /// to store reference data. However, this is also the way that the
        /// NativeWindow class of WinForms does it.
        /// </remarks>
        private readonly WindowSubclassHandlerNativeMethods.WindowProc windowProcDelegate;

        /// <summary>
        /// The function pointer created from <see cref="windowProcDelegate"/>.
        /// </summary>
        private readonly IntPtr windowProcDelegatePtr;


        public WindowSubclassHandler(IntPtr handle)
        {
            if (handle == IntPtr.Zero)
                throw new ArgumentNullException(nameof(handle));

            this.handle = handle;

            // Create a delegate for our window procedure, and get a function
            // pointer for it.
            this.windowProcDelegate = (hWnd, msg, wParam, lParam) =>
            {
                Debug.Assert(hWnd == this.handle);
                return this.WndProc(msg, wParam, lParam);
            };

            this.windowProcDelegatePtr = Marshal.GetFunctionPointerForDelegate(
                    this.windowProcDelegate);
        }


        /// <summary>
        /// Subclasses the window.
        /// </summary>
        /// <remarks>
        /// You must call <see cref="Dispose()"/> to undo the subclassing before
        /// the window is destroyed.
        /// </remarks>
        /// <returns></returns>
        public void Open()
        {
            if (this.disposed)
                throw new ObjectDisposedException(nameof(WindowSubclassHandler));
            if (this.opened)
                throw new InvalidOperationException();

            // Replace the existing window procedure with our one
            // ("instance subclassing").
            // We need to explicitely clear the last Win32 error and then retrieve
            // it, to check if the call succeeded.
            WindowSubclassHandlerNativeMethods.SetLastError(0);
            this.originalWindowProc = WindowSubclassHandlerNativeMethods.SetWindowLongPtr(
                    this.handle,
                    WindowSubclassHandlerNativeMethods.GWLP_WNDPROC,
                    this.windowProcDelegatePtr);
            if (this.originalWindowProc == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
                throw new Win32Exception();

            Debug.Assert(this.originalWindowProc != this.windowProcDelegatePtr);

            this.opened = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void KeepCallbackDelegateAlive()
        {
            GC.KeepAlive(this.windowProcDelegate);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                // We cannot do anything from the finalizer thread since we have
                // resoures that must only be accessed from the GUI thread.
                if (disposing && this.opened)
                {
                    // Check if the current window procedure is the correct one.
                    // We need to explicitely clear the last Win32 error and then
                    // retrieve it, to check if the call succeeded.
                    WindowSubclassHandlerNativeMethods.SetLastError(0);
                    var currentWindowProcedure = WindowSubclassHandlerNativeMethods.GetWindowLongPtr(
                            this.handle,
                            WindowSubclassHandlerNativeMethods.GWLP_WNDPROC);
                    if (currentWindowProcedure == IntPtr.Zero && Marshal.GetLastWin32Error() != 0)
                        throw new Win32Exception();

                    if (currentWindowProcedure != this.windowProcDelegatePtr)
                        throw new InvalidOperationException(
                                "The current window procedure is not the expected one.");

                    // Undo the subclassing by restoring the original window
                    // procedure.
                    WindowSubclassHandlerNativeMethods.SetLastError(0);
                    if (WindowSubclassHandlerNativeMethods.SetWindowLongPtr(
                            this.handle,
                            WindowSubclassHandlerNativeMethods.GWLP_WNDPROC,
                            this.originalWindowProc) == IntPtr.Zero &&
                            Marshal.GetLastWin32Error() != 0)
                        throw new Win32Exception();

                    // Ensure to keep the delegate alive up to the point after we
                    // have undone the subclassing.
                    this.KeepCallbackDelegateAlive();
                }

                this.disposed = true;
            }
        }

        protected virtual IntPtr WndProc(
                int msg,
                IntPtr wParam,
                IntPtr lParam)
        {
            // Call the original window procedure to process the message.
            if (this.originalWindowProc != IntPtr.Zero)
            {
                return WindowSubclassHandlerNativeMethods.CallWindowProc(
                        this.originalWindowProc,
                        this.handle,
                        msg,
                        wParam,
                        lParam);
            }

            return IntPtr.Zero;
        }
    }
}
