using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.ViewModels;

namespace FRESHMusicPlayer.Views
{
    public partial class PlaylistManagement : Window
    {
        private PlaylistManagementViewModel ViewModel { get => DataContext as PlaylistManagementViewModel; }

        public PlaylistManagement()
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

        public PlaylistManagement SetStuff(MainWindowViewModel mainWindow, string track)
        {
            ViewModel.MainWindow = mainWindow;
            ViewModel.Track = track;
            ViewModel.Initialize();
            return this;
        }

        private void OnAddButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DisplayPlaylist x)
            {
                ViewModel?.AddToPlaylistCommand(x.Name);
            }
        }

        private void OnRemoveButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DisplayPlaylist x)
            {
                ViewModel?.RemoveFromPlaylistCommand(x.Name);
            }
        }

        private void OnAddThingButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            if (cmd.DataContext is DisplayPlaylist x)
            {
                ViewModel?.AddThingToPlaylistCommand(x.Name);
            }
        }

        private void OnMiscButtonClick(object sender, RoutedEventArgs e)
        {
            var cmd = (Button)sender;
            cmd.ContextMenu.Open();
        }

        private void OnRenameItemClick(object sender, RoutedEventArgs e)
        {
            var cmd = (MenuItem)sender;
            if (cmd.DataContext is DisplayPlaylist x)
            {
                ViewModel?.RenamePlaylistCommand(x.Name);
            }
        }
        private void OnDeleteItemClick(object sender, RoutedEventArgs e)
        {
            var cmd = (MenuItem)sender;
            if (cmd.DataContext is DisplayPlaylist x)
            {
                ViewModel?.DeletePlaylistCommand(x.Name);
            }
        }
        private void OnExportItemClick(object sender, RoutedEventArgs e)
        {
            var cmd = (MenuItem)sender;
            if (cmd.DataContext is DisplayPlaylist x)
            {
                ViewModel?.ExportPlaylistCommand(x.Name);
            }
        }

        private void OnOKButtonClick(object sender, RoutedEventArgs e) => Close();
    }
}
