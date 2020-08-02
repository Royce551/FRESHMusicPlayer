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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationBox.xaml
    /// </summary>
    public partial class NotificationBox : UserControl
    {
        public bool IsImportant = false;
        public bool DisplayAsToast = false;

        public string HeaderText;
        public string ContentText;
        public NotificationBox(NotificationInfo info)
        {
            InitializeComponent();
            UpdateContent(info);
        }
        public void UpdateContent(NotificationInfo info)
        {
            HeaderLabel.Text = info.HeaderText;
            ContentLabel.Text = info.ContentText;
            this.IsImportant = info.IsImportant;
            this.DisplayAsToast = info.DisplayAsToast;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MainWindow.NotificationHandler.Remove(this);
    }
}
