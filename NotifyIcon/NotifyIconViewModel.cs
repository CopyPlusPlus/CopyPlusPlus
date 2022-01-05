using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CopyPlusPlus.NotifyIcon
{
    /// <summary>
    ///     Provides bindable properties and commands for the NotifyIcon. In this sample, the
    ///     view model is assigned to the NotifyIcon in XAML. Alternatively, the startup routing
    ///     in App.xaml.cs could have created this view model, and assigned it to the NotifyIcon.
    /// </summary>
    public class NotifyIconViewModel
    {
        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is MainWindow) as MainWindow;

        /// <summary>
        ///     Shows a window, if none is already open.
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
                        if (Application.Current.MainWindow != null)
                        {
                            Application.Current.MainWindow.Show();
                            Application.Current.MainWindow.WindowState = WindowState.Normal;
                        }

                        _mainWindow.HideNotifyIcon();
                        _mainWindow.CheckUpdate();
                    }
                };
            }
        }

        /// <summary>
        ///     Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get { return new DelegateCommand { CommandAction = () => Application.Current.Shutdown() }; }
        }

        public ICommand DisableApp
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () => { _mainWindow.GlobalSwitch = false; }
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
                    CommandAction = () => { _mainWindow.GlobalSwitch = true; }
                };
            }
        }
    }
}