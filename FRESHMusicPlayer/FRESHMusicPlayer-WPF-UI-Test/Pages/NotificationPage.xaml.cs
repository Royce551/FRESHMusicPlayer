using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FRESHMusicPlayer.Handlers;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer;
using System.Windows.Media.Animation;

namespace FRESHMusicPlayer_WPF_UI_Test.Pages
{
    /// <summary>
    /// Interaction logic for NotificationPage.xaml
    /// </summary>
    public partial class NotificationPage : Page
    {
        public NotificationPage()
        {
            MainWindow.NotificationHandler.NotificationInvalidate += InvalidateNotifications;
            InitializeComponent();
            ShowNotifications();
            KeepAlive = false;
        }

        private void InvalidateNotifications(object sender, EventArgs e) => ShowNotifications();
        private void ShowNotifications()
        {
            NotificationList.Items.Clear();
            foreach (NotificationBox box in MainWindow.NotificationHandler.Notifications)
            {
                NotificationList.Items.Add(box);
                box.Read = true;
            }
                
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            NotificationList.Items.Clear();
            MainWindow.NotificationHandler.NotificationInvalidate -= InvalidateNotifications;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainWindow.NotificationHandler.ClearAll();
        }
    }
}
