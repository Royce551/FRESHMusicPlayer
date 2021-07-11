using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    /// <summary>
    /// Handles FMP's notifications
    /// </summary>
    public class NotificationHandler
    {
        /// <summary>
        /// Gets the currently visible notifications for the window.
        /// </summary>
        public List<Notification> Notifications { get; private set; } = new List<Notification>();
        /// <summary>
        /// Raised whenever a notification is added, removed, or updated.
        /// </summary>
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
