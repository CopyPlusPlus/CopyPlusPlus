using CopyPlusPlus.Languages;
using CopyPlusPlus.Properties;
using Gma.System.MouseKeyHook;
using GoogleTranslateFreeApi;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static TaskbarIcon NotifyIcon;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly IKeyboardMouseEvents _globalMouseKeyHook;

        //如果第一次切换到单个弹窗，则新开一个窗口，不把以前的窗口覆盖
        private bool _firstlySwitch = true;

        public int IconPopupX, IconPopupY;

        public string TranslateId;
        public string TranslateKey;

        public MainWindow()
        {
            InitializeComponent();

            NotifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");
            NotifyIcon.Visibility = Visibility.Visible;

            IconPopupX = 10;
            IconPopupY = 15;

            // 读取 key
            if (Settings.Default.AppID != "None" && Settings.Default.SecretKey != "None")
            {
                if (string.IsNullOrWhiteSpace(Settings.Default.AppID) || string.IsNullOrWhiteSpace(Settings.Default.SecretKey))
                {
                    System.Windows.MessageBox.Show("请检查百度翻译的Key设置");
                }
                TranslateId = Settings.Default.AppID;
                TranslateKey = Settings.Default.SecretKey;
            }
            else
            {
                TranslateId = null;
                TranslateKey = null;
            }

            //读取上次关闭时保存的每个Switch的状态
            var checkList = Settings.Default.SwitchCheck.Cast<string>().ToList();
            SwitchMain.IsOn = Convert.ToBoolean(checkList[0]);
            SwitchSpace.IsOn = Convert.ToBoolean(checkList[1]);
            SwitchWidth.IsOn = Convert.ToBoolean(checkList[2]);
            SwitchTranslate.IsOn = Convert.ToBoolean(checkList[3]);
            SwitchManyPopups.IsOn = Convert.ToBoolean(checkList[4]);
            SwitchAutoStart.IsOn = Convert.ToBoolean(checkList[5]);
            SwitchPopup.IsOn = Convert.ToBoolean(checkList[6]);
            SwitchCopyOriginal.IsOn = Convert.ToBoolean(checkList[7]);
            TransFromComboBox.SelectedIndex = Convert.ToInt32(checkList[8]);
            TransToComboBox.SelectedIndex = Convert.ToInt32(checkList[9]);
            TransEngineComboBox.SelectedIndex = Convert.ToInt32(checkList[10]);
            SwitchSelectText.IsOn = Convert.ToBoolean(checkList[11]);
            SwitchShortcut.IsOn = Convert.ToBoolean(checkList[12]);

            _globalMouseKeyHook = Hook.GlobalEvents();

            _globalMouseKeyHook.MouseClick += OnMouseClick;
            //_globalMouseKeyHook.MouseDoubleClick += OnMouseDoubleClick;
            _globalMouseKeyHook.MouseDragFinished += OnMouseDragFinished;
            _globalMouseKeyHook.MouseWheel += OnMouseWheel;

            var keySequence = Sequence.FromString("Control+C,Control+C");
            Action actionAfterCopy = AfterKeySequence;
            Hook.GlobalEvents().OnSequence(new Dictionary<Sequence, Action>
            {
                { keySequence, actionAfterCopy }
            });
        }

        private async void AfterKeySequence()
        {
            if (SwitchShortcut.IsOn == false) return;

            await Task.Delay(50);
            if (Clipboard.ContainsText())
                ProcessText(Clipboard.GetText());
        }

        private static void OnMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Clicks != 1) return;

            Application.Current.Windows
                .Cast<Window>()
                .LastOrDefault(window => window is IconPopup popup)?.Close();
        }

        private static void OnMouseWheel(object sender, MouseEventArgs e)
        {
            Application.Current.Windows
                .Cast<Window>()
                .LastOrDefault(window => window is IconPopup popup)?.Close();
        }

        private async void OnMouseDoubleClick(object sender, MouseEventArgs e)
        {
            var tmpClipboard = Clipboard.GetDataObject();
            Clipboard.Clear();
            await Task.Delay(50);
            SendKeys.SendWait("^c");
            await Task.Delay(50);

            if (Clipboard.ContainsText())
            {
                try
                {
                    var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                    var mouse = transform.Transform(new Point(e.X, e.Y));
                    var iconPopup = new IconPopup
                    {
                        Left = mouse.X + 10,
                        Top = mouse.Y + 20
                    };
                    iconPopup.Show();
                    iconPopup.CopiedText = Clipboard.GetText();
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                if (tmpClipboard != null) Clipboard.SetDataObject(tmpClipboard);
            }
        }

        private async void OnMouseDragFinished(object sender, MouseEventArgs e)
        {
            if (SwitchSelectText.IsOn == false) return;

            var tmpClipboard = Clipboard.GetDataObject();
            Clipboard.Clear();
            await Task.Delay(50);
            SendKeys.SendWait("^c");
            await Task.Delay(50);

            if (Clipboard.ContainsText())
            {
                try
                {
                    var transform = PresentationSource.FromVisual(this).CompositionTarget.TransformFromDevice;
                    var mouse = transform.Transform(new Point(e.X, e.Y));
                    var iconPopup = new IconPopup
                    {
                        Left = mouse.X + IconPopupX,
                        Top = mouse.Y + IconPopupY,
                        ShowActivated = false,
                        Focusable = false,
                        CopiedText = Clipboard.GetText()
                    };
                    iconPopup.Show();
                }
                catch
                {
                    // ignored
                }
            }
            else
            {
                if (tmpClipboard != null) Clipboard.SetDataObject(tmpClipboard);
            }
        }

        public void ProcessText(string text)
        {
            // 去掉 CAJ viewer 造成的莫名的空格符号
            text = text.Replace("", "");

            // 全角转半角
            if (SwitchWidth.IsOn) text = text.Normalize(NormalizationForm.FormKC);

            if (SwitchMain.IsOn || SwitchSpace.IsOn)
                if (text.Length > 1)
                {
                    // 判断文本是否包含中文
                    var isChinese = Regex.IsMatch(text, @"[\u4e00-\u9fa5]");

                    for (var counter = 0; counter < text.Length - 1; counter++)
                    {
                        // 合并换行
                        if (SwitchMain.IsOn)
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

                        // 对中文去除空格
                        if (SwitchSpace.IsOn && isChinese &&
                            text[counter] == ' ')
                            text = text.Remove(counter, 1);
                    }
                }

            if (SwitchTranslate.IsOn)
            {
                var textBeforeTrans = text;

                switch (TransEngineComboBox.Text)
                {
                    case "百度翻译":
                        // 判断是否复制原文
                        if (SwitchCopyOriginal.IsOn)
                        {
                            ShowTrans(BdTrans(TranslateId, TranslateKey, text), textBeforeTrans);
                        }
                        else
                        {
                            text = BdTrans(TranslateId, TranslateKey, text);
                            ShowTrans(text, textBeforeTrans);
                        }

                        break;

                    case "谷歌翻译":

                        // 判断是否复制原文
                        if (SwitchCopyOriginal.IsOn)
                        {
                            ShowTrans(GoogleTrans(text), textBeforeTrans);
                        }
                        else
                        {
                            text = GoogleTrans(text);
                            ShowTrans(text, textBeforeTrans);
                        }

                        break;

                    case "DeepL":
                        DeepL(text);
                        break;
                }
            }

            Clipboard.SetText(text);
        }

        private void SwitchManyPopups_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!SwitchManyPopups.IsOn) _firstlySwitch = true;
        }

        //翻译结果弹窗
        private void ShowTrans(string text, string textBeforeTrans)
        {
            if (!SwitchPopup.IsOn || text == textBeforeTrans) return;
            if (SwitchManyPopups.IsOn)
            {
                var translateResult = new TranslateResult { TextBox = { Text = text } };

                //每次弹窗启动位置偏移,未实现
                //translateResult.WindowStartupLocation = WindowStartupLocation.Manual;
                //translateResult.Left = System.Windows.Forms.Control.MousePosition.X;
                //translateResult.Top = System.Windows.Forms.Control.MousePosition.Y;

                translateResult.Show();
                translateResult.TextBox.Focus();
            }
            else
            {
                if (_firstlySwitch)
                {
                    var translateResult = new TranslateResult { TextBox = { Text = text } };

                    //每次弹窗启动位置偏移,未实现
                    //translateResult.WindowStartupLocation = WindowStartupLocation.Manual;
                    //translateResult.Left = System.Windows.Forms.Control.MousePosition.X;
                    //translateResult.Top = System.Windows.Forms.Control.MousePosition.Y;

                    translateResult.Show();
                    translateResult.TextBox.Focus();

                    _firstlySwitch = false;
                    return;
                }

                // Get Window
                if (!(Application.Current.Windows
                        .Cast<Window>()
                        .LastOrDefault(window => window is TranslateResult) is TranslateResult transWindow))
                {
                    var translateResult = new TranslateResult { TextBox = { Text = text } };
                    translateResult.Show();
                    translateResult.TextBox.Focus();
                }
                else
                {
                    transWindow.TextBox.Text = text;
                }
            }
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/CopyPlusPlus/CopyPlusPlus-NetFramework");
        }

        private string GoogleTrans(string text, bool detect = false)
        {
            //初始化谷歌翻译
            var translator = new GoogleTranslator();
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            Language from;
            if (TransFromComboBox.Text == "检测语言" || detect)
                from = new Language("Automatic", "auto");
            else
                from = GoogleTranslator.GetLanguageByISO(GoogleLanguage.GetLanguage[TransFromComboBox.Text]);

            var to = GoogleTranslator.GetLanguageByISO(GoogleLanguage.GetLanguage[TransToComboBox.Text]);

            //var result = await translator.TranslateAsync(text, from, to);
            //var text1 = text;
            var result = Task.Run(async () => await translator.TranslateAsync(text, from, to));
            if (result.Wait(TimeSpan.FromSeconds(4)))
                return detect ? result.Result.LanguageDetections[0].Language.ISO639 : result.Result.MergedTranslation;
            //Console.WriteLine($"Result 1: {result.MergedTranslation}");

            //返回值一直为null，所以不用了
            //if (SwitchDictionary.IsOn)
            //{
            //    if(result.Result.ExtraTranslations != null)
            //        return result.Result.ExtraTranslations.ToString();
            //}

            return detect ? "auto" : "翻译超时，请检查网络，或更换翻译平台。";
        }

        //百度翻译
        private string BdTrans(string appId, string secretKey, string q = "apple")
        {
            if (appId == null)
            {
                //生成随机数,随机读取API
                var random = new Random();
                var i = random.Next(0, Api.BaiduApi.GetLength(0) - 1);
                TranslateId = Api.BaiduApi[i, 0];
                TranslateKey = Api.BaiduApi[i, 1];
            }

            //q为原文

            // 源语言
            //var from = "auto";
            var from = BaiduLanguage.GetLanguage[TransFromComboBox.Text];
            // 目标语言
            //var to = "zh";
            var to = BaiduLanguage.GetLanguage[TransToComboBox.Text];

            // 改成您的APP ID
            //appId = NoAPI.baidu_id;
            // 改成您的密钥
            //secretKey = NoAPI.baidu_secretKey;

            var rd = new Random();
            var salt = rd.Next(100000).ToString();
            var sign = EncryptString(appId + q + salt + secretKey);
            var url = "http://api.fanyi.baidu.com/api/trans/vip/translate?";
            url += "q=" + HttpUtility.UrlEncode(q);
            url += "&from=" + from;
            url += "&to=" + to;
            url += "&appid=" + appId;
            url += "&salt=" + salt;
            url += "&sign=" + sign;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "text/html;charset=UTF-8";
            request.UserAgent = null;
            request.Timeout = 6666;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch
            {
                return "翻译超时，请检查网络，或更换翻译平台。。";
            }

            var myResponseStream = response.GetResponseStream();
            var myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            var retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            //read json(retString) as a object
            var result = JsonConvert.DeserializeObject<Rootobject>(retString)?.trans_result[0].dst;
            return result ?? "翻译超时，请检查网络，或更换翻译平台。";
        }

        // 计算MD5值
        public static string EncryptString(string str)
        {
            var md5 = MD5.Create();
            // 将字符串转换成字节数组
            var byteOld = Encoding.UTF8.GetBytes(str);
            // 调用加密方法
            var byteNew = md5.ComputeHash(byteOld);
            // 将加密结果转换为字符串
            var sb = new StringBuilder();
            foreach (var b in byteNew)
                // 将字节转换成16进制表示的字符串，
                sb.Append(b.ToString("x2"));
            // 返回加密的字符串
            return sb.ToString();
        }

        //DeepL翻译
        public void DeepL(string text)
        {
            text = text.Replace(" ", "%20");
            Process.Start("https://www.deepl.com/translator#"
                          + DeepLanguage.GetLanguage[TransFromComboBox.Text] + "/"
                          + DeepLanguage.GetLanguage[TransToComboBox.Text] + "/" + text);
        }

        private void DeepL_OnSelected(object sender, RoutedEventArgs e)
        {
            TransFromComboBox.Items.RemoveAt(7);
            TransFromComboBox.Items.RemoveAt(7);
            TransToComboBox.Items.RemoveAt(6);
            TransToComboBox.Items.RemoveAt(6);
        }

        private void DeepL_OnUnselected(object sender, RoutedEventArgs e)
        {
            TransFromComboBox.Items.Add(new ComboBoxItem { Content = "韩语" });
            TransFromComboBox.Items.Add(new ComboBoxItem { Content = "繁体中文" });
            TransToComboBox.Items.Add(new ComboBoxItem { Content = "韩语" });
            TransToComboBox.Items.Add(new ComboBoxItem { Content = "繁体中文" });
        }

        private void Trans_OnToggled(object sender, RoutedEventArgs e)
        {
            SwitchPopup.IsEnabled = SwitchTranslate.IsOn;
            SwitchManyPopups.IsEnabled = SwitchTranslate.IsOn;
            SwitchCopyOriginal.IsEnabled = SwitchTranslate.IsOn;
            //SwitchDictionary.IsEnabled = SwitchTranslate.IsOn;
        }

        private void TransEngineComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //_textLast = "";

            //为不同的翻译引擎设置不同的语言选项
            if (TransEngineComboBox.Text == "谷歌翻译")
            {
            }
        }

        private void TransFromComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //_textLast = "";
        }

        private void TransToComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //_textLast = "";
        }

        // 鼠标右键 翻译开关 文字
        private void TranslateText_Clicked(object sender, MouseButtonEventArgs e)
        {
            var keyInput = new KeyInput
            {
                Owner = this
            };
            keyInput.Show();
        }

        //private void SwitchUncheck(object sender, RoutedEventArgs e)
        //{
        //    var switchButton = sender as ToggleSwitch;
        //    var switchName = switchButton.Name;
        //    if (switchName == "switch1") Switch1Check = false;
        //    if (switchName == "switch2") Switch2Check = false;
        //    if (switchName == "switch3") Switch3Check = false;
        //    if (switchName == "switch4") Switch4Check = false;
        //}

        //private void SwitchCheck(object sender, RoutedEventArgs e)
        //{
        //    var switchButton = sender as ToggleSwitch;
        //    var switchName = switchButton.Name;
        //    if (switchName == "switch1") Switch1Check = true;
        //    if (switchName == "switch2") Switch2Check = true;
        //    if (switchName == "switch3") Switch3Check = true;
        //    if (switchName == "switch4") Switch4Check = true;
        //}

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //记录每个Switch的状态,以便下次打开恢复
            Settings.Default.SwitchCheck[0] = SwitchMain.IsOn.ToString();
            Settings.Default.SwitchCheck[1] = SwitchSpace.IsOn.ToString();
            Settings.Default.SwitchCheck[2] = SwitchWidth.IsOn.ToString();
            Settings.Default.SwitchCheck[3] = SwitchTranslate.IsOn.ToString();
            Settings.Default.SwitchCheck[4] = SwitchManyPopups.IsOn.ToString();
            Settings.Default.SwitchCheck[5] = SwitchAutoStart.IsOn.ToString();
            Settings.Default.SwitchCheck[6] = SwitchPopup.IsOn.ToString();
            Settings.Default.SwitchCheck[7] = SwitchCopyOriginal.IsOn.ToString();
            Settings.Default.SwitchCheck[8] = TransFromComboBox.SelectedIndex.ToString();
            Settings.Default.SwitchCheck[9] = TransToComboBox.SelectedIndex.ToString();
            Settings.Default.SwitchCheck[10] = TransEngineComboBox.SelectedIndex.ToString();
            Settings.Default.SwitchCheck[11] = SwitchSelectText.IsOn.ToString();
            Settings.Default.SwitchCheck[12] = SwitchShortcut.IsOn.ToString();

            Settings.Default.Save();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState != WindowState.Minimized) return;
            NotifyIcon.Visibility = Visibility.Visible;
            NotifyIcon.ShowBalloonTip("Copy++", "软件已最小化至托盘，点击图标显示主界面，右键可退出", BalloonIcon.Info);

            Hide();
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Visible;
            NotifyIcon.ShowBalloonTip("Copy++", "软件已最小化至托盘，点击图标显示主界面，右键可退出", BalloonIcon.Info);

            Hide();

            e.Cancel = true;
        }

        public void HideNotifyIcon()
        {
            NotifyIcon.Visibility = Visibility.Visible;
        }

        public void CheckUpdate()
        {
            switch (Settings.Default.LastOpenDate.ToString("G", CultureInfo.GetCultureInfo("en-US")))
            {
                //不再检查
                case "7/24/1999 12:00:00 AM":
                    return;

                //第一次打开初始化日期
                case "4/16/2021 12:00:00 AM":
                    Settings.Default.LastOpenDate = DateTime.Today;
                    Settings.Default.Save();
                    break;

                default:
                    var daySpan = DateTime.Today.Subtract(Settings.Default.LastOpenDate);
                    if (daySpan.Days > 30)
                    {
                        var notifyUpdate = new
                            NotifyUpdate("打扰一下，您已经使用这个软件版本很久啦！\n\n或许已经有新版本了，欢迎前去公众号获取最新版。✨",
                                "知道啦", "别再提示")
                        {
                            Owner = this
                        };
                        notifyUpdate.Show();
                    }
                    break;
            }
        }

        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            CheckUpdate();
        }

        private void ShowPay(object sender, MouseButtonEventArgs e)
        {
            var payMe = new PayMe
            {
                Owner = this
            };
            payMe.Show();
        }

        private void MeatDown(object sender, MouseButtonEventArgs e)
        {
            Meat.Text = "🦴";
        }

        private void MeatUp(object sender, MouseButtonEventArgs e)
        {
            Meat.Text = "🍖";
        }

        private void MeatLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Meat.Text = "🍖";
        }

        public void OnAutoStart(bool auto)
        {
            if (auto)
            {
                Show();
                Hide();
                NotifyIcon.Visibility = Visibility.Visible;
            }
            else
            {
                Show();
            }
        }

        private void SwitchAutoStart_OnToggled(object sender, RoutedEventArgs e)
        {
            const string path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
            var key = Registry.CurrentUser.OpenSubKey(path, true);
            if (key == null) return;
            if (SwitchAutoStart.IsOn)
                //每次软件路径发生变化，系统会视为新软件，生成新的设置文件，因此不用担心路径发生变化
                key.SetValue("CopyPlusPlus", Assembly.GetExecutingAssembly().Location + " /AutoStart");
            else
                key.DeleteValue("CopyPlusPlus", false);
        }

        private void ManualBtn_Click(object sender, RoutedEventArgs e)
        {
            Manual manual = new Manual();
            manual.Show();
        }

        private void DiyReplace(object sender, RoutedEventArgs e)
        {
            var addReplace = new AddReplace
            {
                Owner = this
            };
            addReplace.Show();
        }
    }
}