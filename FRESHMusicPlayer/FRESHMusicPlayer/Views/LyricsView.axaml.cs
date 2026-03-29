using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class LyricsView : UserControl
{
    public LyricsView()
    {
        InitializeComponent();
        LyricsScrollViewer.AddHandler(PointerWheelChangedEvent, LyricsScrollViewer_PointerWheelChanged, Avalonia.Interactivity.RoutingStrategies.Tunnel);
    }

    public void ScrollToCenter(List<LyricLineViewModel> currentLines)
    {
        List<Control> lyricLineControls = [.. currentLines.Select(LyricsItemsControl.ContainerFromItem)];

        double offset = 0;

        foreach (var control in lyricLineControls) offset += control.Bounds.Height;

        offset -= lyricLineControls.Last().Bounds.Height;

        offset -= LyricsScrollViewer.Viewport.Height / 2;

        LyricsScrollViewer.Offset = new Vector(0, offset);
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (DataContext as LyricsViewModel)!.View = this;
    }


    private void LyricsScrollViewer_PointerWheelChanged(object? sender, Avalonia.Input.PointerWheelEventArgs e)
    {
        (DataContext as LyricsViewModel)!.AutoScrollEnabled = false;
    }
}