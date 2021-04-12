using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer_Avalonia.ViewModels;

namespace FRESHMusicPlayer_Avalonia.Views
{
    public class MainWindow : Window
    {
        private MainWindowViewModel? viewModel { get => DataContext as MainWindowViewModel; }
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        private void OnPlayButtonClick(object sender, RoutedEventArgs e)    // TODO: figure out why i need this stuff instead
        {                                                                   // of just using commands
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                viewModel?.PlayCommand(x.Path);
            }
        }
        private void OnEnqueueButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                viewModel?.EnqueueCommand(x.Path);
            }
        }
        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DatabaseTrack x)
            {
                viewModel?.DeleteCommand(x.Path);
            }
        }
    }
}
