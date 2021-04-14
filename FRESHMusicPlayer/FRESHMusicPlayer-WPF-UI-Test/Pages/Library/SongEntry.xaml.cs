using FRESHMusicPlayer.Forms.Playlists;
using FRESHMusicPlayer.Utilities;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Handlers;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Pages.Library
{
    /// <summary>
    /// Interaction logic for SongEntry.xaml
    /// </summary>
    public partial class SongEntry : UserControl
    {
        public string FilePath;
        public string Title;

        private Player player;
        private NotificationHandler notificationHandler;
        private GUILibrary library;
        public SongEntry(string filePath, string artist, string album, string title, Player player, NotificationHandler notificationHandler, GUILibrary library)
        {
            this.player = player;
            this.notificationHandler = notificationHandler;
            this.library = library;
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
                if (player.FileLoaded) player.Queue.Clear();
                player.PlayMusic(FilePath);
            }
            else
            {
                notificationHandler.Add(new Handlers.Notifications.Notification
                {
                    ContentText = string.Format(Properties.Resources.NOTIFICATION_FILEGONE, FilePath),
                    ButtonText = "Remove from library",
                    OnButtonClicked = () =>
                    {
                        library.Remove(FilePath);
                        ((ListBox)Parent).Items.Remove(this);
                        return true;
                    }
                });
            }
        }

        private void QueueButtonClick(object sender, MouseButtonEventArgs e) => player.Queue.Add(FilePath);

        private void DeleteButtonClick(object sender, MouseButtonEventArgs e)
        {
            library.Remove(FilePath);
            ((ListBox)Parent).Items.Remove(this);
        }

        private async void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                await Task.Run(() =>
                {
                    if (player.FileLoaded) player.Queue.Clear();
                    player.PlayMusic(FilePath);
                });
                
            }
        }

        private void ContextMenu_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void MainPanel_ContextMenuOpening(object sender, RoutedEventArgs e)
        {
            MiscContext.Items.Clear();
            var playlists = library.Database.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToEnumerable();
            foreach (var playlist in playlists)
            {
                var tracks = library.ReadTracksForPlaylist(playlist.Name);
                var trackIsInPlaylist = tracks.Any(x => x.Path == FilePath);
                var item = new MenuItem
                {
                    Header = playlist.Name,
                    IsCheckable = true
                };
                item.IsChecked = trackIsInPlaylist;
                item.Click += (object sende, RoutedEventArgs ee) =>
                {
                    if (trackIsInPlaylist) library.RemoveTrackFromPlaylist((string)item.Header, FilePath);
                    else library.AddTrackToPlaylist((string)item.Header, FilePath);
                };
                MiscContext.Items.Add(item);
            }
            MiscContext.Items.Add(new Separator());
            var otheritem = new MenuItem();
            otheritem.Header = Properties.Resources.PLAYLISTMANAGEMENT;
            otheritem.Click += (object send, RoutedEventArgs eee) =>
            {
                var management = new PlaylistManagement(library, notificationHandler, FilePath);
                management.ShowDialog();
            };
            MiscContext.Items.Add(otheritem);
        }

        private void OpenInFileExplorer_Click(object sender, RoutedEventArgs e) => Process.Start(Path.GetDirectoryName(FilePath));
    }
}
