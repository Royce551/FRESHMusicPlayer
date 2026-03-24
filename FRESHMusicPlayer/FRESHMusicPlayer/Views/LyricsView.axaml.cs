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
    }

    public void ScrollToCenter(List<LyricLineViewModel> currentLines)
    {
        if (currentLines.Count == 0) return;

        List<Control> lyricLineControls = currentLines.Select(LyricsItemsControl.ContainerFromItem).ToList();

        double offset = 0;

        foreach (var control in lyricLineControls) offset += control.Bounds.Height;

        offset -= lyricLineControls.Last().Bounds.Height;

        offset -= LyricsScrollViewer.Viewport.Height / 2;

        LyricsScrollViewer.Offset = new Vector(0, offset);
    }

    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        (DataContext as LyricsViewModel).View = this;
    }
}