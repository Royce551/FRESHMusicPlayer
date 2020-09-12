using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    public class Notification
    {
        public bool IsImportant { get; set; } = false;
        public bool DisplayAsToast { get; set; } = false;
        public bool Read { get; set; } = false;
        public string HeaderText { get; set; }
        public string ContentText { get; set; }
        public NotificationType Type { get; set; } = NotificationType.Generic;
    }
    public enum NotificationType
    {
        Success,
        Information,
        Failure,
        Generic
    }
}
