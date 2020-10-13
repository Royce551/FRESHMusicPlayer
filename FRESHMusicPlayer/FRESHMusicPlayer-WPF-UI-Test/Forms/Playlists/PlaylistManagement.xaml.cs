using ATL.Playlist;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace FRESHMusicPlayer.Forms.Playlists
{
    /// <summary>
    /// Interaction logic for PlaylistManagement.xaml
    /// </summary>
    public partial class PlaylistManagement : Window
    {
        private readonly string track;
        public PlaylistManagement(string track = null)
        {
            InitializeComponent();
            if (track != null) EditingHeader.Text = $"What do you want to do with \"{Path.GetFileName(track)}\"?";
            else EditingHeader.Visibility = Visibility.Collapsed;
            this.track = track;
            InitFields();
        }
        public async void InitFields()
        {
            PlaylistBox.Items.Clear();
            var x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            await Task.Run(() =>
            {
                foreach (var thing in x)
                {
                    Dispatcher.Invoke(() => PlaylistBox.Items.Add(new PlaylistEntry(thing.Name, track)));
                }
            });
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) => Close();

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FMPTextEntryBox("Playlist Name");
            dialog.ShowDialog();
            if (dialog.OK) DatabaseUtils.CreatePlaylist(dialog.Response, track);
            InitFields();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Playlist Files|*.xspf;*.asx;*.wax;*.wvx;*.b4s;*.m3u;*.m3u8;*.pls;*.smil;*.smi;*.zpl;";
            if (dialog.ShowDialog() == true)
            {
                IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(dialog.FileName);
                foreach (string s in reader.FilePaths)
                {
                    if (!File.Exists(s))
                    {
                        MainWindow.NotificationHandler.Add(new Notification
                        {
                            HeaderText = Properties.Resources.NOTIFICATION_COULD_NOT_IMPORT_PLAYLIST,
                            ContentText = $"This playlist file could not be imported because one or more of the tracks could not be found.\nMissing File: {s}",
                            IsImportant = true,
                            DisplayAsToast = true,
                            Type = NotificationType.Failure
                        });
                        continue;
                    }
                    DatabaseUtils.AddTrackToPlaylist(Path.GetFileNameWithoutExtension(dialog.FileName), s);
                }
            }
            InitFields();
        }
    }
}
