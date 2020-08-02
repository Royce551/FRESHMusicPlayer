using System;
using System.Collections.Generic;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    public class NotificationHandler
    {
        public List<NotificationBox> Notifications = new List<NotificationBox>();
        public event EventHandler NotificationInvalidate;
        public NotificationHandler()
        {

        }
        public void Add(NotificationBox box)
        {
            Notifications.Add(box);
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
        public void Update(NotificationBox box, NotificationInfo info)
        {
            Notifications[Notifications.IndexOf(box)].UpdateContent(info);
            NotificationInvalidate?.Invoke(null, EventArgs.Empty);
        }
        public void Remove(NotificationBox box)
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
