using ATL.Playlist;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
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

        private readonly DatabaseHandlerX library;
        private readonly NotificationHandler notificationHandler;
        public PlaylistManagement(DatabaseHandlerX library, NotificationHandler notificationHandler, string track = null)
        {
            this.library = library;
            this.notificationHandler = notificationHandler;
            InitializeComponent();
            if (track != null) EditingHeader.Text = string.Format(Properties.Resources.PLAYLISTMANAGEMENT_HEADER, Path.GetFileName(track));
            else EditingHeader.Visibility = Visibility.Collapsed;
            this.track = track;
            InitFields();
        }
        public async void InitFields()
        {
            PlaylistBox.Items.Clear();
            var x = library.Library.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            await Task.Run(() =>
            {
                foreach (var thing in x)
                {
                    Dispatcher.Invoke(() => PlaylistBox.Items.Add(new PlaylistEntry(thing.Name, track, library)));
                }
            });
        }

        private void OKButton_Click(object sender, RoutedEventArgs e) => Close();

        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FMPTextEntryBox("Playlist Name");
            dialog.ShowDialog();
            if (dialog.OK) library.CreatePlaylist(dialog.Response, track);
            InitFields();
        }

        private void ImportButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Playlist Files|*.xspf;*.asx;*.wax;*.wvx;*.b4s;*.m3u;*.m3u8;*.pls;*.smil;*.smi;*.zpl;";
            if (dialog.ShowDialog() == true)
            {
                IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(dialog.FileName);
                foreach (string s in reader.FilePaths)
                {
                    if (!File.Exists(s))
                    {
                        notificationHandler.Add(new Notification
                        {
                            ContentText = string.Format(Properties.Resources.NOTIFICATION_COULD_NOT_IMPORT_PLAYLIST, s),
                            IsImportant = true,
                            DisplayAsToast = true,
                            Type = NotificationType.Failure
                        });
                        continue;
                    }
                    library.AddTrackToPlaylist(Path.GetFileNameWithoutExtension(dialog.FileName), s);
                }
            }
            InitFields();
        }
    }
}
