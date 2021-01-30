using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly string logPath;
        private readonly string fileName;

        private int timesSadFaceClicked = 1;
        public CriticalErrorBox(DispatcherUnhandledExceptionEventArgs e, string logPath, string fileName)
        {
            InitializeComponent();
            this.logPath = logPath;
            this.fileName = fileName;
            ContentTextBlock.Text = string.Format(Properties.Resources.APPLICATION_CRITICALERROR, e.Exception.Message.ToString(), logPath + fileName);
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) => Close();
        private void OpenDebugLogButton_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(logPath);
            Process.Start(logPath + fileName);
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            timesSadFaceClicked++;
            if (timesSadFaceClicked > 5) Process.Start(@"https://www.youtube.com/watch?v=PJph1bc1HNo");
        }
    }
}
