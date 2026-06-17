using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Xaml.Interactivity;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class PlaylistsView : UserControl
{
    public PlaylistsView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
        MainListBox.SelectedItem = null;
    }

    private PlaylistsViewModel? viewModel => DataContext as PlaylistsViewModel;

    private void DragEnter(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private async void DragDrop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.TryGetFiles() != null && viewModel != null)
        {
            viewModel.MainView.ShowDragDropOverlay = false;
            if (viewModel.SelectedPlaylist is null) return;
            foreach (var item in e.DataTransfer.TryGetFiles()!)
            {
                await viewModel.MainView.Library.AddTrackToPlaylistAsync(viewModel.SelectedPlaylist.Name, item.Path.LocalPath);
            }
        }
    }

    private void Grid_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Grid grid && grid.DataContext is DatabaseTrackViewModel trackViewModel)
        {
            if (e.ClickCount >= 2) trackViewModel.Play();
        }
    }

    private void ListBox_KeyDown_1(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (sender is ListBox listBox && listBox.SelectedItem is DatabaseTrackViewModel track)
        {
            if (e.Key == Key.Enter) track.Play();

        }
    }
}