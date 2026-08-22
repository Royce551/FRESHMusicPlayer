using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Avalonia.VisualTree;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FRESHMusicPlayer.Views;

public partial class FullscreenView : UserControl
{
    private FullscreenViewModel viewModel;
    private DispatcherTimer controlDismissTimer;

    public FullscreenView()
    {
        InitializeComponent();
        controlDismissTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1),
            IsEnabled = true
        };
        controlDismissTimer.Tick += ControlDismissTimer_Tick;
    }

    public void ScrollToCenter(List<LyricLineViewModel> currentLines)
    {
        var LyricsItemsControl = this.FindDescendantOfType<ItemsControl>();
        var LyricsScrollViewer = this.FindDescendantOfType<ScrollViewer>();

        if (LyricsItemsControl is null || LyricsScrollViewer is null) return;

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

    private WindowState previousWindowState;
    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        viewModel = DataContext as FullscreenViewModel ?? throw new Exception();
        viewModel.View = this;

        viewModel.MainView.IsContentFullscreen = true;
        previousWindowState = viewModel.MainView.MainWindow.WindowState;
        viewModel.MainView.MainWindow.WindowState = WindowState.FullScreen;
        _ = viewModel.MainView.CloseSidePaneAsync();
    }

    public void LeaveFullscreen()
    {
        controlDismissTimer.Tick -= ControlDismissTimer_Tick;
        viewModel.MainView.SetControlsVisibility(true);
        viewModel.MainView.IsContentFullscreen = false;
        viewModel.MainView.MainWindow.WindowState = previousWindowState;
    }

    private Point? lastMouseMovePosition = null;
    private bool isMouseMoving = false;
    private const int minimumMouseMovementThreshold = 1;
    private void UserControl_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (lastMouseMovePosition != null)
        {
            var position = e.GetPosition(null);
            if (Math.Abs(position.X - lastMouseMovePosition.Value.X) > minimumMouseMovementThreshold ||
                Math.Abs(position.Y - lastMouseMovePosition.Value.Y) > minimumMouseMovementThreshold)
            {
                if (!controlDismissTimer.IsEnabled && viewModel.MainView.AreControlsHidden)
                {
                    viewModel.MainView.SetControlsVisibility(true);
                    controlDismissTimer.Interval = TimeSpan.FromSeconds(3);
                    controlDismissTimer.Start();
                    Cursor = new(StandardCursorType.Arrow);
                    ControlBar.IsVisible = true;
                }
                isMouseMoving = true;
            }
            else
            {
                controlDismissTimer.Interval = TimeSpan.FromMilliseconds(1000);
                isMouseMoving = false;
            }
        }
        lastMouseMovePosition = e.GetPosition(null);
    }

    private void ControlDismissTimer_Tick(object? sender, EventArgs e)
    {
        if ((IsPointerOver && isMouseMoving) || (!IsPointerOver && viewModel.MainView.MainWindow.IsPointerOver) || viewModel.MainView.AreControlsHidden)
        {
            controlDismissTimer.Interval = TimeSpan.FromMilliseconds(100);
            return;
        }

        controlDismissTimer.Stop();
        viewModel.MainView.SetControlsVisibility(false);
        Cursor = new(StandardCursorType.None);
        _ = viewModel.MainView.CloseSidePaneAsync();
        ControlBar.IsVisible = false;
    }
}