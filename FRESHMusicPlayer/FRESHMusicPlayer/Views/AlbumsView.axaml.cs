using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class AlbumsView : UserControl
{
    public AlbumsView()
    {
        InitializeComponent();
    }

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {

    }

    private AlbumsViewModel? viewModel => DataContext as AlbumsViewModel;

    private void DragEnter(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private async void DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() != null && viewModel != null)
        {
            foreach (var item in e.Data.GetFiles()!)
            {
                await InterfaceUtils.DoDragDropAsync([.. e.Data.GetFiles()!.Select(x => x.Path.LocalPath)], viewModel.MainView.Player, viewModel.MainView.Library);
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