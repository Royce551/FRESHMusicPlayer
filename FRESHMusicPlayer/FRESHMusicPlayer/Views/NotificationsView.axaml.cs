using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views;

public partial class NotificationsView : UserControl
{
    public NotificationsView()
    {
        InitializeComponent();
    }

    private NotificationsViewModel viewModel => DataContext as NotificationsViewModel;

    private void Border_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        viewModel.MainView.Notifications.Remove(((sender as Border).DataContext as Handlers.Notification));
    }
}