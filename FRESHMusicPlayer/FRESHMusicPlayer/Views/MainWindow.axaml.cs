using Avalonia.Animation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Platform;
using FRESHMusicPlayer.ViewModels;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
    }

    public async Task AnimateSidePaneInAsync(double width)
    {
        var animation = (Animation)Resources["RightSidePaneIn450"];
        await animation.RunAsync(SidePaneControl);
    }

    public async Task AnimateSidePaneOutAsync()
    {
        var animation = (Animation)Resources["RightSidePaneOut450"];
        await animation.RunAsync(SidePaneControl);
    }
}
