using CopyPlusPlus.Properties;
using System;
using System.Windows;

namespace CopyPlusPlus
{
    /// <summary>
    /// Interaction logic for NotifyUpdate.xaml
    /// </summary>
    public partial class NotifyUpdate : Window
    {
        public int Result;

        public NotifyUpdate()
        {
            InitializeComponent();
        }

        public NotifyUpdate(string message, string buttonText1, string buttonText2)
        {
            InitializeComponent();
            Result = 0;
            Message.Text = message;
            Button1.Content = buttonText1;
            Button2.Content = buttonText2;
        }

        public void Button2_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.LastOpenDate = new DateTime(1999, 7, 24);
            Settings.Default.Save();
            this.Close();
        }

        private void Button1_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}