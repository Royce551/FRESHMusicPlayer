using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    public struct NotificationInfo
    {
        public string HeaderText { get; set; }
        public string ContentText { get; set; }
        public bool IsImportant { get; set; }
        public bool DisplayAsToast { get; set; }
        public NotificationInfo(string HeaderText, string ContentText, bool IsImportant = false, bool DisplayAsToast = true)
        {
            this.HeaderText = HeaderText;
            this.ContentText = ContentText;
            this.IsImportant = IsImportant;
            this.DisplayAsToast = DisplayAsToast;
        }
    }
}
