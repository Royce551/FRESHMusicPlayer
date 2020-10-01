using ATL.Playlist;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace FRESHMusicPlayer.Forms.Playlists
{
    /// <summary>
    /// Interaction logic for PlaylistEntry.xaml
    /// </summary>
    public partial class PlaylistEntry : UserControl
    {
        private bool playlistExists = false;
        private string playlist;
        private string path;
        public PlaylistEntry(string playlist, string path)
        {
            InitializeComponent();
            TitleLabel.Text = playlist;
            this.playlist = playlist;
            this.path = path;
            CheckIfPlaylistExists();
        }
        private void CheckIfPlaylistExists()
        {
            foreach (var thing in DatabaseUtils.ReadTracksForPlaylist(playlist))
            {
                if (thing.Path == path)
                {
                    AddRemoveButton.Content = "-";
                    playlistExists = true;
                    return;
                }
            }
            AddRemoveButton.Content = "+";
            playlistExists = false;
        }
        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            AddRemoveButton.Visibility = RenameButton.Visibility = DeleteButton.Visibility = ExportButton.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            AddRemoveButton.Visibility = RenameButton.Visibility = DeleteButton.Visibility = ExportButton.Visibility = Visibility.Collapsed;
        }

        private void RenameButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FMPTextEntryBox("Playlist Name", playlist);
            dialog.ShowDialog();
            if (dialog.OK)
            {
                var x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
                x.Name = dialog.Response;
                TitleLabel.Text = dialog.Response;
                MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Update(x);
            }
        }

        private void AddRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (playlistExists) DatabaseUtils.RemoveTrackFromPlaylist(playlist, path);
            else DatabaseUtils.AddTrackToPlaylist(playlist, path);
            CheckIfPlaylistExists();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) => DatabaseUtils.DeletePlaylist(playlist);

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var tracks = DatabaseUtils.ReadTracksForPlaylist(playlist);
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
    }
}
