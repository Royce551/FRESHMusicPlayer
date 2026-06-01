using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FRESHMusicPlayer.Views;

public partial class PlaylistManagementWindow : Window
{
    public PlaylistManagementWindow()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();
}