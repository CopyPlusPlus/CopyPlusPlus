using System.Linq;
using System.Windows;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for IconPopup.xaml
    /// </summary>
    public partial class IconPopup
    {
        public string CopiedText;

        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is MainWindow) as MainWindow;

        public IconPopup()
        {
            InitializeComponent();
        }

        private void OnTranslateClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
        }

        private void OnCopyClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _mainWindow.ClipboardChanged(CopiedText);
        }
    }
}