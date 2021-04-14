using System;
using System.Windows;
using System.Windows.Controls;

namespace CopyPlusPlus
{
    /// <summary>
    /// Interaction logic for KeyInput.xaml
    /// </summary>
    public partial class KeyInput : Window
    {
        // Holds a value determining if this is the first time the box has been clicked
        // So that the text value is not always wiped out.
        private bool hasBeenClicked1 = false;

        private bool hasBeenClicked2 = false;

        public KeyInput()
        {
            InitializeComponent();

            if (Properties.Settings.Default.AppID == "none")
            {
                textBox1.Text = "点击这里输入";
            }
            else
            {
                textBox1.Text = Properties.Settings.Default.AppID;
            }

            if (Properties.Settings.Default.SecretKey == "none")
            {
                textBox2.Text = "关闭窗口自动保存";
            }
            else
            {
                textBox2.Text = Properties.Settings.Default.SecretKey;
            }
        }

        private void ClearText(object sender, RoutedEventArgs e)
        {
            TextBox box = sender as TextBox;
            if (box.Name == "textBox1")
            {
                if (!hasBeenClicked1)
                {
                    box.Text = String.Empty;
                    hasBeenClicked1 = true;
                }
            }
            if (box.Name == "textBox2")
            {
                if (!hasBeenClicked2)
                {
                    box.Text = String.Empty;
                    hasBeenClicked2 = true;
                }
            }
        }

        private void WriteKey(object sender, EventArgs e)
        {
            if (textBox1.Text != "点击这里输入" && textBox1.Text != "" && textBox1.Text != " ")
            {
                Properties.Settings.Default.AppID = textBox1.Text;
            }
            if (textBox2.Text != "关闭窗口自动保存" && textBox2.Text != "" && textBox2.Text != " ")
            {
                Properties.Settings.Default.SecretKey = textBox2.Text;
            }
            Properties.Settings.Default.Save();

            MainWindow.changeStatus = false;
        }
    }
}