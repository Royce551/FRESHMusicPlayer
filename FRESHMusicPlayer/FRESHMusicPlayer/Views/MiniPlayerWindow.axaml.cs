using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views;

public partial class MiniPlayerWindow : Window
{
    public MiniPlayerWindow()
    {
        InitializeComponent();
    }

    private void Window_Closed(object? sender, System.EventArgs e)
    {
        (DataContext as MiniPlayerViewModel)?.MainViewModel.MainWindow.Show();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => Close();

    private void Window_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}