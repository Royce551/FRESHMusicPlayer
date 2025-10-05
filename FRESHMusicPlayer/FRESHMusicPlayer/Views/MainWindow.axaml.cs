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

namespace FRESHMusicPlayer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
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

    public async Task AnimateSidePaneInAsync(double width)
    {
        var animation = (Animation)Resources["RightSidePaneIn450"]!;
        await animation.RunAsync(SidePaneControl);
    }

    public async Task AnimateSidePaneOutAsync()
    {
        var animation = (Animation)Resources["RightSidePaneOut450"]!;
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
}
