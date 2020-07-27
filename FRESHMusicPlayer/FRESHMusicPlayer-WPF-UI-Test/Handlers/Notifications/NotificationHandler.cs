using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;

namespace FRESHMusicPlayer_WPF_UI_Test.Handlers.Notifications
{
    public class NotificationHandler
    {
        public List<NotificationBox> Notifications = new List<NotificationBox>();
        public event EventHandler NotificationInvalidate;
        public NotificationHandler()
        {

        }
        public void AddNotification(string HeaderText, string ContentText, bool IsImportant = false, bool DisplayAsToast = true)
        {
            Notifications.Add(new NotificationBox(HeaderText, ContentText, IsImportant, DisplayAsToast));
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
        public void ClearAllNotifications()
        {
            Notifications.Clear();
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
    }
}
