using System;
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

        public RemainOriginal()
        {
            InitializeComponent();
        }

        private void OnClose(object sender, EventArgs e)
        {
            _mainWindow.RemainChinese = SwitchChineseOriginal.IsOn;
            _mainWindow.RemainEnglish = SwitchEnglishOriginal.IsOn;

            Settings.Default.RemainChinese = SwitchChineseOriginal.IsOn;
            Settings.Default.RemainEnglish = SwitchEnglishOriginal.IsOn;

            Settings.Default.Save();
        }
    }
}