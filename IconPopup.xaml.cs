using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;

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

        public IconPopup()
        {
            InitializeComponent();

            // Create a timer
            _stayTimer = new Timer(2000);
            // Hook up the Elapsed event for the timer.
            _stayTimer.Elapsed += OnStayTimedEvent;

            // Create a timer
            _fadeTimer = new Timer(100);
            // Hook up the Elapsed event for the timer.
            _fadeTimer.Elapsed += OnFadeTimedEvent;
            _fadeTimer.AutoReset = true;

            Stay();
        }

        private void OnCopyClick(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.ProcessText(CopiedText);
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