using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
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
        public string SelectedItem => CategoryPanel.SelectedItem as string;

        private readonly MainWindow window;
        public LibraryPage(MainWindow window, string search = null)
        {
            this.window = window;
            window.Library.OtherLibraryUpdateOcccured += Library_LibraryChanged;
            window.Library.TracksAdded += Library_TracksAdded;
            window.Library.TracksRemoved += Library_TracksRemoved;
            window.Library.TracksUpdated += Library_TracksUpdated;
            window.Library.PlaylistAdded += Library_PlaylistAdded;
            window.Library.PlaylistRemoved += Library_PlaylistRemoved;
            InitializeComponent();
            LoadLibrary();
            CategoryPanel.Focus();
            if (search != null)
            {
                Thread.Sleep(10);
                CategoryPanel.SelectedItem = search;
                CategoryPanel.ScrollIntoView(search);
            }
        }

        private void Library_PlaylistRemoved(object sender, string e)
        {
            if (window.CurrentTab == Tab.Playlists) CategoryPanel.Items.Remove(e);
        }

        private void Library_PlaylistAdded(object sender, string e)
        {
            if (window.CurrentTab == Tab.Playlists) AddItemToCategoryPanelSorted(e);
        }

        private async void Library_TracksUpdated(object sender, IEnumerable<string> e)
        {
            LibraryEmptyTextBlock.Visibility = Visibility.Collapsed;

            window.NotificationHandler.Add(new Notification { ContentText = "Tracks updated" });
            foreach (var track in e)
            {
                var dbTrack = await window.Library.GetFallbackTrackAsync(track);
                switch (window.CurrentTab)
                {
                    case Tab.Artists:
                        foreach (var artist in dbTrack.Artists)
                        {
                            if (!CategoryPanel.Items.Contains(artist)) AddItemToCategoryPanelSorted(artist);

                            if ((string)CategoryPanel.SelectedItem == artist) await ShowTracksforArtist(artist);
                        }
                        break;
                    case Tab.Albums:
                        var album = dbTrack.Album;

                        if (!CategoryPanel.Items.Contains(album)) AddItemToCategoryPanelSorted(album);

                        if ((string)CategoryPanel.SelectedItem == album) await ShowTracksforAlbum(album);
                        break;
                }

            }
            if (window.CurrentTab == Tab.Tracks)
                await ShowTracks(); // TODO: this is not really ideal, would be nice to dynamically add tracks to the pane
        }

        private void Library_TracksRemoved(object sender, IEnumerable<string> e)
        {
            window.NotificationHandler.Add(new Notification { ContentText = "Tracks removed" });
        }

        private async void Library_TracksAdded(object sender, IEnumerable<string> e)
        {
            window.NotificationHandler.Add(new Notification { ContentText = "Tracks added" });
        }

        private void AddItemToCategoryPanelSorted(string item)
        {
            var items = CategoryPanel.Items.Cast<string>().ToList();
            var index = items.BinarySearch(item);
            if (index < 0) index = ~index;
            CategoryPanel.Items.Insert(index, item);
        }

        private void Library_LibraryChanged(object sender, EventArgs e)
        {
            var selectedItem = CategoryPanel.SelectedItem;
            LoadLibrary();
            Thread.Sleep(10);
            CategoryPanel.SelectedItem = selectedItem;
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
                var tracks = window.Library.GetAllTracks();

                if (tracks.Count <= 0)
                {
                    window.Dispatcher.Invoke(() => LibraryEmptyTextBlock.Visibility = Visibility.Visible);
                    return;
                }
                window.Dispatcher.Invoke(() => LibraryEmptyTextBlock.Visibility = Visibility.Collapsed);

                foreach (var thing in tracks)
                {
                    window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artists, thing.Album, thing.Title, window, window.NotificationHandler, window.Library)));
                    length += thing.Length;
                    if (i % 25 == 0) Thread.Sleep(1); // Apply a slight delay once in a while to let the UI catch up
                    i++;
                }   
            });
            var lengthTimeSpan = new TimeSpan(0, 0, 0, length);
            InfoLabel.Visibility = Visibility.Visible;
            var lengthString = lengthTimeSpan.Days != 0 ? lengthTimeSpan.ToString(@"d\:hh\:mm\:ss") : lengthTimeSpan.ToString(@"hh\:mm\:ss");
            InfoLabel.Text = $@"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {lengthString}";
        }
        public async Task ShowArtists()
        {
            await Task.Run(() =>
            {
                var tracks = window.Library.GetAllTracks();

                if (tracks.Count <= 0)
                {
                    window.Dispatcher.Invoke(() => LibraryEmptyTextBlock.Visibility = Visibility.Visible);
                    return;
                }
                window.Dispatcher.Invoke(() => LibraryEmptyTextBlock.Visibility = Visibility.Collapsed);

                var distinctiveArtists = tracks.SelectMany(x => x.Artists).Distinct().ToList();
                distinctiveArtists.Sort();
                foreach (var thing in distinctiveArtists)
                {
                    window.Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing));
                }
            });
        }
        public async Task ShowAlbums()
        {
            await Task.Run(() =>
            {
                var tracks = window.Library.GetAllTracks("Album");

                if (tracks.Count <= 0)
                {
                    window.Dispatcher.Invoke(() => LibraryEmptyTextBlock.Visibility = Visibility.Visible);
                    return;
                }
                window.Dispatcher.Invoke(() => LibraryEmptyTextBlock.Visibility = Visibility.Collapsed);

                foreach (var thing in tracks)
                {
                    if (CategoryPanel.Items.Contains(thing.Album)) continue;
                    window.Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Album));
                }
            });
        }
        public async Task ShowPlaylists()
        {
            var x = window.Library.Database.GetCollection<DatabasePlaylist>("Playlists").Query().OrderBy("Name").ToList();
            await Task.Run(async () =>
            {
                if (x.Count == 0) await window.Dispatcher.Invoke(async () => await window.Library.CreatePlaylistAsync("Liked", true));
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
            window.BackLog.Add((Tab.Artists, selectedItem));
            await Task.Run(() =>
            {
                var albums = new List<string>();
                var tracks = window.Library.GetTracksForArtist(selectedItem);

                foreach (var thing in tracks)
                {
                    if (!albums.Contains(thing.Album)) albums.Add(thing.Album);
                }

                if (albums.Count <= 1)
                {
                    foreach (var thing in tracks)
                    {
                        window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artists, thing.Album, thing.Title, window, window.NotificationHandler, window.Library)));
                        length += thing.Length;
                    }
                }
                else
                {
                    albums.Sort();
                    foreach (var album in albums)
                    {
                        var tracksInAlbum = tracks.Where(x => x.Album == album);
                        window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new LibraryHeader(window, album, tracksInAlbum.Select(x => x.Path).ToList())));
                        foreach (var thing in tracksInAlbum)
                        {
                            window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artists, thing.Album, thing.Title, window, window.NotificationHandler, window.Library)));
                            length += thing.Length;
                        }
                    }
                }

                //foreach (var thing in window.Library.ReadTracksForArtist(selectedItem))
                //{
                //    window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title, window.Player, window.NotificationHandler, window.Library)));
                //    length += thing.Length;
                //}
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async Task ShowTracksforAlbum(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            window.BackLog.Add((Tab.Albums, selectedItem));
            await Task.Run(() =>
            {
                var discs = new List<int>();
                var tracks = window.Library.GetTracksForAlbum(selectedItem);

                foreach (var thing in tracks)
                {
                    if (!discs.Contains(thing.DiscNumber)) discs.Add(thing.DiscNumber);
                }

                if (discs.Count <= 1)
                {
                    foreach (var thing in tracks)
                    {
                        window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artists, thing.Album, thing.Title, window, window.NotificationHandler, window.Library)));
                        length += thing.Length;
                    }
                }
                else
                {
                    foreach (var disc in discs)
                    {
                        var tracksInDisc = tracks.Where(x => x.DiscNumber == disc);
                        window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new LibraryHeader(window, string.Format(Properties.Resources.LIBRARY_DISCHEADER, disc), tracksInDisc.Select(x => x.Path).ToList())));
                        foreach (var thing in tracksInDisc)
                        {
                            window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artists, thing.Album, thing.Title, window, window.NotificationHandler, window.Library)));
                            length += thing.Length;
                        }
                    }
                }
                
            });
            InfoLabel.Visibility = Visibility.Visible;
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async Task ShowTracksforPlaylist(string selectedItem)
        {
            TracksPanel.Items.Clear();
            int length = 0;
            window.BackLog.Add((Tab.Playlists, selectedItem));
            await Task.Run(() =>
            {
                foreach (var thing in window.Library.GetTracksForPlaylist(selectedItem))
                {
                    window.Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artists, thing.Album, thing.Title, window, window.NotificationHandler, window.Library)));
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
            window.Library.OtherLibraryUpdateOcccured -= Library_LibraryChanged;
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
            //var selectedItem = CategoryPanel.SelectedItem;
            //LoadLibrary();
            //Thread.Sleep(10);
            //CategoryPanel.SelectedItem = selectedItem;
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

        private void TracksPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (var item in e.AddedItems)
            {
                if (item is SongEntry uc)
                    uc.ShowButtons();
            }
            foreach (var item in e.RemovedItems)
            {
                if (item is SongEntry uc)
                    uc.HideButtons();
            }
        }

        private void TracksPanel_LostKeyboardFocus(object sender, System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            if (TracksPanel.IsKeyboardFocusWithin) return;

            foreach (var item in TracksPanel.Items)
            {
                if (item is SongEntry uc)
                    uc.HideButtons();
            }
            TracksPanel.SelectedItem = null;
        }
    }
}
