using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers
{
    public partial class Notification : ObservableObject
    {
        [ObservableProperty]
        private string contentText = string.Empty;

        public string ButtonText { get; set; } = string.Empty;

        public bool HasBeenRead { get; set; } = false;

        public NotificationType Type { get; set; } = NotificationType.Generic;

        public Func<bool>? OnButtonClicked { get; set; } = null;

        public string? StatusBarText { get; set; } = null;

        public bool DisplayAsToast { get; set; } = false;

        public TimeSpan? ToastDisplayTime { get; set; } = null;

        private readonly MainViewModel viewModel;
        public Notification(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public bool IsNotificationButtonVisible => OnButtonClicked != null;

        public void NotificationButton()
        {
            if (OnButtonClicked?.Invoke() ?? true) viewModel.Notifications.Remove(this);
        }

        public Brush? BorderColor => Type switch
        {
            NotificationType.Success => new SolidColorBrush(Color.FromRgb(105, 181, 120)),
            NotificationType.Failure => new SolidColorBrush(Color.FromRgb(213, 70, 53)),
            _ => (Brush)Application.Current.FindResource(viewModel.MainWindow.ActualThemeVariant, "SecondaryTextColor"),
        };
    }

    public enum NotificationType
    {
        Success,
        Information,
        Failure,
        Progress,
        Generic
    }
}
