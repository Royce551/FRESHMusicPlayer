using FRESHMusicPlayer.Forms.Playlists;
using FRESHMusicPlayer.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

namespace FRESHMusicPlayer.Pages.Library
{
    /// <summary>
    /// Interaction logic for SongEntry.xaml
    /// </summary>
    public partial class SongEntry : UserControl
    {
        public string FilePath;
        public string Title;
        public SongEntry(string filePath, string artist, string album, string title)
        {
            InitializeComponent();
            FilePath = filePath;
            ArtistAlbumLabel.Text = $"{artist} ・ {album}";
            TitleLabel.Text = title;
            Title = title;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = QueueButton.Visibility = DeleteButton.Visibility = PlayHitbox.Visibility = QueueHitbox.Visibility = DeleteHitbox.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            PlayButton.Visibility = QueueButton.Visibility = DeleteButton.Visibility = PlayHitbox.Visibility = QueueHitbox.Visibility = DeleteHitbox.Visibility = Visibility.Collapsed;
        }

        private void PlayButtonClick(object sender, MouseButtonEventArgs e)
        {
            if (FilePath.StartsWith("http") || File.Exists(FilePath))
            {
                if (MainWindow.Player.Playing) MainWindow.Player.ClearQueue();
                MainWindow.Player.AddQueue(FilePath);
                MainWindow.Player.PlayMusic();
            }
            else
            {
                MainWindow.NotificationHandler.Add(new Handlers.Notifications.Notification
                {
                    ContentText = string.Format(Properties.Resources.NOTIFICATION_FILEGONE, FilePath),
                    ButtonText = "Remove from library",
                    OnButtonClicked = () =>
                    {
                        DatabaseUtils.Remove(FilePath);
                        ((ListBox)Parent).Items.Remove(this);
                        return true;
                    }
                });
            }
        }

        private void QueueButtonClick(object sender, MouseButtonEventArgs e) => MainWindow.Player.AddQueue(FilePath);

        private void DeleteButtonClick(object sender, MouseButtonEventArgs e)
        {
            DatabaseUtils.Remove(FilePath);
            ((ListBox)Parent).Items.Remove(this);
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (MainWindow.Player.Playing) MainWindow.Player.ClearQueue();
                MainWindow.Player.AddQueue(FilePath);
                MainWindow.Player.PlayMusic();
            }
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MainPanel_ContextMenuOpening(object sender, RoutedEventArgs e) // TODO: refactor this
        {
            MiscContext.Items.Clear();
            var playlists = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            foreach (var playlist in playlists)
            {
                var tracks = DatabaseUtils.ReadTracksForPlaylist(playlist.Name);
                var trackIsInPlaylist = tracks.Where(x => x.Path == FilePath).Count() != 0;
                var item = new MenuItem
                {
                    Header = playlist.Name,
                    IsCheckable = true
                };
                item.IsChecked = trackIsInPlaylist;
                item.Click += (object sende, RoutedEventArgs ee) =>
                {
                    if (trackIsInPlaylist) DatabaseUtils.RemoveTrackFromPlaylist((string)item.Header, FilePath);
                    else DatabaseUtils.AddTrackToPlaylist((string)item.Header, FilePath);
                };
                MiscContext.Items.Add(item);
            }
            MiscContext.Items.Add(new Separator());
            var otheritem = new MenuItem();
            otheritem.Header = Properties.Resources.PLAYLISTMANAGEMENT;
            otheritem.Click += (object send, RoutedEventArgs eee) =>
            {
                var management = new PlaylistManagement(FilePath);
                management.ShowDialog();
            };
            MiscContext.Items.Add(otheritem);
        }

        private void OpenInFileExplorer_Click(object sender, RoutedEventArgs e) => Process.Start(Path.GetDirectoryName(FilePath));
    }
}
