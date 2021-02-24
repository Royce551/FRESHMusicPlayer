using ATL.Playlist;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for QueueManagementPage.xaml
    /// </summary>
    public partial class QueueManagement : Page
    {
        private bool taskisrunning;
        private readonly Queue<List<string>> displayqueue = new Queue<List<string>>();
        private int currentIndex = 0;

        private readonly Player player;
        private readonly DatabaseHandlerX library;
        private readonly NotificationHandler notificationHandler;
        public QueueManagement(Player player, DatabaseHandlerX library, NotificationHandler notificationHandler)
        {
            this.player = player;
            this.library = library;
            this.notificationHandler = notificationHandler;
            InitializeComponent();
            PopulateList();
            player.QueueChanged += Player_QueueChanged;
            player.SongStopped += Player_SongStopped;
        }

        public void PopulateList()
        {
            displayqueue.Enqueue(player.Queue); // Queue of pending queue management updates
            async void GetResults()
            {
                var list = displayqueue.Dequeue();
                var nextlength = 0;
                int number = 1;
                SetControlEnabled(false);
                QueueList.Items.Clear();
                await Task.Run(() => // Display controls
                {
                    foreach (var song in list)
                    {
                        if (displayqueue.Count > 1) break;
                        QueueEntry entry;
                        var dbTrack = library.GetFallbackTrack(song);
                        entry = Dispatcher.Invoke(() => new QueueEntry(dbTrack.Artist, dbTrack.Album, dbTrack.Title, number.ToString(), number - 1, player));
                        Dispatcher.Invoke(() => QueueList.Items.Add(entry));
                        if (entry.Index + 1 == player.QueuePosition) currentIndex = entry.Index;
                        if (player.QueuePosition < number) nextlength += dbTrack.Length;
                        if (number % 25 == 0) Thread.Sleep(1); // Apply a slight delay once in a while to let the UI catch up
                        number++;
                    }
                });
                if (QueueList.Items.Count > 0) (QueueList.Items[currentIndex] as QueueEntry).BringIntoView(); // Bring current track into view
                RemainingTimeLabel.Text = Properties.Resources.QUEUEMANAGEMENT_REMAININGTIME + new TimeSpan(0, 0, 0, nextlength).ToString(@"hh\:mm\:ss");
                SetControlEnabled(true);
                taskisrunning = false;
                if (displayqueue.Count != 0) GetResults();
                else return;
            }
            if (!taskisrunning)
            {
                taskisrunning = true;
                GetResults();
            }
        }
        private void SetControlEnabled(bool enabled)
        {
            AddTrackButton.IsEnabled = enabled;
            AddPlaylistButton.IsEnabled = enabled;
            ClearQueueButton.IsEnabled = enabled;
        }
        private void Player_QueueChanged(object sender, EventArgs e) => PopulateList();
        private void Player_SongStopped(object sender, EventArgs e) => PopulateList();
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            player.QueueChanged -= Player_QueueChanged;
            player.SongStopped -= Player_SongStopped;
            QueueList.Items.Clear();
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files|*.wav;*.aiff;*.mp3;*.wma;*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wmv;*.aac;*.adts;*.avi;*.m4a;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.flac|Other|*";
            if (dialog.ShowDialog() == true)
            {
                player.AddQueue(dialog.FileName);
            }
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
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
                            IsImportant =  true,
                            DisplayAsToast = true,
                            Type = NotificationType.Failure
                        });
                        continue;
                    }
                    player.AddQueue(s);
                }
            }
        }

        private void ClearQueue_Click(object sender, RoutedEventArgs e)
        {
            player.ClearQueue();
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {  
            InterfaceUtils.DoDragDrop((string[])e.Data.GetData(DataFormats.FileDrop), player, library, import: false, clearqueue: false);
        }
    }
}
