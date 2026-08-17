using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using FRESHMusicPlayer.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class FullscreenView : UserControl
{
    public FullscreenView()
    {
        InitializeComponent();
    }



    public void ScrollToCenter(List<LyricLineViewModel> currentLines)
    {
        var LyricsItemsControl = this.FindDescendantOfType<ItemsControl>();
        var LyricsScrollViewer = this.FindDescendantOfType<ScrollViewer>();

        List<Control> lyricLineControls = [.. currentLines.Select(LyricsItemsControl.ContainerFromItem)];

        double offset = 0;

        foreach (var control in lyricLineControls)
        {
            if (control is null) return;
            offset += control.Bounds.Height + control.Margin.Top + control.Margin.Bottom;
        }
        var last = lyricLineControls.Last();
        offset -= last.Bounds.Height + last.Margin.Top + last.Margin.Bottom;
        offset -= LyricsScrollViewer.Viewport.Height / 2;
        offset += 150;

        LyricsScrollViewer.Offset = new Vector(0, offset);
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (DataContext as FullscreenViewModel)?.View = this;
    }
}