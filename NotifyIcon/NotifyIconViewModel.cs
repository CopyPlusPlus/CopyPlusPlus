using System.Windows;
using System.Windows.Input;

namespace CopyPlusPlus.NotifyIcon
{
    /// <summary>
    /// Provides bindable properties and commands for the NotifyIcon. In this sample, the
    /// view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    /// in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        /// <summary>
        /// Shows a window, if none is already open.
        /// </summary>
        public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        //Application.Current.MainWindow = new MainWindow();
                        Application.Current.MainWindow.Show();
                        Application.Current.MainWindow.WindowState = WindowState.Normal;
                        MainWindow.HideNotifyIcon();
                    }
                };
            }
        }

        /// <summary>
        /// Hides the main window. This command is only enabled if a window is open.
        /// </summary>
        public ICommand HideWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () => Application.Current.MainWindow.Close(),
                    CanExecuteFunc = () => Application.Current.MainWindow != null
                };
            }
        }

        /// <summary>
        /// Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() };
            }
        }

        private bool disableStatus = false;
        private bool switch1;
        private bool switch2;
        private bool switch3;
        private bool switch4;
        public ICommand DisableApp
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        switch1 = MainWindow.Switch1Check;
                        switch2 = MainWindow.Switch2Check;
                        switch3 = MainWindow.Switch3Check;
                        switch4 = MainWindow.Switch4Check;

                        MainWindow.Switch1Check = false;
                        MainWindow.Switch2Check = false;
                        MainWindow.Switch3Check = false;
                        MainWindow.Switch4Check = false;
                        disableStatus = true;
                    }
                };
            }
        }

        public ICommand EnableApp
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        if (disableStatus)
                        {
                            MainWindow.Switch1Check = switch1;
                            MainWindow.Switch2Check = switch2;
                            MainWindow.Switch3Check = switch3;
                            MainWindow.Switch4Check = switch4;
                            disableStatus = false;
                        }
                    }
                };
            }
        }
    }
}
