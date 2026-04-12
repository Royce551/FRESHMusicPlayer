using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace FRESHMusicPlayer.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => GeneralSectionHeader.BringIntoView();

    private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => AppearanceSectionHeader.BringIntoView();

    private void Button_Click_2(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => MaintenanceSectionHeader.BringIntoView();

    private void Button_Click_3(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => AboutSectionHeader.BringIntoView();
}