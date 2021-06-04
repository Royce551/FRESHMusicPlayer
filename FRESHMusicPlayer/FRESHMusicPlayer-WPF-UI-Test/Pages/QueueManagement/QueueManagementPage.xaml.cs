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

        private readonly MainWindow window;
        public QueueManagement(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            PopulateList();
            window.Player.Queue.QueueChanged += Player_QueueChanged;
            window.Player.SongStopped += Player_SongStopped;
        }

        public void PopulateList()
        {
            displayqueue.Enqueue(window.Player.Queue.Queue); // Queue of pending queue management updates
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
                        var dbTrack = window.Library.GetFallbackTrack(song);
                        entry = Dispatcher.Invoke(() => new QueueEntry(dbTrack.Artist, dbTrack.Album, dbTrack.Title, number.ToString(), number - 1, window.Player));
                        Dispatcher.Invoke(() => QueueList.Items.Add(entry));
                        if (entry.Index + 1 == window.Player.Queue.Position) currentIndex = entry.Index;
                        if (window.Player.Queue.Position < number) nextlength += dbTrack.Length;
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
            window.Player.Queue.QueueChanged -= Player_QueueChanged;
            window.Player.SongStopped -= Player_SongStopped;
            QueueList.Items.Clear();
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files|*.wav;*.aiff;*.mp3;*.wma;*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wmv;*.aac;*.adts;*.avi;*.m4a;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.flac|Other|*";
            if (dialog.ShowDialog() == true)
            {
                window.Player.Queue.Add(dialog.FileName);
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
                        window.NotificationHandler.Add(new Notification
                        {
                            ContentText = string.Format(Properties.Resources.NOTIFICATION_COULD_NOT_IMPORT_PLAYLIST, s),
                            IsImportant =  true,
                            DisplayAsToast = true,
                            Type = NotificationType.Failure
                        });
                        continue;
                    }
                    window.Player.Queue.Add(s);
                }
            }
        }

        private void ClearQueue_Click(object sender, RoutedEventArgs e)
        {
            window.Player.Queue.Clear();
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {  
            InterfaceUtils.DoDragDrop((string[])e.Data.GetData(DataFormats.FileDrop), window.Player, window.Library, import: false, clearqueue: false);
        }

        private void PreviousQueueButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextQueueButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
