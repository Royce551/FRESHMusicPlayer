using ATL;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers.Notifications;
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

namespace FRESHMusicPlayer_WPF_UI_Test.Pages.Library
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        private bool taskIsRunning = false;
        private Queue<string> searchqueries = new Queue<string>();
        private string searchterm = "";
        public SearchPage()
        {
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {         
            searchqueries.Enqueue(SearchBox.Text.ToUpper());
            async void GetResults()
            {
                TracksPanel.Visibility = Visibility.Hidden; // avoids making everything flash
                InfoLabel.Text = "Loading, please wait.";
                int length = 0;
                taskIsRunning = true;
                int total = MainWindow.Library.Count;
                searchterm = searchqueries.Dequeue();
                TracksPanel.Items.Clear();
                await Task.Run(() =>
                {
                    foreach (string thing in MainWindow.Library)
                    {
                        Track track = new Track(thing);
                        if (track.Title.ToUpper().Contains(searchterm) || track.Artist.ToUpper().Contains(searchterm) || track.Album.ToUpper().Contains(searchterm))
                        {
                            Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing, track.Artist, track.Album, track.Title)));
                            length += track.Duration;
                        }            
                    }
                });
                InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
                taskIsRunning = false;
                if (searchqueries.Count != 0)
                {
                    GetResults();
                }
                else
                {
                    TracksPanel.Visibility = Visibility.Visible;
                    return;
                }
            }
            if (!taskIsRunning) GetResults();
        }
        private void QueueAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongEntry entry in TracksPanel.Items)
            {
                MainWindow.Player.AddQueue(entry.FilePath);
            }
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongEntry entry in TracksPanel.Items)
            {
                MainWindow.Player.AddQueue(entry.FilePath);
            }
            MainWindow.Player.PlayMusic();
        }
    }
}
