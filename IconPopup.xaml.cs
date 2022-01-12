using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using ControlzEx.Standard;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Timer = System.Timers.Timer;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for IconPopup.xaml
    /// </summary>
    public partial class IconPopup
    {
        private static Timer _fadeTimer;
        private static Timer _stayTimer;

        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is MainWindow) as MainWindow;

        private double _opacity = 1;

        public string CopiedText;

        [Obsolete] public POINT MouseLocation;

        public IconPopup()
        {
            InitializeComponent();

            // Create a timer
            _stayTimer = new Timer(1111);
            // Hook up the Elapsed event for the timer.
            _stayTimer.Elapsed += OnStayTimedEvent;

            // Create a timer
            _fadeTimer = new Timer(100);
            // Hook up the Elapsed event for the timer.
            _fadeTimer.Elapsed += OnFadeTimedEvent;
            _fadeTimer.AutoReset = true;

            Stay();
        }

        [DllImport("user32.dll")]
        public static extern int SetForegroundWindow(IntPtr hwnd);

        [DllImport("user32.dll")]
        [Obsolete]
        private static extern IntPtr WindowFromPoint(POINT point);

        [Obsolete]
        private async void OnLeftMouseDown(object sender, MouseButtonEventArgs e)
        {
            Hide();

            SetForegroundWindow(WindowFromPoint(MouseLocation));

            //await Task.Delay(10);

            SendKeys.SendWait("^c");

            await Task.Delay(666);
            //Thread.Sleep(1111);

            _mainWindow.ProcessText(Clipboard.GetText());
        }

        private static void Stay()
        {
            _stayTimer.Start();
        }

        private static void OnStayTimedEvent(object source, ElapsedEventArgs e)
        {
            Fade();
            _stayTimer.Stop();
        }

        private static void Fade()
        {
            _fadeTimer.Start();
        }

        private void OnFadeTimedEvent(object source, ElapsedEventArgs e)
        {
            _opacity -= 0.04;
            if (_opacity == 0) Close();

            Icon.Dispatcher.Invoke(delegate { Icon.Opacity = _opacity; });
        }

        private void Icon_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Icon.Dispatcher.Invoke(delegate { Icon.Opacity = 1; });

            _stayTimer.Stop();
            _fadeTimer.Stop();
            _opacity = 1;
        }

        private void Icon_OnMouseLeave(object sender, MouseEventArgs e)
        {
            Stay();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _fadeTimer.Stop();
            _fadeTimer.Dispose();

            _stayTimer.Stop();
            _stayTimer.Dispose();
        }
    }
}