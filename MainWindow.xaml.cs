using CopyPlusPlus.Languages;
using CopyPlusPlus.Properties;
using GlobalHotKey;
using Gma.System.MouseKeyHook;
using GoogleTranslateFreeApi;
using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TextCopy;
using WindowsInput;
using WindowsInput.Native;

//using WK.Libraries.SharpClipboardNS;
//.net framework 4.6 not supported
//using System.Text.Json;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //Is the translate API being changed or not, bool声明默认值为false
        public static bool ChangeStatus;

        //public SharpClipboard Clipboard;

        public static TaskbarIcon NotifyIcon;

        public string TranslateId;
        public string TranslateKey;

        //private ClipboardManager _windowClipboardManager;

        //局部快捷键
        //public static RoutedCommand Copy = new RoutedCommand();

        //全局快捷键
        public HotKeyManager HotKeyManagerCopy = new HotKeyManager();
        public HotKeyManager HotKeyManager = new HotKeyManager();

        private IKeyboardMouseEvents globalMouseHook;

        public MainWindow()
        {
            InitializeComponent();

            NotifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");
            NotifyIcon.Visibility = Visibility.Collapsed;

            #region 全局快捷键示例

            //Register Ctrl+Alt+F5 hotkey. Save this variable somewhere for the further unregistering.
            //var hotKey = hotKeyManager.Register(Key.F5, ModifierKeys.Control | ModifierKeys.Alt);

            // Unregister Ctrl+Alt+F5 hotkey.
            //hotKeyManager.Unregister(hotKey);

            // Dispose the hotkey manager.
            //hotKeyManager.Dispose();

            #endregion 全局快捷键示例

            try
            {
                //// 全局监测Ctrl+C
                //HotKeyManagerCopy.Register(Key.C, ModifierKeys.Control);
                HotKeyManagerCopy.KeyPressed += CopyPressed;
                // 其他全局快捷键
                //HotKeyManager.Register(Key.Escape, ModifierKeys.None);
                HotKeyManager.Register(Key.C, ModifierKeys.Shift | ModifierKeys.Control);
                HotKeyManager.KeyPressed += HotKeyPressed;
            }
            catch
            {
                MessageBox.Show("检测到快捷键（Ctrl+C）冲突，请检查后重启软件。\n\n提示：Copy++是否已经打开？");
            }

            //局部快捷键
            //Copy.InputGestures.Add(new KeyGesture(Key.C, ModifierKeys.Control));

            //生成随机数,随机读取API
            var random = new Random();
            var i = random.Next(0, Api.BaiduApi.GetLength(0) - 1);
            TranslateId = Api.BaiduApi[i, 0];
            TranslateKey = Api.BaiduApi[i, 1];

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

            globalMouseHook = Hook.GlobalEvents();
            globalMouseHook.MouseDoubleClick += MouseDoubleClicked;
            globalMouseHook.MouseDragStarted += async (o, args) => await MouseDragStarted(o, args);
            globalMouseHook.MouseDragFinished += MouseDragFinished;
        }

        private async void MouseDoubleClicked(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            IDataObject tmpClipboard = System.Windows.Clipboard.GetDataObject();
            System.Windows.Clipboard.Clear();
            //Thread.Sleep(100);
            await Task.Delay(50);
            System.Windows.Forms.SendKeys.SendWait("^c");
            //Thread.Sleep(50);
            await Task.Delay(50);

            if (System.Windows.Clipboard.ContainsText())
            {
                string text = System.Windows.Clipboard.GetText();
                MessageBox.Show(text);
            }
            else
            {
                System.Windows.Clipboard.SetDataObject(tmpClipboard);
            }
        }

        private async Task MouseDragStarted(object sender, System.Windows.Forms.MouseEventArgs e)
        {
        }

        private async void MouseDragFinished(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            IDataObject tmpClipboard = System.Windows.Clipboard.GetDataObject();
            System.Windows.Clipboard.Clear();
            await Task.Delay(100);
            System.Windows.Forms.SendKeys.SendWait("^c");
            await Task.Delay(50);

            if (System.Windows.Clipboard.ContainsText())
            {
                string text = System.Windows.Clipboard.GetText();
                MessageBox.Show(text);
            }
            else
            {
                System.Windows.Clipboard.SetDataObject(tmpClipboard);
            }
        }

        //全局复制事件
        private void CopyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.C)
            {
                ClipboardChanged();
            }
        }

        private void HotKeyPressed(object sender, KeyPressedEventArgs e)
        {
            if (e.HotKey.Key == Key.Escape)
            {
                CloseResult();
            }

            if (e.HotKey.Key == Key.C)
            {
                //取消Ctr+C快捷键
                HotKeyManagerCopy.Dispose();
                new InputSimulator().Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
                //重设Ctr+C快捷键
                HotKeyManagerCopy = new HotKeyManager();
                HotKeyManagerCopy.Register(Key.C, ModifierKeys.Control);
                HotKeyManagerCopy.KeyPressed += CopyPressed;
            }
        }

        //按 Esc 关闭翻译结果
        private void CloseResult()
        {
            if (Application.Current.Windows
                .Cast<Window>()
                .LastOrDefault(window => window is TranslateResult) is TranslateResult transWindow)
            {
                transWindow.Close();
            }
        }

        //局部快捷键
        //private void MyCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        //{
        //    var payMe = new PayMe
        //    {
        //        Owner = this
        //    };
        //    payMe.Show();
        //}

        //protected override void OnSourceInitialized(EventArgs e)
        //{
        //    base.OnSourceInitialized(e);

        //    //Initialize the clipboard now that we have a window soruce to use
        //    _windowClipboardManager = new ClipboardManager(this);
        //    //_windowClipboardManager.ClipboardChanged += ClipboardChanged;
        //}

        //private string _textLast = "";

        //private void ClipboardChanged(object sender, EventArgs e)
        private void ClipboardChanged()
        {
            //取消Ctr+C快捷键
            HotKeyManagerCopy.Dispose();
            Thread.Sleep(10);
            new InputSimulator().Keyboard.ModifiedKeyStroke(VirtualKeyCode.CONTROL, VirtualKeyCode.VK_C);
            Thread.Sleep(10);

            if (System.Windows.Clipboard.ContainsText())
            {
                //Thread.Sleep(500);

                //var text = Clipboard.GetText();

                // Get the cut / copied text.
                //string text;
                //try
                //{
                //    text = Clipboard.GetText();
                //}
                //catch
                //{
                //    text = Clipboard.GetText();
                //}

                //TextCopy.Clipboard clipboard =new TextCopy.Clipboard();
                //string text = clipboard.GetText();

                string text = ClipboardService.GetText();

                //if (text != _textLast && _textLast != "-")
                //{
                // 去掉 CAJ viewer 造成的莫名的空格符号
                text = text.Replace("", "");

                // 全角转半角
                if (SwitchWidth.IsOn)
                {
                    text = text.Normalize(NormalizationForm.FormKC);
                }

                if (SwitchMain.IsOn || SwitchSpace.IsOn)
                    for (var counter = 0; counter < text.Length - 1; counter++)
                    {
                        //合并换行
                        if (SwitchMain.IsOn)
                            if (text[counter + 1].ToString() == "\r")
                            {
                                //如果检测到句号结尾,则不去掉换行
                                //if (text[counter].ToString() == "." || text[counter].ToString() == "。") continue;
                                if (text[counter].ToString() == "。") continue;

                                //去除换行
                                try
                                {
                                    text = text.Remove(counter + 1, 2);
                                }
                                catch
                                {
                                    text = text.Remove(counter + 1, 1);
                                }

                                //判断英文单词或,结尾,则加一个空格
                                if (Regex.IsMatch(text[counter].ToString(), "[a-zA-Z]") || text[counter].ToString() == ",")
                                    text = text.Insert(counter + 1, " ");

                                //判断"-"结尾,且前一个字符为英文单词,则去除"-"
                                if (text[counter].ToString() == "-" && Regex.IsMatch(text[counter - 1].ToString(), "[a-zA-Z]"))
                                    text = text.Remove(counter, 1);
                            }
                        //检测到中文时去除空格
                        if (SwitchSpace.IsOn && Regex.IsMatch(text, @"[\u4e00-\u9fa5]") && text[counter].ToString() == " ")
                            text = text.Remove(counter, 1);
                    }

                if (SwitchTranslate.IsOn)
                //判断是否和选中要翻译的语言相同-----移至弹窗时,检测text是否一样
                //if (!Regex.IsMatch(text, @"[\u4e00-\u9fa5]"))
                //if (TransToComboBox.Text != GoogleLanguage.GetLanguage.FirstOrDefault(x => x.Value == GoogleTrans(text.Substring(0, Math.Max(text.Length, 4)), true)).Key)
                {
                    var appId = TranslateId;
                    var secretKey = TranslateKey;
                    if (Settings.Default.AppID != "None" && Settings.Default.SecretKey != "None")
                    {
                        appId = Settings.Default.AppID;
                        secretKey = Settings.Default.SecretKey;
                    }

                    //这个if已经无效
                    //if (appId == "None" || secretKey == "None")
                    //{
                    //    //MessageBox.Show("请先设置翻译接口", "Copy++");
                    //    Show_InputAPIWindow();
                    //}
                    //else
                    //{
                    string textBeforeTrans = text;
                    //Debug.WriteLine(text);
                    switch (TransEngineComboBox.Text)
                    {
                        case "百度翻译":
                            //判断是否复制原文
                            if (SwitchCopyOriginal.IsOn)
                            {
                                //var tranResult = BaiduTrans(appId, secretKey, text);

                                ShowTrans(BaiduTrans(appId, secretKey, text), textBeforeTrans);

                                //if (tranResult.Length > 4 && tranResult.Substring(0, 4) == "翻译超时")
                                //{
                                //    ShowTrans(tranResult, textBeforeTrans);
                                //    //_textLast = "-";
                                //}
                                //else
                                //{
                                //    ShowTrans(tranResult, textBeforeTrans);
                                //}
                            }
                            else
                            {
                                text = BaiduTrans(appId, secretKey, text);
                                ShowTrans(text, textBeforeTrans);
                            }

                            break;

                        case "谷歌翻译":

                            //判断是否复制原文
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

                    //Debug.WriteLine(text);
                }
                //}

                //if (_textLast != "-")
                //{
                //    _textLast = text;
                //}

                //stop monitoring to prevent loop
                //Clipboard.StopMonitoring();
                //_windowClipboardManager.ClipboardChanged -= ClipboardChanged;
                //_windowClipboardManager = null;

                //Clipboard.SetDataObject(text);

                ClipboardService.SetText(text);

                //clipboard.SetText(text);

                //重设Ctr+C快捷键
                HotKeyManagerCopy = new HotKeyManager();
                HotKeyManagerCopy.Register(Key.C, ModifierKeys.Control);
                HotKeyManagerCopy.KeyPressed += CopyPressed;

                // _windowClipboardManager.self = true;

                //_windowClipboardManager = new ClipboardManager(this);
                //_windowClipboardManager.ClipboardChanged += ClipboardChanged;
                //System.Windows.Clipboard.Flush();

                //restart monitoring
                //InitializeClipboardMonitor();
                //_windowClipboardManager.ClipboardChanged += ClipboardChanged;
                //}
            }
        }

        //如果第一次切换到单个弹窗，则新开一个窗口，不把以前的窗口覆盖
        private bool _firstlySwitch = true;

        private System.Drawing.Point mouseSecondPoint;
        private bool isMouseDown;
        private object mouseFirstPoint;

        private void SwitchManyPopups_OnToggled(object sender, RoutedEventArgs e)
        {
            if (!SwitchManyPopups.IsOn) _firstlySwitch = true;
        }

        private void ShowTrans(string text, string textBeforeTrans)
        {
            //翻译结果弹窗
            if (SwitchPopup.IsOn && text != textBeforeTrans)
            {
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
                    //Get Window
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

            Language to = GoogleTranslator.GetLanguageByISO(GoogleLanguage.GetLanguage[TransToComboBox.Text]);

            //var result = await translator.TranslateAsync(text, from, to);
            //var text1 = text;
            var result = Task.Run(async () => await translator.TranslateAsync(text, from, to));
            if (result.Wait(TimeSpan.FromSeconds(4)))
            {
                if (detect)
                {
                    return result.Result.LanguageDetections[0].Language.ISO639;
                }
                //Console.WriteLine($"Result 1: {result.MergedTranslation}");

                //返回值一直为null，所以不用了
                //if (SwitchDictionary.IsOn)
                //{
                //    if(result.Result.ExtraTranslations != null)
                //        return result.Result.ExtraTranslations.ToString();
                //}

                return result.Result.MergedTranslation;
            }

            if (detect)
            {
                return "auto";
            }

            return "翻译超时，请检查网络，或更换翻译平台。";
        }

        //百度翻译
        private string BaiduTrans(string appId, string secretKey, string q = "apple")
        {
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
            //var result = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(retString);
            var result = JsonConvert.DeserializeObject<Rootobject>(retString);
            if (result.trans_result == null)
            {
                return "翻译超时，请检查网络，或更换翻译平台。";
            }
            return result.trans_result[0].dst;
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

        private void TransEngineComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //_textLast = "";

            //为不同的翻译引擎设置不同的语言选项
            if (TransEngineComboBox.Text == "谷歌翻译")
            {
            }
        }

        private void TransFromComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //_textLast = "";
        }

        private void TransToComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            //_textLast = "";
        }

        //打开翻译按钮
        private void TranslateSwitch_Check(object sender, RoutedEventArgs e)
        {
            //已内置key,故不用检查

            //string appId = Properties.Settings.Default.AppID;
            //string secretKey = Properties.Settings.Default.SecretKey;
            //if (appId == "None" || secretKey == "None")
            //{
            //    //MessageBox.Show("请先设置翻译接口", "Copy++");
            //    Show_InputAPIWindow();
            //}
            //switch3Check = true;
        }

        //点击"翻译"文字
        private void TranslateText_Clicked(object sender, MouseButtonEventArgs e)
        {
            Show_InputAPIWindow(false);
        }

        private void Show_InputAPIWindow(bool showMessage = true)
        {
            var keyinput = new KeyInput
            {
                Owner = this
            };

            keyinput.Show();

            if (showMessage) MessageBox.Show(keyinput, "请先设置翻译接口", "Copy++");
            ChangeStatus = true;
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
            // Dispose the hotkey manager.
            HotKeyManagerCopy.Dispose();

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

            //已内置Key,无需判断
            ////判断Swith3状态,避免bug
            //if (Properties.Settings.Default.AppID == "None" || Properties.Settings.Default.SecretKey == "None")
            //{
            //    Properties.Settings.Default.Switch3Check = false;
            //}

            Settings.Default.Save();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                NotifyIcon.Visibility = Visibility.Visible;
                NotifyIcon.ShowBalloonTip("Copy++", "软件已最小化至托盘，点击图标显示主界面，右键可退出", BalloonIcon.Info);

                Hide();
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Visible;
            NotifyIcon.ShowBalloonTip("Copy++", "软件已最小化至托盘，点击图标显示主界面，右键可退出", BalloonIcon.Info);

            Hide();

            e.Cancel = true;
        }

        public void HideNotifyIcon()
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
        }

        public void CheckUpdate()
        {
            switch (Settings.Default.LastOpenDate.ToString(CultureInfo.CurrentCulture))
            {
                //不再检查
                case "1999/7/24 0:00:00":
                    return;
                //第一次打开初始化日期
                case "2021/4/16 0:00:00":
                    Settings.Default.LastOpenDate = DateTime.Today;
                    break;

                default:
                    {
                        var daySpan = DateTime.Today.Subtract(Settings.Default.LastOpenDate);
                        if (daySpan.Days > 10)
                        {
                            var notifyUpdate = new
                                NotifyUpdate("打扰一下，您已经使用这个软件版本很久啦！\n\n或许已经有新版本了，欢迎前去公众号获取最新版。✨",
                                    "知道啦", "别再提示")
                            {
                                Owner = this
                            };
                            notifyUpdate.Show();
                            Settings.Default.LastOpenDate = DateTime.Today;
                        }
                        break;
                    }
            }
            Settings.Default.Save();
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

            //var notifyUpdate = new NotifyUpdate("打扰一下，您已经使用这个软件版本很久啦！\n\n或许已经有新版本了，欢迎前去公众号获取最新版。✨", "知道啦", "别再提示")
            //{
            //    Owner = this
            //};
            //notifyUpdate.Show();
        }

        private void MeatDown(object sender, MouseButtonEventArgs e)
        {
            Meat.Text = "🦴";
        }

        private void MeatUp(object sender, MouseButtonEventArgs e)
        {
            Meat.Text = "🍖";
        }

        private void MeatLeave(object sender, MouseEventArgs e)
        {
            Meat.Text = "🍖";
        }

        public void OnAutoStart(bool auto)
        {
            //if (WindowState == WindowState.Minimized)
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
            {
                //每次软件路径发生变化，系统会视为新软件，生成新的设置文件，因此不用担心路径发生变化
                key.SetValue("CopyPlusPlus", System.Reflection.Assembly.GetExecutingAssembly().Location + " /AutoStart");
            }
            else
            {
                key.DeleteValue("CopyPlusPlus", false);
            }
        }

        private void DiyReplace(object sender, RoutedEventArgs e)
        {
            AddReplace addReplace = new AddReplace
            {
                Owner = this
            };
            addReplace.Show();
        }
    }
}