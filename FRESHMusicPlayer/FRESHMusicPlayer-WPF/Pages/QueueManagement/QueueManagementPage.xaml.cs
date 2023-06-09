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
using System.Windows.Input;
using System.Windows.Threading;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for QueueManagementPage.xaml
    /// </summary>
    public partial class QueueManagement : UserControl
    {
        private bool taskIsRunning;
        private readonly Queue<List<string>> displayqueue = new Queue<List<string>>();
        private int currentIndex = 0;
        private List<string> lastQueue = new List<string>();
        private DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100), IsEnabled = true };

        private readonly MainWindow window;
        public QueueManagement(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            PopulateList();
            window.Player.Queue.QueueChanged += Player_QueueChanged;
            window.Player.SongStopped += Player_SongStopped;
            timer.Tick += Timer_Tick;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!window.Player.FileLoaded) return;

            var remainingTime = new TimeSpan();
            var queueAsQueueEntries = QueueList.Items.Cast<QueueEntry>().ToList();

            for (int i = 0; i < queueAsQueueEntries.Count; i++)
            {
                if ((i + 1) < window.Player.Queue.Position) continue;
                var track = queueAsQueueEntries[i];
                if (i != (queueAsQueueEntries.Count - 1)) remainingTime += TimeSpan.FromSeconds(track.Length);
            }
            remainingTime += (window.Player.TotalTime - window.Player.CurrentTime);
            RemainingTimeLabel.Text = string.Format(Properties.Resources.QUEUEMANAGEMENT_ENDSAT, (DateTime.Now + remainingTime).ToString("t"));
        }

        public void PopulateList()
        {
            displayqueue.Enqueue(window.Player.Queue.Queue); // Queue of pending queue management updates
            async void GetResults()
            {
                var list = displayqueue.Dequeue();
                var nextLength = 0; // length of the tracks that come after the current
                int number = 1;
                SetControlEnabled(false);
                await Task.Run(async () =>
                {
                    
                    if (!list.SequenceEqual(lastQueue)) // has the contents of the queue changed, or just the positions?
                    {                                   // yes; refresh list
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
                            var correspondingQueueEntry = await window.Library.GetFallbackTrackAsync(window.Player.Queue.Queue[i]);

                            Dispatcher.Invoke(() =>
                            {
                                entry.Artists = correspondingQueueEntry.Artists;
                                entry.Album = correspondingQueueEntry.Album;
                                entry.Title = correspondingQueueEntry.Title;
                                entry.Index = i;
                                entry.Position = (i + 1).ToString();
                                entry.Length = correspondingQueueEntry.Length;
                                entry.UpdatePosition();
                            });
                        }
                    }
                    else // no; just update positions (massively faster)
                    {
                        foreach (var item in QueueList.Items)
                        {
                            if (item is QueueEntry entry)
                            {
                                Dispatcher.Invoke(() => entry.UpdatePosition());
                                if (entry.Index + 1 == window.Player.Queue.Position) currentIndex = entry.Index;
                                if (window.Player.Queue.Position < number) nextLength += entry.Length;
                            }
                            number++;
                        }
                    }            // convert the items in the listbox into a list of file paths to be compared with `list`
                    lastQueue = Dispatcher.Invoke(() => QueueList.Items.Cast<QueueEntry>().Select(x => window.Player.Queue.Queue[x.Index]).ToList());
                });

                if (QueueList.Items.Count > 0) (QueueList.Items[currentIndex] as QueueEntry).BringIntoView(); // Bring current track into view
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
            timer.Tick -= Timer_Tick;
            timer.Stop();
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

        private void UserControl_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AddTrackButton.Visibility = AddPlaylistButton.Visibility = ClearQueueButton.Visibility = Visibility.Visible;
        }

        private void UserControl_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            AddTrackButton.Visibility = AddPlaylistButton.Visibility = ClearQueueButton.Visibility = Visibility.Collapsed;
        }

        private void QueueList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is QueueEntry uc)
                    uc.ShowButtons();
            }
            foreach (var item in e.RemovedItems)
            {
                if (item is QueueEntry uc)
                    uc.HideButtons();
            }
        }

        private void QueueList_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (QueueList.IsKeyboardFocusWithin) return;

            foreach (var item in QueueList.Items)
            {
                if (item is QueueEntry uc)
                    uc.HideButtons();
            }
            QueueList.SelectedItem = null;
        }


        private void UserControl_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            AddTrackButton.Visibility = AddPlaylistButton.Visibility = ClearQueueButton.Visibility = Visibility.Visible;
        }

        private void UserControl_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            AddTrackButton.Visibility = AddPlaylistButton.Visibility = ClearQueueButton.Visibility = Visibility.Collapsed;
        }

        //private void PreviousQueueButton_Click(object sender, RoutedEventArgs e) for use in FMP 10.2
        //{

        //}

        //private void NextQueueButton_Click(object sender, RoutedEventArgs e)
        //{

        //}
    }
}
