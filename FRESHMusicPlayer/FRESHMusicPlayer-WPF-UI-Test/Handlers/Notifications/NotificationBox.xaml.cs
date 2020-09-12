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
            HeaderLabel.Text = Notification.HeaderText;
            ContentLabel.Text = Notification.ContentText;
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => MainWindow.NotificationHandler.Remove(Notification);

    }
}
