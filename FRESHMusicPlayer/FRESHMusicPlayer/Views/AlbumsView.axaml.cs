using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FRESHMusicPlayer.Views;

public partial class AlbumsView : UserControl
{
    public AlbumsView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        MainListBox.SelectedItem = null;
    }
}