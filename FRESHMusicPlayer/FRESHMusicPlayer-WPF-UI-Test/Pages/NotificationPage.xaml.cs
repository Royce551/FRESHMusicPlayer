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
        private readonly MainWindow window;
        public NotificationPage(MainWindow window)
        {
            this.window = window;
            window.NotificationHandler.NotificationInvalidate += InvalidateNotifications;
            InitializeComponent();
            ShowNotifications();
            KeepAlive = false;
        }

        private void InvalidateNotifications(object sender, EventArgs e) => ShowNotifications();
        private void ShowNotifications()
        {
            NotificationList.Items.Clear();
            foreach (Notification box in window.NotificationHandler.Notifications)
            {
                box.Read = true;
                NotificationList.Items.Add(new NotificationBox(box, window.NotificationHandler));
            }
            if (NotificationList.Items.Count == 0) (Application.Current.MainWindow as MainWindow)?.HideAuxilliaryPane();
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
