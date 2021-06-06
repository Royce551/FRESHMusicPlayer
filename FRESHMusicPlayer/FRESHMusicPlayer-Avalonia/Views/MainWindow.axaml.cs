using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.ViewModels;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FRESHMusicPlayer.Views
{
    public class MainWindow : Window
    {
        private MainWindowViewModel ViewModel { get => DataContext as MainWindowViewModel; }
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            DoStuff();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            
        }

        private void DoStuff()
        {
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            ViewModel?.CloseThings();
        }

        private void OpenTrackInfo(object sender, PointerPressedEventArgs e)    // HACK: THIS SHOULD NOT BE IN THE
        {                                                                       // CODE BEHIND!!!!
            if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
            new TrackInfo().SetStuff(ViewModel.Player).Show(this);
        }

        private void OnPlayButtonClick(object sender, RoutedEventArgs e)    // TODO: figure out why i need this stuff instead
        {                                                                   // of just using commands
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                ViewModel?.PlayCommand(x.Path);
            }
        }
        private void OnEnqueueButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                ViewModel?.EnqueueCommand(x.Path);
            }
        }
        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                ViewModel?.DeleteCommand(x.Path);
            }
        }
    }
}
