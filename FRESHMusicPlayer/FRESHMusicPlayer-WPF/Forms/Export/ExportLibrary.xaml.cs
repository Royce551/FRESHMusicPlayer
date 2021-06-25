using FRESHMusicPlayer.Handlers;
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
using System.IO;
using WinForms = System.Windows.Forms;
using ATL.Playlist;

namespace FRESHMusicPlayer.Forms.Export
{
    /// <summary>
    /// Interaction logic for ExportLibrary.xaml
    /// </summary>
    public partial class ExportLibrary : Window
    {
        private readonly MainWindow window;

        public ExportLibrary(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
        }

        private async void ExportFolderButton_Click(object sender, RoutedEventArgs e)
        {
            var count = window.Library.Read().Count;
            var result = MessageBox.Show($"{count * 5}MB - mp3, {count * 50}MB - flac, ok?", "FRESHMusicPlayer", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
            if (result != MessageBoxResult.OK) return;

            var dialog = new WinForms.FolderBrowserDialog();
            if (dialog.ShowDialog() != WinForms.DialogResult.OK) return;

            
            await Task.Run(() =>
            {
                IPlaylistIO pls = PlaylistIOFactory.GetInstance().GetPlaylistIO(Path.Combine(dialog.SelectedPath, "playlist.m3u8"));

                var newPaths = new List<string>();
                int progress = 0;
                foreach (var file in window.Library.Read().Select(x => x.Path))
                {
                    var newPath = Path.Combine(dialog.SelectedPath, Path.GetFileName(file));

                    while (File.Exists(newPath))
                    {
                        newPath = $"x{newPath}";
                    }
                    File.Copy(file, newPath);
                    progress++;
                    newPaths.Add(newPath);
                    Dispatcher.Invoke(() => ExportFolderSubheader.Text = $"{progress}/{count} - Copying {file}");
                }

                Dispatcher.Invoke(() => ExportFolderSubheader.Text = $"Creating playlist files...");
                pls.FilePaths = newPaths;
                foreach (var playlist in window.Library.Database.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList())
                {
                    var playlistFile = PlaylistIOFactory.GetInstance().GetPlaylistIO(Path.Combine(dialog.SelectedPath, "playlists", $"{playlist.DatabasePlaylistID}.m3u8"));
                    playlistFile.FilePaths = playlist.Tracks;
                }
                Dispatcher.Invoke(() => ExportFolderSubheader.Text = $"Done!");
            });
            
        }

        private void ExportJSONButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
