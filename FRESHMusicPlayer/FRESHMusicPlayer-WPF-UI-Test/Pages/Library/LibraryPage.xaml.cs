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
    public partial class LibraryPage : Page
    {
        private Menu previousPage;

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

        public void LoadLibrary()
        {
            TracksPanel.Items.Clear();
            CategoryPanel.Items.Clear();
            InfoLabel.Visibility = Visibility.Hidden;
            switch (window.SelectedMenu) // all of this stuff is here so that i can avoid copying and pasting the same page thrice, maybe there's a better way?
            {
                case Menu.Tracks:
                    ShowTracks();
                    break;
                case Menu.Artists:
                    ShowArtists();
                    break;
                case Menu.Albums:
                    ShowAlbums();
                    break;
                case Menu.Playlists:
                    ShowPlaylists();
                    break;
            }
        }
        public async void ShowTracks()
        {
            LeftSide.Width = new GridLength(0);
            int length = 0;
            await Task.Run(() =>
            {
                int i = 0;
                foreach (var thing in window.Library.Read())
                {
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                    if (i % 25 == 0) Thread.Sleep(1); // Apply a slight delay once in a while to let the UI catch up
                    i++;
                }   
            });
            var lengthTimeSpan = new TimeSpan(0, 0, 0, length);
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $@"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {Math.Floor(lengthTimeSpan.TotalHours)}:{lengthTimeSpan:mm}:{lengthTimeSpan:ss}";
        }
        public async void ShowArtists()
        {
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.Read("Artist"))
                {
                    if (CategoryPanel.Items.Contains(thing.Artist)) continue;
                    Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Artist));
                }
            });
        }
        public async void ShowAlbums()
        {
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.Read("Album"))
                {
                    if (CategoryPanel.Items.Contains(thing.Album)) continue;
                    Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Album));
                }
            });
        }
        public async void ShowPlaylists()
        {
            var x = window.Library.Library.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            await Task.Run(() =>
            {
                if (x.Count == 0) window.Library.CreatePlaylist("Liked");
                foreach (var thing in x)
                {
                    if (CategoryPanel.Items.Contains(thing.Name)) continue;
                    Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Name));
                }
            });
        }
        public async void ShowTracksforArtist(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.ReadTracksForArtist(selectedItem))
                {
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                }
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async void ShowTracksforAlbum(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.ReadTracksForAlbum(selectedItem))
                {
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, $"{thing.TrackNumber} - {thing.Title}", window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                }
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async void ShowTracksforPlaylist(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.ReadTracksForPlaylist(selectedItem))
                {
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                }
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        private void CategoryPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (string)CategoryPanel.SelectedItem;
            if (selectedItem == null) return;
            if (window.SelectedMenu == Menu.Artists) ShowTracksforArtist(selectedItem);
            else if (window.SelectedMenu == Menu.Playlists) ShowTracksforPlaylist(selectedItem);
            else ShowTracksforAlbum(selectedItem);
        }
        private void MainWindow_TabChanged(object sender, string e)
        {
            if (previousPage == Menu.Tracks) LeftSide.Width = new GridLength(222);
            LoadLibrary();
            if (e != null)
            {
                Thread.Sleep(10);
                CategoryPanel.SelectedItem = e;
            }
            previousPage = window.SelectedMenu;
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

        private void Page_Drop(object sender, DragEventArgs e)
        {
            window.Player.ClearQueue();
            InterfaceUtils.DoDragDrop((string[])e.Data.GetData(DataFormats.FileDrop), window.Player, window.Library);
            window.Player.PlayMusic();
            var selectedItem = CategoryPanel.SelectedItem;
            LoadLibrary();
            Thread.Sleep(10);
            CategoryPanel.SelectedItem = selectedItem;
        }

        private void QueueAllButton_Click(object sender, RoutedEventArgs e)
        {
            string[] tracks = TracksPanel.Items.OfType<SongEntry>().Select(x => x.FilePath).ToArray(); // avoids firing queue changed event too much
            window.Player.AddQueue(tracks);
        }

        private void PlayAllButton_Click(object sender, RoutedEventArgs e)
        {
            window.Player.ClearQueue();
            string[] tracks = TracksPanel.Items.OfType<SongEntry>().Select(x => x.FilePath).ToArray(); // avoids firing queue changed event too much
            window.Player.AddQueue(tracks);
            window.Player.PlayMusic();
        }
    }
}
