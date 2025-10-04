using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
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

    private ViewModelBase? viewModel => DataContext as ViewModelBase;

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
}