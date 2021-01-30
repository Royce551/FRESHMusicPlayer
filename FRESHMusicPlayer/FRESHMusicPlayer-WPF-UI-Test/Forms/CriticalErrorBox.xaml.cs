using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace FRESHMusicPlayer.Forms
{
    /// <summary>
    /// Interaction logic for CriticalErrorBox.xaml
    /// </summary>
    public partial class CriticalErrorBox : Window
    {
        public CriticalErrorBox(DispatcherUnhandledExceptionEventArgs e)
        {
            InitializeComponent();
            string logPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\\FRESHMusicPlayer\\Logs";
            string fileName = $"\\{DateTime.Now:M.d.yyyy hh mm tt}.txt";
            ContentTextBlock.Text = string.Format(Properties.Resources.APPLICATION_CRITICALERROR, e.Exception.Message.ToString(), logPath + fileName);
        }
    }
}
