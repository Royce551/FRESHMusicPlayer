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
            StackPanel.Children.Clear();
            foreach (NotificationBox box in MainWindow.NotificationHandler.Notifications) StackPanel.Children.Add(box);
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            StackPanel.Children.Clear();
            MainWindow.NotificationHandler.NotificationInvalidate -= InvalidateNotifications;
        }
    }
}
