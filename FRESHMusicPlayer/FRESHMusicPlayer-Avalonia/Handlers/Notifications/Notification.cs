using System;

namespace FRESHMusicPlayer.Handlers.Notifications
{
    public class Notification
    {
        /// <summary>
        /// Whether the notification should appear on top of other notifications
        /// </summary>
        public bool IsImportant { get; set; } = false;
        /// <summary>
        /// Whether the notification should forcefully open the Notification pane
        /// </summary>
        public bool DisplayAsToast { get; set; } = false;
        /// <summary>
        /// Whether the user has seen the notification
        /// </summary>
        public bool Read { get; set; } = false;
        /// <summary>
        /// The actual content of the notification
        /// </summary>
        public string ContentText { get; set; } = string.Empty;
        /// <summary>
        /// The text to show in the button
        /// </summary>
        public string ButtonText { get; set; } = string.Empty;
        /// <summary>
        /// The type of notification
        /// </summary>
        public NotificationType Type { get; set; } = NotificationType.Generic;
        /// <summary>
        /// Invoked whenever the notification's button has been clicked. Return true to close the notification.
        /// </summary>
        public Func<bool> OnButtonClicked { get; set; } = null;

        public bool ShouldButtonBeVisible => !string.IsNullOrEmpty(ButtonText) && OnButtonClicked is not null;
    }
    public enum NotificationType
    {
        /// <summary>
        /// The task the notification is associated with succeeded - green border
        /// </summary>
        Success,
        /// <summary>
        /// The notification displays information - no border
        /// </summary>
        Information,
        /// <summary>
        /// The task the notification is associated with failed - red border
        /// </summary>
        Failure,
        /// <summary>
        /// A generic notification
        /// </summary>
        Generic
    }
}
