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
    public partial class NotificationPage : UserControl
    {
        private readonly MainWindow window;
        public NotificationPage(MainWindow window)
        {
            this.window = window;
            window.NotificationHandler.NotificationInvalidate += InvalidateNotifications;
            InitializeComponent();
            ShowNotifications();
        }

        private void InvalidateNotifications(object sender, EventArgs e) => ShowNotifications();
        private async void ShowNotifications()
        {
            NotificationList.Items.Clear();
            foreach (Notification box in window.NotificationHandler.Notifications)
            {
                box.Read = true;
                NotificationList.Items.Add(new NotificationBox(box, window.NotificationHandler));
            }
            if (NotificationList.Items.Count == 0) await window.HideAuxilliaryPane();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            NotificationList.Items.Clear();
            window.NotificationHandler.NotificationInvalidate -= InvalidateNotifications;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            window.NotificationHandler.ClearAll();
        }
    }
}
