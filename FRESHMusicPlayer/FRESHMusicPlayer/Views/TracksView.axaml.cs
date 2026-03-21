using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
using System.Diagnostics;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class TracksView : UserControl
{
    public TracksView()
    {
        InitializeComponent();
    }

    private TracksViewModel? viewModel => DataContext as TracksViewModel;

    private void ListBox_SelectionChanged(object? sender, Avalonia.Controls.SelectionChangedEventArgs e)
    {
     
    }

    private void DragEnter(object? sender, DragEventArgs e)
    {
        e.DragEffects = DragDropEffects.Copy;
    }

    private async void DragDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() != null && viewModel != null)
        {
            viewModel.MainView.ShowDragDropOverlay = false;
            await InterfaceUtils.DoDragDropAsync([.. e.Data.GetFiles()!.Select(x => x.Path.LocalPath)], viewModel.MainView.Player, viewModel.MainView.Library);
        }
    }
}