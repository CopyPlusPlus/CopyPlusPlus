using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using CopyPlusPlus.Languages;
using CopyPlusPlus.Properties;
using GoogleTranslateFreeApi;
using Newtonsoft.Json;
using TextCopy;

namespace CopyPlusPlus
{
    /// <summary>
    ///     Interaction logic for IconPopup.xaml
    /// </summary>
    public partial class IconPopup
    {
        //Get MainWindow
        private readonly MainWindow _mainWindow = Application.Current.Windows
            .Cast<Window>()
            .FirstOrDefault(window => window is MainWindow) as MainWindow;

        public IconPopup()
        {
            InitializeComponent();
        }

    }
}