using System.Text;
using System.Text.RegularExpressions;
using System.Windows;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for Manual.xaml
    /// </summary>
    public partial class Manual : Window
    {
        public Manual()
        {
            InitializeComponent();
        }

        private void MergeLineBtn_Click(object sender, RoutedEventArgs e)
        {
            var text = TextBox.Text;
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
                    if (text[counter] == '-' &&
                        Regex.IsMatch(text[counter - 1].ToString(), "[a-zA-Z]"))
                        text = text.Remove(counter, 1);
                }

            TextBox.Text = text;
        }

        private void MergeSpacesBtn_Click(object sender, RoutedEventArgs e)
        {
            var text = TextBox.Text;
            for (var counter = 0; counter < text.Length - 1; counter++)
                if (text[counter] == ' ')
                    text = text.Remove(counter, 1);
            TextBox.Text = text;
        }

        private void WidthBtn_Click(object sender, RoutedEventArgs e)
        {
            TextBox.Text = TextBox.Text.Normalize(NormalizationForm.FormKC);
        }

        private void CopyBtn_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(TextBox.Text);
        }

        public void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox.Text = string.Empty;
            TextBox.GotFocus -= TextBox_GotFocus;
        }
    }
}