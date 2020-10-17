using System;
using System.Collections.Generic;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    public class NotificationHandler
    {
        public List<Notification> Notifications = new List<Notification>();
        public event EventHandler NotificationInvalidate;

        public void Add(Notification box)
        {
            Notifications.Add(box);
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
        public void Update(Notification box)
        {
            int notificationindex = Notifications.IndexOf(box);
            if (Notifications[notificationindex] != null)
            {
                Notifications[notificationindex] = box;
            }
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
        public void Remove(Notification box)
        {
            Notifications.Remove(box);
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
        public void ClearAll()
        {
            Notifications.Clear();
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
        
    }
}
