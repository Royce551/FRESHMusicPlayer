using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Runtime.Remoting;
using Windows.Media.ClosedCaptioning;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationBox.xaml
    /// </summary>
    public partial class NotificationBox : UserControl
    {
        public Notification Notification;
        public NotificationBox(Notification info)
        {
            InitializeComponent();
            UpdateContent(info);
        }
        public void UpdateContent(Notification info)
        {
            Notification = info;
            switch (Notification.Type)
            {
                case NotificationType.Success:
                    Border.BorderBrush = new SolidColorBrush(Color.FromRgb(105, 181, 120));
                    break;
                case NotificationType.Failure:
                    Border.BorderBrush = new SolidColorBrush(Color.FromRgb(213, 70, 53));
                    break;
            }
            ContentLabel.Text = Notification.ContentText;
            if (!string.IsNullOrEmpty(Notification.ButtonText) && Notification.OnButtonClicked != null)
            {
                Button.Content = Notification.ButtonText;
                Button.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MainWindow.NotificationHandler.Remove(Notification);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Notification.OnButtonClicked?.Invoke() ?? true) MainWindow.NotificationHandler.Remove(Notification);
        }
    }
}
