using System.Linq;
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
        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is MainWindow) as MainWindow;

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
                        _mainWindow.CheckUpdate();
                        //MainWindow.HideNotifyIcon();
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

        //Store status before disable
        private bool _disableStatus;
        private bool _switch1Before;
        private bool _switch2Before;
        private bool _switch3Before;
        private bool _switch4Before;
        

        public ICommand DisableApp
        {
            get
            {
                return new DelegateCommand
                {
                    CanExecuteFunc = () => Application.Current.MainWindow != null,
                    CommandAction = () =>
                    {
                        _switch1Before = _mainWindow.Switch1.IsOn;
                        _switch2Before = _mainWindow.Switch2.IsOn;
                        _switch3Before = _mainWindow.Switch3.IsOn;
                        _switch4Before = _mainWindow.Switch4.IsOn;

                        _mainWindow.Switch1.IsOn = false;
                        _mainWindow.Switch2.IsOn = false;
                        _mainWindow.Switch3.IsOn = false;
                        _mainWindow.Switch4.IsOn = false;

                        _disableStatus = true;
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
                        if (_disableStatus)
                        {
                            _mainWindow.Switch1.IsOn = _switch1Before;
                            _mainWindow.Switch2.IsOn = _switch2Before;
                            _mainWindow.Switch3.IsOn = _switch3Before;
                            _mainWindow.Switch4.IsOn = _switch4Before;
                            _disableStatus = false;
                        }
                    }
                };
            }
        }
    }
}
