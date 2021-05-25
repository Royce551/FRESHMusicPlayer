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
            // TODO: reimplement as notification
            //if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            //{
            //    new MessageBox().SetStuff("Did you download the wrong thing?",
            //        $"This is FRESHMusicPlayer for Mac and Linux. {Environment.NewLine}" +
            //        "Although you're free to keep using this version (we won't bother you again), " +
            //        "you'll get a better experience if you grab the Windows version from" +
            //        "https://github.com/royce551/freshmusicplayer/releases/latest. " +
            //        "If there's something you think was done better here, let us know in the issue tracker!").ShowDialog(this);
            //}
        }

        private void OnClosing(object sender, CancelEventArgs e)
        {
            ViewModel?.CloseThings();
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
