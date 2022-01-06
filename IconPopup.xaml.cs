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
        private static Timer _aTimer;

        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is MainWindow) as MainWindow;

        private double _opacity = 1;

        public string CopiedText;

        public IconPopup()
        {
            InitializeComponent();

            Fade();
        }

        private void OnCopyClick(object sender, MouseButtonEventArgs e)
        {
            _mainWindow.ProcessText(CopiedText);
        }

        private void Fade()
        {
            // Create a timer with a two second interval.
            _aTimer = new Timer(100);
            // Hook up the Elapsed event for the timer.
            _aTimer.Elapsed += OnTimedEvent;
            _aTimer.AutoReset = true;
            _aTimer.Enabled = true;
        }

        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            _opacity -= 0.04;
            if (_opacity == 0) Close();

            Icon.Dispatcher.Invoke(delegate { Icon.Opacity = _opacity; });
        }

        private void Icon_OnMouseEnter(object sender, MouseEventArgs e)
        {
            Icon.Dispatcher.Invoke(delegate { Icon.Opacity = 1; });
            _aTimer.Stop();
            _opacity = 1;
        }

        private void Icon_OnMouseLeave(object sender, MouseEventArgs e)
        {
            _aTimer.Start();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            _aTimer.Stop();
            _aTimer.Dispose();
        }
    }
}