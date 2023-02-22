﻿using ATL.Playlist;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FRESHMusicPlayer.Forms.Playlists
{
    /// <summary>
    /// Interaction logic for PlaylistManagement.xaml
    /// </summary>
    public partial class PlaylistManagement : UserControl
    {
        private readonly string track;

        private readonly GUILibrary library;
        private readonly NotificationHandler notificationHandler;
        private readonly Tab selectedMenu;
        public PlaylistManagement(GUILibrary library, NotificationHandler notificationHandler, Tab selectedMenu, string track = null)
        {
            this.library = library;
            this.notificationHandler = notificationHandler;
            this.selectedMenu = selectedMenu;
            InitializeComponent();
            if (track != null) EditingHeader.Text = string.Format(Properties.Resources.PLAYLISTMANAGEMENT_HEADER, Path.GetFileName(track));
            else EditingHeader.Visibility = Visibility.Collapsed;
            this.track = track;
            InitFields();
        }
        public async void InitFields()
        {
            PlaylistBox.Items.Clear();
            var x = library.Database.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            await Task.Run(() =>
            {
                foreach (var thing in x)
                {
                    Dispatcher.Invoke(() => PlaylistBox.Items.Add(new PlaylistEntry(thing.Name, track, library, this, selectedMenu)));
                }
            });
        }

        private async void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FMPTextEntryBox("Playlist Name");
            dialog.ShowDialog();

            if (dialog.OK)
            {
                if (string.IsNullOrWhiteSpace(dialog.Response))
                    MessageBox.Show(string.Format(Properties.Resources.PLAYLISTMANAGEMENT_INVALIDNAME, dialog.Response));
                else
                {
                    await library.CreatePlaylistAsync(dialog.Response, false, track);
                    InitFields();
                }
            }
        }

        private async void ImportButton_Click(object sender, RoutedEventArgs e)
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
                    await library.AddTrackToPlaylistAsync(Path.GetFileNameWithoutExtension(dialog.FileName), s);
                }
            }
            InitFields();
        }

        private void PlaylistBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is PlaylistEntry uc)
                    uc.ShowButtons();
            }
            foreach (var item in e.RemovedItems)
            {
                if (item is PlaylistEntry uc)
                    uc.HideButtons();
            }
        }
    }
}
