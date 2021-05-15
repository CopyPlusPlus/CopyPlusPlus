using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Windows;
using System.Windows.Input;
using CopyPlusPlus.Properties;
using Hardcodet.Wpf.TaskbarNotification;
using Newtonsoft.Json;
using ToggleSwitch;

//using WK.Libraries.SharpClipboardNS;
//.net framework 4.6 not supported
//using System.Text.Json;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Is the translate API being changed or not, bool声明默认值为false
        public static bool ChangeStatus;
        private bool _firstClipboardChange = true;

        //public SharpClipboard Clipboard;

        public static TaskbarIcon NotifyIcon;

        public static bool Switch1Check;
        public static bool Switch2Check;
        public static bool Switch3Check;
        public static bool Switch4Check;

        public string TranslateId;
        public string TranslateKey;

        private ClipboardManager _windowClipboardManager;

        public MainWindow()
        {
            InitializeComponent();

            //InitializeClipboardMonitor();

            NotifyIcon = (TaskbarIcon)FindResource("MyNotifyIcon");

            NotifyIcon.Visibility = Visibility.Collapsed;

            if (Settings.Default.LastOpenDate.ToString() == "0001/1/1 0:00:00")
            {
                Settings.Default.LastOpenDate = DateTime.Today;
            }
            else
            {
                TimeSpan daySpan = DateTime.Today.Subtract(Settings.Default.LastOpenDate);
                if (daySpan.Days > 7)
                {
                    //MessageBox.Show("由于软件没有在线更新功能，因此增加了这个提示","提醒您前去公众号检查更新");
                    Settings.Default.LastOpenDate = DateTime.Today;
                }
            }


            //生成随机数,随机读取API
            var random = new Random();
            var i = random.Next(0, Api.BaiduApi.GetLength(0) - 1);
            TranslateId = Api.BaiduApi[i, 0];
            TranslateKey = Api.BaiduApi[i, 1];

            //读取上次关闭时保存的每个Switch的状态
            Switch1Check = Settings.Default.Switch1Check;
            Switch2Check = Settings.Default.Switch2Check;
            Switch3Check = Settings.Default.Switch3Check;
            Switch4Check = Settings.Default.Switch4Check;

            //Switch1默认为开启,所以判断为false,其他反之
            if (Switch1Check == false) switch1.IsChecked = false;
            if (Switch2Check) switch2.IsChecked = true;
            if (Switch3Check) switch3.IsChecked = true;
            if (Switch4Check) switch4.IsChecked = true;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            //Initialize the clipboard now that we have a window soruce to use
            _windowClipboardManager = new ClipboardManager(this);
            _windowClipboardManager.ClipboardChanged += ClipboardChanged;
        }

        private void ClipboardChanged(object sender, EventArgs e)
        {
            switch1.IsChecked = Switch1Check;
            switch2.IsChecked = Switch2Check;
            switch3.IsChecked = Switch3Check;
            switch4.IsChecked = Switch4Check;

            if (_firstClipboardChange)
            {
                // Handle your clipboard update
                if (Clipboard.ContainsText())
                {
                    //Debug.WriteLine(Clipboard.GetText());

                    // Get the cut/copied text.
                    var text = Clipboard.GetText();

                    text = text.Replace("", "");

                    //Console.WriteLine("123");

                    if (Switch1Check || Switch2Check)
                        for (var counter = 0; counter < text.Length - 1; counter++)
                        {
                            if (Switch1Check)
                                if (text[counter + 1].ToString() == "\r")
                                {
                                    if (text[counter].ToString() == ".") continue;
                                    if (text[counter].ToString() == "。") continue;
                                    text = text.Remove(counter + 1, 2);

                                    //判断英文单词结尾,则加一个空格
                                    if (Regex.IsMatch(text[counter].ToString(), "[a-zA-Z]"))
                                        text = text.Insert(counter + 1, " ");

                                    //判断"-"结尾,则去除"-"
                                    if (text[counter].ToString() == "-") text = text.Remove(counter, 1);
                                }

                            if (Switch2Check)
                                if (text[counter].ToString() == " ")
                                    text = text.Remove(counter, 1);
                        }

                    if (Switch4Check && Switch3Check == false) MessageBox.Show("当前未打开翻译功能，因此翻译弹窗不生效");

                    if (Switch3Check)
                        if (ChangeStatus == false)
                            //判断中文
                            if (!Regex.IsMatch(text, @"[\u4e00-\u9fa5]"))
                            {
                                var appId = TranslateId;
                                var secretKey = TranslateKey;
                                if (Settings.Default.AppID != "none" && Settings.Default.SecretKey != "none")
                                {
                                    appId = Settings.Default.AppID;
                                    secretKey = Settings.Default.SecretKey;
                                }

                                //这个if已经无效
                                if (appId == "none" || secretKey == "none")
                                {
                                    //MessageBox.Show("请先设置翻译接口", "Copy++");
                                    Show_InputAPIWindow();
                                }
                                else
                                {
                                    Debug.WriteLine(text);
                                    text = BaiduTrans(appId, secretKey, text);
                                    Debug.WriteLine(text);

                                    //翻译结果弹窗
                                    if (Switch4Check)
                                    {
                                        //MessageBox.Show(text);
                                        var translateResult = new TranslateResult { TextBox = { Text = text } };

                                        //translateResult.WindowStartupLocation = WindowStartupLocation.Manual;
                                        //translateResult.Left = System.Windows.Forms.Control.MousePosition.X;
                                        //translateResult.Top = System.Windows.Forms.Control.MousePosition.Y;
                                        translateResult.Show();

                                        //var left = translateResult.Left;
                                        //var top = translateResult.Top;
                                    }
                                }
                            }

                    //stop monitoring to prevent loop
                    //Clipboard.StopMonitoring();
                    //_windowClipboardManager.ClipboardChanged -= ClipboardChanged;
                    //_windowClipboardManager = null;

                    Clipboard.SetDataObject(text);

                    //_windowClipboardManager = new ClipboardManager(this);
                    //_windowClipboardManager.ClipboardChanged += ClipboardChanged;
                    //System.Windows.Clipboard.Flush();


                    //restart monitoring
                    //InitializeClipboardMonitor();
                    //_windowClipboardManager.ClipboardChanged += ClipboardChanged;
                }

                _firstClipboardChange = false;
            }
            else
            {
                _firstClipboardChange = true;
            }
        }

        private void Todolist_Checked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("欢迎赞助我，加快开发进度！");
        }

        private void Github_Click(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "https://github.com/CopyPlusPlus/CopyPlusPlus-NetFramework");
        }

        private static string BaiduTrans(string appId, string secretKey, string q = "apple")
        {
            //q为原文

            // 源语言
            var from = "auto";
            // 目标语言
            var to = "zh";

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
            request.Timeout = 6000;
            var response = (HttpWebResponse)request.GetResponse();
            var myResponseStream = response.GetResponseStream();
            var myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));
            var retString = myStreamReader.ReadToEnd();
            myStreamReader.Close();
            myResponseStream.Close();

            //read json(retString) as a object
            //var result = System.Text.Json.JsonSerializer.Deserialize<Rootobject>(retString);
            var result = JsonConvert.DeserializeObject<Rootobject>(retString);

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

        //打开翻译按钮
        private void TranslateSwitch_Check(object sender, RoutedEventArgs e)
        {
            //已内置key,故不用检查

            //string appId = Properties.Settings.Default.AppID;
            //string secretKey = Properties.Settings.Default.SecretKey;
            //if (appId == "none" || secretKey == "none")
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

        private void SwitchUncheck(object sender, RoutedEventArgs e)
        {
            var switchButton = sender as HorizontalToggleSwitch;
            var switchName = switchButton.Name;
            if (switchName == "switch1") Switch1Check = false;
            if (switchName == "switch2") Switch2Check = false;
            if (switchName == "switch3") Switch3Check = false;
            if (switchName == "switch4") Switch4Check = false;
        }

        private void SwitchCheck(object sender, RoutedEventArgs e)
        {
            var switchButton = sender as HorizontalToggleSwitch;
            var switchName = switchButton.Name;
            if (switchName == "switch1") Switch1Check = true;
            if (switchName == "switch2") Switch2Check = true;
            if (switchName == "switch3") Switch3Check = true;
            if (switchName == "switch4") Switch4Check = true;
        }


        private void MainWindow_Closed(object sender, EventArgs e)
        {
            //记录每个Switch的状态,以便下次打开恢复
            Settings.Default.Switch1Check = Switch1Check;
            Settings.Default.Switch2Check = Switch2Check;
            Settings.Default.Switch3Check = Switch3Check;
            Settings.Default.Switch4Check = Switch4Check;

            //已内置Key,无需判断
            ////判断Swith3状态,避免bug
            //if (Properties.Settings.Default.AppID == "none" || Properties.Settings.Default.SecretKey == "none")
            //{
            //    Properties.Settings.Default.Switch3Check = false;
            //}

            Settings.Default.Save();
        }

        private void MainWindow_OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
            {
                this.Hide();
                NotifyIcon.Visibility = Visibility.Visible;
                NotifyIcon.ShowBalloonTip("Copy++", "软件已最小化至托盘，点击图标显示主界面，右键可退出", BalloonIcon.Info);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Hide();
            NotifyIcon.Visibility = Visibility.Visible;
            e.Cancel = true;

            //if (!Settings.Default.FirstClose) return;

            //show balloon with custom icon
            NotifyIcon.ShowBalloonTip("Copy++", "软件已最小化至托盘，点击图标显示主界面，右键可退出", BalloonIcon.Info);
            //Settings.Default.FirstClose = false;

        }

        public static void HideNotifyIcon()
        {
            NotifyIcon.Visibility = Visibility.Collapsed;
        }
    }
}