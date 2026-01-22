using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace FRESHMusicPlayer.Views;

public partial class SearchView : UserControl
{
    public SearchView()
    {
        InitializeComponent();
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        SearchBox.Focus();
    }
}