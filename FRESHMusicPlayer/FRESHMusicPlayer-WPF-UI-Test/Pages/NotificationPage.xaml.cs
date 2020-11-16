using FRESHMusicPlayer.Handlers.Notifications;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FRESHMusicPlayer.Pages
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
            foreach (Notification box in MainWindow.NotificationHandler.Notifications)
            {
                box.Read = true;
                NotificationList.Items.Add(new NotificationBox(box));
            }
            if (NotificationList.Items.Count == 0) (Application.Current.MainWindow as MainWindow)?.HideAuxilliaryPane();
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
