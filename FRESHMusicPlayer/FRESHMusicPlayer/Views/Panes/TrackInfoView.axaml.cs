using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform.Storage;
using FRESHMusicPlayer.ViewModels;
using System.IO;

namespace FRESHMusicPlayer.Views;

public partial class TrackInfoView : UserControl
{
    public TrackInfoView()
    {
        InitializeComponent();
    }

    private void UserControl_PointerPressed(object? sender, Avalonia.Input.PointerPressedEventArgs e)
    {
        var point = e.GetCurrentPoint(sender as Control);
        if (point.Properties.IsRightButtonPressed)
        {
            var path = Path.Combine(Path.GetTempPath(), "cover.png");
            (DataContext as TrackInfoViewModel).CoverArt.Save(path);
            TopLevel.GetTopLevel(this).Launcher.LaunchFileInfoAsync(new FileInfo(path));
        }
    }
}