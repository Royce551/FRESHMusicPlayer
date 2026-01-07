using Avalonia.Animation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using FRESHMusicPlayer.ViewModels;
using System.Threading.Tasks;
using System.Diagnostics;
using Avalonia.Media;
using Avalonia.Animation.Easings;
using System;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using System.IO;

namespace FRESHMusicPlayer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        ProgressSlider.AddHandler(PointerPressedEvent, ProgressSlider_PointerPressed, Avalonia.Interactivity.RoutingStrategies.Tunnel);
        ProgressSlider.AddHandler(PointerReleasedEvent, ProgressSlider_PointerReleased, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    public bool ProgressSliderIsAnimating => ProgressSlider.IsAnimating(RangeBase.ValueProperty);

    private MainViewModel viewModel => DataContext as MainViewModel;

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    public void UpdateIconStates()
    {
        if (viewModel.Player.Paused)
        {
            if (TryGetResource("PlayIcon", null, out object? icon)) PlayPauseIcon.Data = (Geometry)icon;

        }
        else
        {
            if (TryGetResource("PauseIcon", null, out object? icon)) PlayPauseIcon.Data = (Geometry)icon;
        }

        if (viewModel.Player.Queue.Shuffle)
        {
            if (Application.Current.TryFindResource("AccentGradientColor", ActualThemeVariant, out object? brush))
                ShuffleIcon.Foreground = (Brush)brush;
        }
        else
        {
            if (Application.Current.TryFindResource("PrimaryTextColor", ActualThemeVariant, out object? brush))
                ShuffleIcon.Foreground = (Brush)brush;
        }

        if (viewModel.Player.Queue.RepeatMode == RepeatMode.None)
        {
            if (TryGetResource("RepeatIcon", null, out object? icon)) RepeatIcon.Data = (Geometry)icon;
            if (Application.Current.TryFindResource("PrimaryTextColor", ActualThemeVariant, out object? brush))
                RepeatIcon.Foreground = (Brush)brush;
        }
        else if (viewModel.Player.Queue.RepeatMode == RepeatMode.RepeatAll)
        {
            if (TryGetResource("RepeatIcon", null, out object? icon)) RepeatIcon.Data = (Geometry)icon;
            if (Application.Current.TryFindResource("AccentGradientColor", ActualThemeVariant, out object? brush))
                RepeatIcon.Foreground = (Brush)brush;
        }
        else
        {
            if (TryGetResource("RepeatOneIcon", null, out object? icon)) RepeatIcon.Data = (Geometry)icon;
            if (Application.Current.TryFindResource("AccentGradientColor", ActualThemeVariant, out object? brush))
                RepeatIcon.Foreground = (Brush)brush;
        }
    }

    string previousAnimation;
    public async Task AnimateSidePaneInAsync(double width, bool onLeft = false)
    {
        // TODO: Adjust animation based on width
        SidePaneControl.Width = width;
        Animation animation;
        if (onLeft)
        {
            DockPanel.SetDock(SidePaneControl, Dock.Left);
            animation = (Animation)Resources["LeftSidePaneIn250"]!;
            previousAnimation = "LeftSidePaneIn250";
        }
        else
        {
            DockPanel.SetDock(SidePaneControl, Dock.Right);
            if (width == 450)
            {
                animation = (Animation)Resources["RightSidePaneIn450"]!;
                previousAnimation = "RightSidePaneIn450";
            }
            else
            {
                animation = (Animation)Resources["RightSidePaneIn300"]!;
                previousAnimation = "RightSidePaneIn300";
            }
        }
        await animation.RunAsync(SidePaneControl);
    }

    public async Task AnimateSidePaneOutAsync()
    {
        Animation animation;
        switch (previousAnimation)
        {
            case "LeftSidePaneIn250":
                animation = (Animation)Resources["LeftSidePaneOut250"]!;
                break;
            case "RightSidePaneIn450":
                animation = (Animation)Resources["RightSidePaneOut450"]!;
                break;
            case "RightSidePaneIn300":
            default:
                animation = (Animation)Resources["RightSidePaneOut300"]!;
                break;
        }
        await animation.RunAsync(SidePaneControl);
    }

    public async Task AnimateCoverArtShowAsync()
    {
        var animation = (Animation)Resources["ShowCoverArt"]!;
        await animation.RunAsync(CoverArt);
    }

    public async Task AnimateCoverArtHideAsync()
    {
        var animation = (Animation)Resources["HideCoverArt"]!;
        await animation.RunAsync(CoverArt);
    }

    public async Task AnimateProgressTo0Async()
    {
        var animation = (Animation)Resources["SetProgressTo0"]!;
        await animation.RunAsync(ProgressSlider);
    }

    private void Window_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        if (e.Source is Control control)
        {
            if (control.FindLogicalAncestorOfType<ListBox>(true) != null)
                return;
        }


        VolumeSlider.Value += ((e.Delta.Y / 100) * 3);
    }

    private void Window_Deactivated(object? sender, System.EventArgs e)
    {
        viewModel.ProgressTimer.Interval = TimeSpan.FromMilliseconds(1000);
    }

    private void Window_Activated_1(object? sender, System.EventArgs e)
    {
        viewModel.ProgressTimer.Interval = TimeSpan.FromMilliseconds(100);
    }

    private void ProgressSlider_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        viewModel.IsDragging = true;
    }

    private void ProgressSlider_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        viewModel.IsDragging = false;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        viewModel.Config.Save(Path.Combine(App.DataFolderLocation, "Configuration"));
        viewModel.Library.Database?.Dispose();
    }

    private void OverflowMenuButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        OverflowMenuButton.ContextMenu?.Open();
    }

    private void TextBlock_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        viewModel.OpenTrackInfoCommand();
    }

    private void CoverArt_PointerReleased(object? sender, Avalonia.Input.PointerReleasedEventArgs e)
    {
        viewModel.OpenTrackInfoCommand();
    }
}
