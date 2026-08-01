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

    private void Button_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => FocusSection(GeneralSection);

    private void Button_Click_1(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => FocusSection(AppearanceSection);

    private void Button_Click_2(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => FocusSection(MaintenanceSection);

    private void Button_Click_3(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => FocusSection(AboutSection);

    private void Button_Click_4(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => FocusSection(PlaybackSection);

    private bool inFocusMode = false;
    private bool ignoreScroll = false;
    private Control focusedControl = null;

    private void FocusSection(Control section)
    {
        ignoreScroll = true;
        section.BringIntoView();

        GeneralSection.Opacity = 0.5;
        AppearanceSection.Opacity = 0.5;
        MaintenanceSection.Opacity = 0.5;
        AboutSection.Opacity = 0.5;
        PlaybackSection.Opacity = 0.5;

        section.Opacity = 1;
        focusedControl = section;

        inFocusMode = true;
    }

    private void ResetFocus()
    {
        GeneralSection.Opacity = 1;
        AppearanceSection.Opacity = 1;
        MaintenanceSection.Opacity = 1;
        AboutSection.Opacity = 1;
        PlaybackSection.Opacity = 1;

        inFocusMode = false;
    }

    private void ScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (ignoreScroll)
        {
            ignoreScroll = false;
            return;
        }

        ResetFocus();
    }

    private void GeneralSection_GotFocus(object? sender, Avalonia.Input.FocusChangedEventArgs e)
    {
        if (inFocusMode && focusedControl != GeneralSection) ResetFocus();
    }
    private void PlaybackSection_GotFocus(object? sender, Avalonia.Input.FocusChangedEventArgs e)
    {
        if (inFocusMode && focusedControl != PlaybackSection) ResetFocus();
    }
    private void AppearanceSection_GotFocus(object? sender, Avalonia.Input.FocusChangedEventArgs e)
    {
        if (inFocusMode && focusedControl != AppearanceSection) ResetFocus();
    }
    private void MaintenanceSection_GotFocus(object? sender, Avalonia.Input.FocusChangedEventArgs e)
    {
        if (inFocusMode && focusedControl != MaintenanceSection) ResetFocus();
    }
    private void AboutSection_GotFocus(object? sender, Avalonia.Input.FocusChangedEventArgs e)
    {
        if (inFocusMode && focusedControl != AboutSection) ResetFocus();
    }
}