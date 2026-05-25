using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Linq;
using FRESHMusicPlayer.ViewModels;

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

    private SearchViewModel viewModel => DataContext as SearchViewModel;

    private async void SearchBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            var track = await viewModel?.Tracks;
            track.FirstOrDefault()?.Play();
        }
    }
}