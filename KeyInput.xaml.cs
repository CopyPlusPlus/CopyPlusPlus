using CopyPlusPlus.Properties;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for KeyInput.xaml
    /// </summary>
    public partial class KeyInput : Window
    {
        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

        // Holds a value determining if this is the first time the box has been clicked
        // So that the text value is not always wiped out.
        private bool _hasBeenClicked1;

        private bool _hasBeenClicked2;

        public KeyInput()
        {
            InitializeComponent();

            TextBox1.Text = Settings.Default.AppID == "None" ? "点击这里输入" : Settings.Default.AppID;

            TextBox2.Text = Settings.Default.SecretKey == "None" ? "关闭窗口自动保存" : Settings.Default.SecretKey;
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            var box = (TextBox)sender;
            switch (box.Name)
            {
                case "TextBox1":
                    {
                        if (!_hasBeenClicked1)
                        {
                            box.Text = "";
                            _hasBeenClicked1 = true;
                        }

                        break;
                    }
                case "TextBox2":
                    {
                        if (!_hasBeenClicked2)
                        {
                            box.Text = "";
                            _hasBeenClicked2 = true;
                        }

                        break;
                    }
            }
        }

        private void WriteKey(object sender, EventArgs e)
        {
            if (TextBox1.Text != "点击这里输入" && TextBox1.Text != "" && TextBox1.Text != " ")
            {
                _mainWindow.TranslateId = TextBox1.Text;
                Settings.Default.AppID = TextBox1.Text;
            }

            if (TextBox2.Text != "关闭窗口自动保存" && TextBox2.Text != "" && TextBox2.Text != " ")
            {
                _mainWindow.TranslateKey = TextBox2.Text;
                Settings.Default.SecretKey = TextBox2.Text;
            }

            Settings.Default.Save();
        }
    }
}