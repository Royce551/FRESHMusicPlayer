using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    public struct NotificationInfo // TODO: investigate whether this can be removed
    {
        public string HeaderText { get; set; }
        public string ContentText { get; set; }
        public bool IsImportant { get; set; }
        public bool DisplayAsToast { get; set; }
        public NotificationType NotificationType { get; set; }
        public NotificationInfo(string HeaderText, string ContentText, bool IsImportant = false, bool DisplayAsToast = true, NotificationType NotificationType = NotificationType.Generic)
        {
            this.HeaderText = HeaderText;
            this.ContentText = ContentText;
            this.IsImportant = IsImportant;
            this.DisplayAsToast = DisplayAsToast;
            this.NotificationType = NotificationType;
        }
    }
}
