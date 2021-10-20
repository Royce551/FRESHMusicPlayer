using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FRESHMusicPlayer.Pages.Library
{
    /// <summary>
    /// Interaction logic for LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : UserControl
    {
        private readonly MainWindow window;
        public LibraryPage(MainWindow window, string search = null)
        {
            this.window = window;
            InitializeComponent();
            LoadLibrary();
            CategoryPanel.Focus();
            if (search != null)
            {
                Thread.Sleep(10);
                CategoryPanel.SelectedItem = search;
            }
        }

        public async void LoadLibrary() // TODO: figure out how to make this not async void
        {
            TracksPanel.Items.Clear();
            CategoryPanel.Items.Clear();
            InfoLabel.Visibility = Visibility.Hidden;
            switch (window.CurrentTab) // all of this stuff is here so that i can avoid copying and pasting the same page thrice, maybe there's a better way?
            {
                case Tab.Tracks:
                    await ShowTracks();
                    break;
                case Tab.Artists:
                    await ShowArtists();
                    break;
                case Tab.Albums:
                    await ShowAlbums();
                    break;
                case Tab.Playlists:
                    await ShowPlaylists();
                    break;
            }
        }
        public async Task ShowTracks()
        {
            LeftSide.Width = new GridLength(0);
            int length = 0;
            await Task.Run(() =>
            {
                int i = 0;
                foreach (var thing in window.Library.Read())
                {
                    window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                    if (i % 25 == 0) Thread.Sleep(1); // Apply a slight delay once in a while to let the UI catch up
                    i++;
                }   
            });
            var lengthTimeSpan = new TimeSpan(0, 0, 0, length);
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $@"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {Math.Floor(lengthTimeSpan.TotalHours)}:{lengthTimeSpan:mm}:{lengthTimeSpan:ss}";
        }
        public async Task ShowArtists()
        {
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.Read("Artist"))
                {
                    if (CategoryPanel.Items.Contains(thing.Artist)) continue;
                    window.Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Artist));
                }
            });
        }
        public async Task ShowAlbums()
        {
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.Read("Album"))
                {
                    if (CategoryPanel.Items.Contains(thing.Album)) continue;
                    window.Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Album));
                }
            });
        }
        public async Task ShowPlaylists()
        {
            var x = window.Library.Database.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            await Task.Run(() =>
            {
                if (x.Count == 0) window.Library.CreatePlaylist("Liked");
                foreach (var thing in x)
                {
                    if (CategoryPanel.Items.Contains(thing.Name)) continue;
                    window.Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Name));
                }
            });
        }
        public async Task ShowTracksforArtist(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.ReadTracksForArtist(selectedItem))
                {
                    window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                }
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async Task ShowTracksforAlbum(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.ReadTracksForAlbum(selectedItem))
                {
                    window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, $"{thing.TrackNumber} - {thing.Title}", window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                }
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async Task ShowTracksforPlaylist(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.ReadTracksForPlaylist(selectedItem))
                {
                    window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                }
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        private async void CategoryPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (string)CategoryPanel.SelectedItem;
            if (selectedItem == null) return;
            if (window.CurrentTab == Tab.Artists) await ShowTracksforArtist(selectedItem);
            else if (window.CurrentTab == Tab.Playlists) await ShowTracksforPlaylist(selectedItem);
            else await ShowTracksforAlbum(selectedItem);
        }
        //private void MainWindow_TabChanged(object sender, string e)
        //{
        //    if (previousPage == Menu.Tracks) LeftSide.Width = new GridLength(222);
        //    LoadLibrary();
        //    if (e != null)
        //    {
        //        Thread.Sleep(10);
        //        CategoryPanel.SelectedItem = e;
        //    }
        //    previousPage = window.SelectedMenu;
        //}

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            CategoryPanel.Items.Clear();
            TracksPanel.Items.Clear();
        }

        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private async void Page_Drop(object sender, DragEventArgs e)
        {
            window.Player.Queue.Clear();
            InterfaceUtils.DoDragDrop((string[])e.Data.GetData(DataFormats.FileDrop), window.Player, window.Library);
            await window.Player.PlayAsync();
            var selectedItem = CategoryPanel.SelectedItem;
            LoadLibrary();
            Thread.Sleep(10);
            CategoryPanel.SelectedItem = selectedItem;
        }

        private void QueueAllButton_Click(object sender, RoutedEventArgs e)
        {
            string[] tracks = TracksPanel.Items.OfType<SongEntry>().Select(x => x.FilePath).ToArray(); // avoids firing queue changed event too much
            window.Player.Queue.Add(tracks);
        }

        private async void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            window.Player.Queue.Clear();
            string[] tracks = TracksPanel.Items.OfType<SongEntry>().Select(x => x.FilePath).ToArray(); // avoids firing queue changed event too much
            window.Player.Queue.Add(tracks);
            await window.Player.PlayAsync();
        }

        
    }
}
