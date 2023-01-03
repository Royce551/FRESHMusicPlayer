﻿using FRESHMusicPlayer.Forms.Playlists;
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
using System.Collections.Generic;
using System.Windows.Media;

namespace FRESHMusicPlayer.Pages.Library
{
    /// <summary>
    /// Interaction logic for SongEntry.xaml
    /// </summary>
    public partial class SongEntry : UserControl
    {
        public string FilePath;
        public string Title;

        private string[] artists;
        private string album;
        private MainWindow window;
        private NotificationHandler notificationHandler;
        private GUILibrary library;
        public SongEntry(string filePath, string[] artists, string album, string title, MainWindow window, NotificationHandler notificationHandler, GUILibrary library)
        {
            this.window = window;
            this.notificationHandler = notificationHandler;
            this.library = library;
            InitializeComponent();
            FilePath = filePath;
            ArtistAlbumLabel.Text = $"{string.Join(", ", artists)} ・ {album}";
            this.artists = artists;
            this.album = album;
            TitleLabel.Text = title;
            Title = title;
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e) => ShowButtons();

        private void UserControl_MouseLeave(object sender, MouseEventArgs e) => HideButtons();

        public void ShowButtons()
        {
            PlayButton.Visibility = QueueButton.Visibility = DeleteButton.Visibility = PlayHitbox.Visibility = QueueHitbox.Visibility = DeleteHitbox.Visibility = Visibility.Visible;

            TitleLabel.SetResourceReference(ForegroundProperty, "PrimaryTextColorOverAccent");
            ArtistAlbumLabel.SetResourceReference(ForegroundProperty, "SecondaryTextColorOverAccent");
            PlayButton.SetResourceReference(System.Windows.Shapes.Path.FillProperty, "PrimaryTextColorOverAccent");
            QueueButton.SetResourceReference(System.Windows.Shapes.Path.FillProperty, "PrimaryTextColorOverAccent");
        }

        public void HideButtons()
        {
            PlayButton.Visibility = QueueButton.Visibility = DeleteButton.Visibility = PlayHitbox.Visibility = QueueHitbox.Visibility = DeleteHitbox.Visibility = Visibility.Collapsed;
            TitleLabel.SetResourceReference(ForegroundProperty, "PrimaryTextColor");
            ArtistAlbumLabel.SetResourceReference(ForegroundProperty, "SecondaryTextColor");
            PlayButton.SetResourceReference(System.Windows.Shapes.Path.FillProperty, "PrimaryTextColor");
            QueueButton.SetResourceReference(System.Windows.Shapes.Path.FillProperty, "PrimaryTextColor");
        }

        private async void PlayButtonClick(object sender, RoutedEventArgs e)
        {
            if (FilePath.StartsWith("http") || File.Exists(FilePath))
            {
                if (window.Player.FileLoaded) window.Player.Queue.Clear();
                await window.Player.PlayAsync(FilePath);
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
                    },
                    Type = NotificationType.Failure
                });
            }
        }

        private void QueueButtonClick(object sender, RoutedEventArgs e) => window.Player.Queue.Add(FilePath);

        private void PlayNextContext_Click(object sender, RoutedEventArgs e)
        {
            window.Player.Queue.PlayNext(FilePath);
        }

        private void DeleteButtonClick(object sender, RoutedEventArgs e)
        {
            library.RaiseLibraryChanged = false;
            library.Remove(FilePath);
            library.RaiseLibraryChanged = true;
            ((ListBox)Parent).Items.Remove(this);
        }

        private async void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (window.Player.FileLoaded) window.Player.Queue.Clear();
                await window.Player.PlayAsync(FilePath);
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
                List<DatabaseTrack> tracks;
                try
                {
                    tracks = library.GetTracksForPlaylist(playlist.Name);
                }
                catch
                {
                    continue;
                }
                var trackIsInPlaylist = tracks.Any(x => x.Path == FilePath);
                var item = new MenuItem
                {
                    Header = playlist.Name,
                    IsCheckable = true
                };
                item.IsChecked = trackIsInPlaylist;
                item.Click += async (object sende, RoutedEventArgs ee) =>
                {
                    if (trackIsInPlaylist) library.RemoveTrackFromPlaylist((string)item.Header, FilePath);
                    else await library.AddTrackToPlaylistAsync((string)item.Header, FilePath);
                };
                MiscContext.Items.Add(item);
            }
            MiscContext.Items.Add(new Separator());
            var otheritem = new MenuItem();
            otheritem.Header = Properties.Resources.PLAYLISTMANAGEMENT;
            otheritem.Click += (object send, RoutedEventArgs eee) =>
            {
                var management = new PlaylistManagement(library, notificationHandler, ((Application.Current as App).MainWindow as MainWindow).CurrentTab, FilePath);
                management.ShowDialog();
            };
            MiscContext.Items.Add(otheritem);
        }

        private void OpenInFileExplorer_Click(object sender, RoutedEventArgs e) => Process.Start(Path.GetDirectoryName(FilePath));

        private void GoToArtistContext_Click(object sender, RoutedEventArgs e) => window.ChangeTabs(Tab.Artists, artists[0]);

        private void GoToAlbumContext_Click(object sender, RoutedEventArgs e) => window.ChangeTabs(Tab.Albums, album);
    }
}
