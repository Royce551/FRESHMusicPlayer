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
        private readonly NotificationHandler notificationHandler;
        public NotificationPage(NotificationHandler notificationHandler)
        {
            this.notificationHandler = notificationHandler;
            notificationHandler.NotificationInvalidate += InvalidateNotifications;
            InitializeComponent();
            ShowNotifications();
            KeepAlive = false;
        }

        private void InvalidateNotifications(object sender, EventArgs e) => ShowNotifications();
        private void ShowNotifications()
        {
            NotificationList.Items.Clear();
            foreach (Notification box in notificationHandler.Notifications)
            {
                box.Read = true;
                NotificationList.Items.Add(new NotificationBox(box, notificationHandler));
            }
            if (NotificationList.Items.Count == 0) (Application.Current.MainWindow as MainWindow)?.HideAuxilliaryPane();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            NotificationList.Items.Clear();
            notificationHandler.NotificationInvalidate -= InvalidateNotifications;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            notificationHandler.ClearAll();
        }
    }
}
