using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    /// <summary>
    /// Interaction logic for NotificationBox.xaml
    /// </summary>
    public partial class NotificationBox : UserControl
    {
        public Notification Notification;

        private readonly NotificationHandler notificationHandler;
        public NotificationBox(Notification info, NotificationHandler notificationHandler)
        {
            this.notificationHandler = notificationHandler;
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

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => notificationHandler.Remove(Notification);

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Notification.OnButtonClicked?.Invoke() ?? true) notificationHandler.Remove(Notification);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => notificationHandler.Remove(Notification);

        private void UserControl_MouseEnter(object sender, MouseEventArgs e) => CloseButton.Opacity = 1;

        private void UserControl_MouseLeave(object sender, MouseEventArgs e) => CloseButton.Opacity = 0;

        private void UserControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => CloseButton.Opacity = 1;

        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e) => CloseButton.Opacity = 0;
    }
}
