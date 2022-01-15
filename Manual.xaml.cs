using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for Manual.xaml
    /// </summary>
    public partial class Manual
    {
        //Get MainWindow
        private readonly MainWindow _mainWindow =
            Application.Current.Windows.Cast<Window>().FirstOrDefault(window => window is MainWindow) as MainWindow;

        // 如果正在处理中，再次点击按钮，直接返回
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

            var text = Clipboard.GetText();

            if (text.Length > 1)
                for (var counter = 0; counter < text.Length; ++counter)
                    // 合并换行
                    if (counter >= 0 && text[counter] == '\r')
                    {
                        if (counter > 0)
                        {
                            // 如果检测到句号结尾,则不去掉换行
                            if (text[counter - 1] == '。' && _mainWindow.RemainChinese) continue;
                            if (text[counter - 1] == '.' && _mainWindow.RemainEnglish) continue;
                        }

                        // 去除换行
                        try
                        {
                            text = text.Remove(counter, 2);
                        }
                        catch
                        {
                            text = text.Remove(counter, 1);
                        }

                        --counter;

                        // 判断 非负数越界 或 句末
                        if (counter >= 0 && counter != text.Length - 1)
                            // 判断 非中文 结尾, 则加一个空格
                            if (!Regex.IsMatch(text[counter].ToString(), "[\n ，。？！《》\u4e00-\u9fa5]"))
                                text = text.Insert(counter + 1, " ");
                    }

            Clipboard.SetDataObject(text);

            _lineStatus = false;
        }

        private void MergeSpacesBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_spaceStatus) return;
            _spaceStatus = true;

            if (!Clipboard.ContainsText()) return;

            var text = Clipboard.GetText();
            for (var counter = 0; counter < text.Length - 1; counter++)
                if (counter >= 0 && text[counter] == ' ')
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
            _mainWindow.Show();
            _mainWindow.GlobalSwitch = true;
        }

        private void OnMergeRight(object sender, MouseButtonEventArgs e)
        {
            var remain = new RemainOriginal();
            remain.Show();
        }
    }
}