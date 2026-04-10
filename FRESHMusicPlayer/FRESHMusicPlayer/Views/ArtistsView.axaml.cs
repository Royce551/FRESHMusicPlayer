using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class ArtistsView : UserControl
{
    public ArtistsView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {

    }

    private ArtistsViewModel? viewModel => DataContext as ArtistsViewModel;

    private void DragEnter(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private async void DragDrop(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.TryGetFiles() != null && viewModel != null)
        {
            viewModel.MainView.ShowDragDropOverlay = false;
            foreach (var item in e.DataTransfer.TryGetFiles()!)
            {
                await InterfaceUtils.DoDragDropAsync([.. e.DataTransfer.TryGetFiles()!.Select(x => x.Path.LocalPath)], viewModel.MainView.Player, viewModel.MainView.Library);
            }
        }
    }

    private void Border_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is Border border && border.DataContext is AlbumGroupHeaderViewModel albumViewModel)
        {
            viewModel?.MainView.NavigateTo(new AlbumsViewModel(albumViewModel.Album));
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