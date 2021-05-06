using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace CopyPlusPlus
{
    class HotKeyCommand
    {
        public static RoutedCommand CustomRoutedCommand = new RoutedCommand();

        private void ExecutedCustomCommand(object sender,
            ExecutedRoutedEventArgs e)
        {
            MessageBox.Show("Custom Command Executed");
        }

        // CanExecuteRoutedEventHandler that only returns true if
        // the source is a control.
        private void CanExecuteCustomCommand(object sender,
            CanExecuteRoutedEventArgs e)
        {
            Control target = e.Source as Control;

            if (target != null)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
    }
}
