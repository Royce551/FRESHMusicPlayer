using System;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    public class Notification
    {
        public bool IsImportant { get; set; } = false;
        public bool DisplayAsToast { get; set; } = false;
        public bool Read { get; set; } = false;
        public string ContentText { get; set; } = string.Empty;
        public string ButtonText { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.Generic;
        public Func<bool> OnButtonClicked { get; set; } = null;
    }
    public enum NotificationType
    {
        Success,
        Information,
        Failure,
        Generic
    }
}
