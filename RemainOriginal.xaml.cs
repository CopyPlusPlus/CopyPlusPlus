using System.Linq;
using System.Windows;
using CopyPlusPlus.Properties;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for RemainOriginal.xaml
    /// </summary>
    public partial class RemainOriginal : Window
    {
        //Get MainWindow
        private readonly MainWindow _mainWindow =
            Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

        // 防止窗口初始化时执行 toggled
        private bool _first1 = true;
        private bool _first2 = true;

        public RemainOriginal()
        {
            InitializeComponent();

            SwitchChineseOriginal.IsOn = _mainWindow.RemainChinese;
            SwitchEnglishOriginal.IsOn = _mainWindow.RemainEnglish;
        }

        private void OnChineseToggled(object sender, RoutedEventArgs e)
        {
            if (_first1)
            {
                _first1 = false;
                return;
            }

            _mainWindow.RemainChinese = SwitchChineseOriginal.IsOn;

            Settings.Default.RemainChinese = SwitchChineseOriginal.IsOn;
            Settings.Default.Save();
        }

        private void OnEnglishToggled(object sender, RoutedEventArgs e)
        {
            if (_first2)
            {
                _first2 = false;
                return;
            }

            _mainWindow.RemainEnglish = SwitchEnglishOriginal.IsOn;

            Settings.Default.RemainEnglish = SwitchEnglishOriginal.IsOn;
            Settings.Default.Save();
        }
    }
}