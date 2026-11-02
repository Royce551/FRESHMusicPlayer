using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
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
            Interval = TimeSpan.FromSeconds(1)
        };
        controlDismissTimer.Tick += ControlDismissTimer_Tick;
        Unloaded += (_, _) => LeaveFullscreen();
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
    private bool isFullscreenActive;
    private void UserControl_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (isFullscreenActive) return;

        viewModel = DataContext as FullscreenViewModel ?? throw new Exception();
        viewModel.View = this;
        isFullscreenActive = true;
        lastMouseMovePosition = null;

        viewModel.MainView.IsContentFullscreen = true;
        previousWindowState = viewModel.MainView.MainWindow.WindowState;
        viewModel.MainView.MainWindow.WindowState = WindowState.FullScreen;
        _ = viewModel.MainView.CloseSidePaneAsync();

        var window = viewModel.MainView.MainWindow;
        window.AddHandler(PointerMovedEvent, UserControl_PointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        window.AddHandler(PointerPressedEvent, Window_Activity, RoutingStrategies.Tunnel, handledEventsToo: true);
        window.AddHandler(KeyDownEvent, Window_Activity, RoutingStrategies.Tunnel, handledEventsToo: true);
        ShowControls();
    }

    public void LeaveFullscreen()
    {
        if (!isFullscreenActive) return;
        isFullscreenActive = false;
        controlDismissTimer.Stop();

        var window = viewModel.MainView.MainWindow;
        window.RemoveHandler(PointerMovedEvent, UserControl_PointerMoved);
        window.RemoveHandler(PointerPressedEvent, Window_Activity);
        window.RemoveHandler(KeyDownEvent, Window_Activity);
        Cursor = null;
        viewModel.MainView.SetControlsVisibility(true, animate: false);
        viewModel.MainView.IsContentFullscreen = false;
        viewModel.MainView.MainWindow.WindowState = previousWindowState;
    }

    private Point? lastMouseMovePosition = null;
    private const int minimumMouseMovementThreshold = 1;
    private void UserControl_PointerMoved(object? sender, Avalonia.Input.PointerEventArgs e)
    {
        if (!isFullscreenActive) return;

        var position = e.GetPosition(viewModel.MainView.MainWindow);
        if (lastMouseMovePosition is { } previous &&
            Math.Abs(position.X - previous.X) < minimumMouseMovementThreshold &&
            Math.Abs(position.Y - previous.Y) < minimumMouseMovementThreshold)
            return;

        lastMouseMovePosition = position;
        ShowControls();
    }

    private void Window_Activity(object? sender, RoutedEventArgs e) => ShowControls();

    private void ShowControls()
    {
        if (!isFullscreenActive) return;

        viewModel.MainView.SetControlsVisibility(true);
        Cursor = null;
        ControlBar.IsVisible = true;
        controlDismissTimer.Stop();
        controlDismissTimer.Start();
    }

    private void ControlDismissTimer_Tick(object? sender, EventArgs e)
    {
        controlDismissTimer.Stop();
        if (!isFullscreenActive) return;

        if (ControlBar.IsPointerOver || viewModel.MainView.IsDragging || (!IsPointerOver && viewModel.MainView.MainWindow.IsPointerOver))
        {
            controlDismissTimer.Start();
            return;
        }

        viewModel.MainView.SetControlsVisibility(false);
        Cursor = new(StandardCursorType.None);
        _ = viewModel.MainView.CloseSidePaneAsync();
        ControlBar.IsVisible = false;
    }
}
