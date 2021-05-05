using System;
using System.Windows;
using System.Windows.Input;

namespace CopyPlusPlus.NotifyIcon
{
    /// <summary>
    /// A simple command that displays the command parameter as
    /// a dialog message.
    /// </summary>
    public class ReopenMainWindowCommand : ICommand
    {
        public void Execute(object parameter)
        {
            Application.Current.MainWindow = new MainWindow();
            Application.Current.MainWindow.Show();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}