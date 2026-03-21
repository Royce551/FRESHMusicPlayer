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
        if (e.Data.GetFiles() != null && viewModel != null)
        {
            viewModel.MainView.ShowDragDropOverlay = false;
            foreach (var item in e.Data.GetFiles()!)
            {
                await InterfaceUtils.DoDragDropAsync([.. e.Data.GetFiles()!.Select(x => x.Path.LocalPath)], viewModel.MainView.Player, viewModel.MainView.Library, import: false, clearqueue: false);
            }
        }
    }
}