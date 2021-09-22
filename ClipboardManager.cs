//using System;
//using System.Runtime.InteropServices;
//using System.Windows;
//using System.Windows.Interop;

//namespace CopyPlusPlus
//{
//    internal static class NativeMethods
//    {
//        // See http://msdn.microsoft.com/en-us/library/ms649021%28v=vs.85%29.aspx
//        public const int WM_CLIPBOARDUPDATE = 0x031D;
//        public static IntPtr HWND_MESSAGE = new IntPtr(-3);

//        // See http://msdn.microsoft.com/en-us/library/ms632599%28VS.85%29.aspx#message_only
//        [DllImport("user32.dll", SetLastError = true)]
//        [return: MarshalAs(UnmanagedType.Bool)]
//        public static extern bool AddClipboardFormatListener(IntPtr hwnd);
//    }

//    public class ClipboardManager
//    {
//        public event EventHandler ClipboardChanged;

//        public ClipboardManager(Window windowSource)
//        {
//            HwndSource source = PresentationSource.FromVisual(windowSource) as HwndSource;
//            if (source == null)
//            {
//                throw new ArgumentException(
//                    "Window source MUST be initialized first, such as in the Window's OnSourceInitialized handler."
//                    , nameof(windowSource));
//            }

//            source.AddHook(WndProc);

//            // get window handle for interop
//            IntPtr windowHandle = new WindowInteropHelper(windowSource).Handle;

//            // register for clipboard events
//            NativeMethods.AddClipboardFormatListener(windowHandle);
//        }



//        private void OnClipboardChanged()
//        {
//            ClipboardChanged?.Invoke(this, EventArgs.Empty);
//        }

//        private static readonly IntPtr WndProcSuccess = IntPtr.Zero;

//        private bool first = true;
//        public bool self = false;

//        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
//        {
//            if (msg == NativeMethods.WM_CLIPBOARDUPDATE)
//            {
//                //if (first || !self)
//                //{
//                    OnClipboardChanged();
//                    handled = true;
//                    //first = false;
//                //}
//            }

//            return WndProcSuccess;
//        }
//    }
//}
