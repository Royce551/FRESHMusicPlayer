using ATL.Playlist;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FRESHMusicPlayer.Forms.Playlists
{
    /// <summary>
    /// Interaction logic for PlaylistEntry.xaml
    /// </summary>
    public partial class PlaylistEntry : UserControl
    {
        private readonly bool trackExists = true;
        private readonly string playlist;
        private readonly string path;

        private readonly GUILibrary library;
        private readonly PlaylistManagement window;
        private readonly Tab selectedMenu;
        public PlaylistEntry(string playlist, string path, GUILibrary library, PlaylistManagement window, Tab selectedMenu)
        {
            this.library = library;
            this.window = window;
            this.selectedMenu = selectedMenu;
            InitializeComponent();
            if (path is null) trackExists = false;
            TitleLabel.Text = playlist;

            if (selectedMenu == Tab.Artists) AddThingButton.Content = $"+ {Properties.Resources.TAGEDITOR_ARTIST}";
            else AddThingButton.Content = $"+ {Properties.Resources.TRACKINFO_ALBUM}";

            this.playlist = playlist;
            this.path = path;
            CheckIfPlaylistExists();
        }
        private void CheckIfPlaylistExists()
        {
            foreach (var thing in library.ReadTracksForPlaylist(playlist))
            {
                if (thing.Path == path)
                {
                    AddButton.IsEnabled = false;
                    RemoveButton.IsEnabled = true;
                    return;
                }
            }
            AddButton.IsEnabled = true;
            RemoveButton.IsEnabled = false;
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            MiscButton.Visibility = Visibility.Visible;
            if (trackExists)
            {
                AddButton.Visibility = RemoveButton.Visibility = Visibility.Visible;
                if (selectedMenu == Tab.Artists || selectedMenu == Tab.Albums)
                    AddThingButton.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            AddButton.Visibility = RemoveButton.Visibility = MiscButton.Visibility = AddThingButton.Visibility = Visibility.Collapsed;
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FMPTextEntryBox("Playlist Name", playlist);
            dialog.ShowDialog();
            if (dialog.OK)
            {
                var x = library.Database.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
                x.Name = dialog.Response;
                TitleLabel.Text = dialog.Response;
                library.Database.GetCollection<DatabasePlaylist>("playlists").Update(x);
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            library.RemoveTrackFromPlaylist(playlist, path);
            CheckIfPlaylistExists();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            library.AddTrackToPlaylist(playlist, path);
            CheckIfPlaylistExists();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            library.DeletePlaylist(playlist);
            window.InitFields();
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var tracks = library.ReadTracksForPlaylist(playlist);
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "M3U UTF-8 Playlist|*.m3u8|Other|*";
            if (saveFileDialog.ShowDialog() == true)
            {
                IPlaylistIO pls = PlaylistIOFactory.GetInstance().GetPlaylistIO(saveFileDialog.FileName);
                IList<string> pathsToWrite = new List<string>();
                foreach (var track in tracks)
                {
                    pathsToWrite.Add(track.Path);
                }
                pls.FilePaths = pathsToWrite;
            }
        }

        private void AddThingButton_Click(object sender, RoutedEventArgs e)
        {
            List<DatabaseTrack> things;
            if (selectedMenu == Tab.Artists)
                things = library.ReadTracksForArtist(library.GetFallbackTrack(path).Artist);
            else
                things = library.ReadTracksForAlbum(library.GetFallbackTrack(path).Album);
            foreach (var thing in things)
                library.AddTrackToPlaylist(playlist, thing.Path);
            CheckIfPlaylistExists();
        }

        private void MiscButton_Click(object sender, RoutedEventArgs e)
        {
            var cm = FindResource("MiscContext") as ContextMenu;
            cm.PlacementTarget = sender as Button;
            cm.IsOpen = true;
        }
    }
}
