using System;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for TranslateResult.xaml
    /// </summary>
    public partial class TranslateResult
    {
        public static RoutedCommand EscEvent = new RoutedCommand();

        private static Timer _fadeTimer;
        private static Timer _stayTimer;

        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is MainWindow) as MainWindow;

        private double _opacity = 1;

        public TranslateResult()
        {
            InitializeComponent();

            EscEvent.InputGestures.Add(new KeyGesture(Key.Escape));

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

        private void EscExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void PinSwitch(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            var converter = new BrushConverter();
            Pin.Background = Pin.Background.ToString() == "#FFFFFEFF"
                ? (Brush)converter.ConvertFromString("#00F6F2F2")
                : (Brush)converter.ConvertFromString("#FFFFFEFF");
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

            Dispatcher.Invoke(delegate { Opacity = _opacity; });
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            Dispatcher.Invoke(delegate { Opacity = 1; });

            _stayTimer.Stop();
            _fadeTimer.Stop();
            _opacity = 1;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
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