using ATL;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using LiteDB;
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
using System.Windows.Threading;

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
                searchterm = searchqueries.Dequeue();
                TracksPanel.Items.Clear();
                await Task.Run(() =>
                {
                        foreach (var thing in MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks")
                            .Query()
                            .Where(x => x.Title.ToUpper().Contains(searchterm) || x.Artist.ToUpper().Contains(searchterm) || x.Album.ToUpper().Contains(searchterm))
                            .OrderBy("Title")
                            .ToList())
                        {
                            if (searchqueries.Count > 1) break; // optimization for typing quickly
                            Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title)));
                            length += thing.Length;
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
