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
    public partial class QueueManagement : UserControl
    {
        private bool taskIsRunning;
        private readonly Queue<List<string>> displayqueue = new Queue<List<string>>();

        private readonly MainWindow window;
        public QueueManagement(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            PopulateList();
            window.Player.Queue.QueueChanged += Player_QueueChanged;
            window.Player.SongStopped += Player_SongStopped;
            window.ProgressTimer.Tick += ProgressTimer_Tick;
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            var remainingTime = new TimeSpan();
            int i = 1;
            int i2 = 0;
            var queueAsQueueEntries = QueueList.Items.Cast<QueueEntry>().ToList();
            foreach (var track in queueAsQueueEntries)
            {
                i++;
                if (i < window.Player.Queue.Position) continue;
                var y = queueAsQueueEntries[i2];
                remainingTime += TimeSpan.FromSeconds(y.Length);
                i2++;
            }
            remainingTime -= window.Player.CurrentTime;
            RemainingTimeLabel.Text = Properties.Resources.QUEUEMANAGEMENT_REMAININGTIME + remainingTime.ToString(@"hh\:mm\:ss");
        }

        public void PopulateList()
        {
            displayqueue.Enqueue(window.Player.Queue.Queue); // Queue of pending queue management updates
            async void GetResults()
            {
                var list = displayqueue.Dequeue();
                SetControlEnabled(false);
                await Task.Run(() =>
                {
                    var difference = QueueList.Items.Count - window.Player.Queue.Queue.Count;
                    while (difference > 0)
                    {
                        Dispatcher.Invoke(() => QueueList.Items.RemoveAt(QueueList.Items.Count - 1));
                        difference--;
                    }
                    while (difference < 0)
                    {
                        Dispatcher.Invoke(() => QueueList.Items.Add(new QueueEntry(null, null, null, null, 0, 0, window.Player)));
                        difference++;
                    }
                    for (int i = 0; i < QueueList.Items.Count; i++)
                    {
                        var entry = QueueList.Items[i] as QueueEntry;
                        var correspondingQueueEntry = window.Library.GetFallbackTrack(window.Player.Queue.Queue[i]);

                        Dispatcher.Invoke(() =>
                        {
                            entry.Artist = correspondingQueueEntry.Artist;
                            entry.Album = correspondingQueueEntry.Album;
                            entry.Title = correspondingQueueEntry.Title;
                            entry.Index = i;
                            entry.Position = (i + 1).ToString();
                            entry.Length = correspondingQueueEntry.Length;
                            entry.UpdatePosition();
                        });
                    }
                });

                //if (QueueList.Items.Count > 0) (QueueList.Items[currentIndex] as QueueEntry).BringIntoView(); // Bring current track into view
                SetControlEnabled(true);
                taskIsRunning = false;
                if (displayqueue.Count != 0) GetResults();
            }
            if (!taskIsRunning)
            {
                taskIsRunning = true;
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
            window.ProgressTimer.Tick -= ProgressTimer_Tick;
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

        //private void PreviousQueueButton_Click(object sender, RoutedEventArgs e) for use in FMP 10.2
        //{

        //}

        //private void NextQueueButton_Click(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
