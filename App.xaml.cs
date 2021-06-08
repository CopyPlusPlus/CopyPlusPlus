using System.Windows;
using Hardcodet.Wpf.TaskbarNotification;

namespace CopyPlusPlus
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            //MessageBox.Show("An unhandled exception just occurred: " + e.Exception.Message, "Exception Sample", MessageBoxButton.OK, MessageBoxImage.Warning);
            e.Handled = true;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            // Application is running
            // Process command line args
            var isAutoStart = false;
            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (e.Args[i] == "/AutoStart")
                {
                    isAutoStart = true;
                }
            }

            // Create main application window, starting minimized if specified
            MainWindow mainWindow = new MainWindow();
            if (isAutoStart)
            {
                mainWindow.WindowState = WindowState.Minimized;
            }
            mainWindow.OnAutoStart();
        }
    }
}
