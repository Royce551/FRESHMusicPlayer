using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace FRESHMusicPlayer.Pages.Library
{
    /// <summary>
    /// Interaction logic for SearchPage.xaml
    /// </summary>
    public partial class SearchPage : Page
    {
        private bool taskIsRunning = false;
        private Queue<string> searchqueries = new Queue<string>();
        private string searchterm = string.Empty;

        private readonly MainWindow window;
        public SearchPage(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {         
            searchqueries.Enqueue(SearchBox.Text.ToUpper());
            async void GetResults()
            {
                if (string.IsNullOrEmpty(SearchBox.Text))
                {
                    searchqueries.Clear();
                    return;
                }
                TracksPanel.Visibility = Visibility.Hidden; // avoids making everything flash
                InfoLabel.Visibility = Visibility.Hidden;
                int length = 0;
                taskIsRunning = true;
                searchterm = searchqueries.Dequeue();
                TracksPanel.Items.Clear();
                await Task.Run(() =>
                {
                    int i = 0;
                    foreach (var thing in window.Library.Database.GetCollection<DatabaseTrack>("tracks")
                        .Query()
                        .Where(x => x.Title.ToUpper().Contains(searchterm) || x.Artist.ToUpper().Contains(searchterm) || x.Album.ToUpper().Contains(searchterm))
                        .OrderBy("Title")
                        .ToList())
                    {
                        if (searchqueries.Count > 1) break; // optimization for typing quickly
                        Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                        length += thing.Length;
                        if (i % 25 == 0) Thread.Sleep(1); // Apply a slight delay once in a while to let the UI catch up
                        i++;
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
                    InfoLabel.Visibility = Visibility.Visible;
                }
            }
            if (!taskIsRunning) GetResults();
        }
        private void QueueAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongEntry entry in TracksPanel.Items)
            {
                window.Player.Queue.Add(entry.FilePath);
            }
        }

        private async void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (SongEntry entry in TracksPanel.Items)
            {
                window.Player.Queue.Add(entry.FilePath);
            }
            await window.Player.PlayAsync();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Dispatcher.Invoke(() => SearchBox.Focus(), DispatcherPriority.ApplicationIdle);
    }
}
