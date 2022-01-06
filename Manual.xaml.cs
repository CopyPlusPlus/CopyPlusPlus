using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for Manual.xaml
    /// </summary>
    public partial class Manual
    {
        private bool _lineStatus;
        private bool _spaceStatus;
        private bool _widthStatus;

        public Manual()
        {
            InitializeComponent();
        }

        private void MergeLineBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_lineStatus) return;
            _lineStatus = true;

            if (!Clipboard.ContainsText()) return;
            //var thread = new Thread(() =>
            //{
            //Thread.CurrentThread.IsBackground = true;

            /* run your code here */
            var text = Clipboard.GetText();
            for (var counter = 0; counter < text.Length - 1; counter++)
                // 合并换行
                if (text[counter + 1] == '\r')
                {
                    // 如果检测到句号结尾,则不去掉换行
                    if (text[counter] == '。') continue;

                    // 去除换行
                    try
                    {
                        text = text.Remove(counter + 1, 2);
                    }
                    catch
                    {
                        text = text.Remove(counter + 1, 1);
                    }

                    //判断 英文字母 或 英文逗号 结尾, 则加一个空格
                    if (Regex.IsMatch(text[counter].ToString(), "[a-zA-Z,]"))
                        text = text.Insert(counter + 1, " ");

                    // 判断 连词符- 结尾, 且前一个字符为英文单词, 则去除"-"
                    if (text[counter] != '-' || !Regex.IsMatch(text[counter - 1].ToString(), "[a-zA-Z]")) continue;
                    text = text.Remove(counter, 1);
                    --counter;
                }

            Clipboard.SetDataObject(text);
            //});
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();

            _lineStatus = false;
        }

        private void MergeSpacesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_spaceStatus) return;
            _spaceStatus = true;

            if (!Clipboard.ContainsText()) return;

            var text = Clipboard.GetText();
            for (var counter = 0; counter < text.Length - 1; counter++)
                if (text[counter] == ' ')
                {
                    text = text.Remove(counter, 1);
                    --counter;
                }

            Clipboard.SetDataObject(text);

            _spaceStatus = false;
        }

        private void WidthBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_widthStatus) return;
            _widthStatus = true;

            if (Clipboard.ContainsText())
                Clipboard.SetDataObject(Clipboard.GetText().Normalize(NormalizationForm.FormKC));

            _widthStatus = false;
        }

        private void PinSwitch(object sender, RoutedEventArgs e)
        {
            Topmost = !Topmost;
            var converter = new BrushConverter();
            Pin.Background = Pin.Background.ToString() == "#FFFFFEFF"
                ? (Brush)converter.ConvertFromString("#00F6F2F2")
                : (Brush)converter.ConvertFromString("#FFFFFEFF");
        }

        private void Manual_OnClosed(object sender, EventArgs e)
        {
            //Get MainWindow
            if (!(Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) is
                    MainWindow mainWindow)) return;

            mainWindow.Show();
            mainWindow.GlobalSwitch = true;
        }
    }
}