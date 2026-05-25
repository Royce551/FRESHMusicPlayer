using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class QueueView : UserControl
{
    public QueueView()
    {
        InitializeComponent();
    }

    private QueueViewModel? viewModel => DataContext as QueueViewModel;

    private async void UserControl_Drop(object? sender, Avalonia.Input.DragEventArgs e)
    {
        if (e.DataTransfer.TryGetFiles() != null && viewModel != null)
        {
            viewModel.MainView.ShowDragDropOverlay = false;
            foreach (var item in e.DataTransfer.TryGetFiles()!)
            {
                await InterfaceUtils.DoDragDropAsync([.. e.DataTransfer.TryGetFiles()!.Select(x => x.Path.LocalPath)], viewModel.MainView.Player, viewModel.MainView.Library, import: false, clearqueue: false);
            }
        }
    }
}