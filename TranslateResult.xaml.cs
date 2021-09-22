using System.Windows;
using System.Windows.Input;

namespace CopyPlusPlus
{
    /// <summary>
    /// Interaction logic for TranslateResult.xaml
    /// </summary>
    public partial class TranslateResult : Window
    {
        public static RoutedCommand EscEvent= new RoutedCommand();

        public TranslateResult()
        {
            InitializeComponent();

            EscEvent.InputGestures.Add(new KeyGesture(Key.Escape));
        }

        private void EscExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Close();
        }
    }
}