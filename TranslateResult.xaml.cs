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

        public TranslateResult()
        {
            InitializeComponent();

            EscEvent.InputGestures.Add(new KeyGesture(Key.Escape));
        }

        private void EscExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }

        private void PinSwitch(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            var converter = new BrushConverter();
            Pin.Background = Pin.Background.ToString() == "#FFF6F2F2"
                ? (Brush)converter.ConvertFromString("#00F6F2F2")
                : (Brush)converter.ConvertFromString("#FFF6F2F2");
        }
    }
}