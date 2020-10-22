using ATL;
using ATL.Playlist;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
        public QueueManagement()
        {
            InitializeComponent();
            PopulateList();
            MainWindow.Player.QueueChanged += Player_QueueChanged;
        }

        public void PopulateList() // God have mercy on future me/others.
        {
            displayqueue.Enqueue(MainWindow.Player.Queue);
            async void GetResults()
            {
                var list = displayqueue.Dequeue();
                var nextlength = 0;
                int number = 1;
                SetControlEnabled(false);
                //QueueList.Visibility = Visibility.Hidden;
                QueueList.Items.Clear();
                await Task.Run(() =>
                {
                    foreach (var song in list)
                    {
                        if (displayqueue.Count > 1) break;
                        QueueEntry entry;
                        var dbTrack = DatabaseUtils.GetFallbackTrack(song);
                        entry = Dispatcher.Invoke(() => new QueueEntry(dbTrack.Artist, dbTrack.Album, dbTrack.Title, number.ToString(), number - 1));
                        Dispatcher.Invoke(() => QueueList.Items.Add(entry));
                        if (entry.Index + 1 == MainWindow.Player.QueuePosition) currentIndex = entry.Index;
                        if (MainWindow.Player.QueuePosition < number) nextlength += dbTrack.Length;
                        number++;
                    }
                });
                if (QueueList.Items.Count > 0) (QueueList.Items[currentIndex] as QueueEntry).BringIntoView();
                RemainingTimeLabel.Text = Properties.Resources.QUEUEMANAGEMENT_REMAININGTIME + new TimeSpan(0, 0, 0, nextlength).ToString(@"hh\:mm\:ss");
                SetControlEnabled(true);
                //QueueList.Visibility = Visibility.Visible;
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
        private void Player_QueueChanged(object sender, EventArgs e)
        {

            PopulateList();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Player.QueueChanged -= Player_QueueChanged;
            QueueList.Items.Clear();
        }

        private void AddTrackButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Audio Files|*.wav;*.aiff;*.mp3;*.wma;*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wmv;*.aac;*.adts;*.avi;*.m4a;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.flac|Other|*";
            if (dialog.ShowDialog() == true)
            {
                MainWindow.Player.AddQueue(dialog.FileName);
            }
        }

        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
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
                            ContentText = $"One of the tracks in the playlist was not added because it could not be found.\nMissing File: {s}",
                            IsImportant =  true,
                            DisplayAsToast = true,
                            Type = NotificationType.Failure
                        });
                        continue;
                    }
                    MainWindow.Player.AddQueue(s);
                }
            }
        }

        private void ClearQueue_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.Player.ClearQueue();
        }
    }
}
