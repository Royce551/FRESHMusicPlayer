using FRESHMusicPlayer;
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
using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using System.Threading;
using System.ComponentModel;
using System.Windows.Shell;
using System.Diagnostics;
using LiteDB;
using FRESHMusicPlayer.Utilities;

namespace FRESHMusicPlayer_WPF_UI_Test.Pages.Library
{
    /// <summary>
    /// Interaction logic for LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : Page
    {
        public LibraryPage()
        {
            InitializeComponent();
            LoadLibrary();
            MainWindow.TabChanged += MainWindow_TabChanged;
        }

        public void LoadLibrary()
        {
            switch (MainWindow.SelectedMenu) // all of this stuff is here so that i can avoid copying and pasting the same page thrice, maybe there's a better way?
            {
                case SelectedMenus.Tracks:
                    ShowTracks();
                    break;
                case SelectedMenus.Artists:
                    ShowArtists();
                    break;
                case SelectedMenus.Albums:
                    ShowAlbums();
                    break;
            }
        }
        public async void ShowTracks()
        {
            NotificationBox box = new NotificationBox(new NotificationInfo("Library is loading", "Please wait", true, false));
            LeftSide.Width = new GridLength(0);
            int length = 0;
            await Task.Run(() =>
            {
                int progress = 0;
                int total = MainWindow.Library.Count;              
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Add(box));
                /*foreach (string thing in MainWindow.Library)
                {
                    Dispatcher.Invoke(() => MainWindow.NotificationHandler.Update(box, new NotificationInfo("Library is loading", $"{progress}/{total}", true, false)));
                    Track track = new Track(thing);
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing, track.Artist, track.Album, track.Title)));
                    length += track.Duration;
                    progress++;
                }*/
                    foreach (var thing in MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().OrderBy("Title").ToList())
                    {
                        Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title)));
                        progress++;
                    }        
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Remove(box));
            });
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async void ShowArtists()
        {
            NotificationBox box = new NotificationBox(new NotificationInfo("Library is loading", "Please wait", true, false));
            await Task.Run(() =>
            {
                int progress = 0;
                int total = MainWindow.Library.Count;
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Add(box));
                /*foreach (string thing in MainWindow.Library)
                {
                    Track track = new Track(thing);
                    if (!CategoryPanel.Items.Contains(track.Artist))
                    {
                        Dispatcher.Invoke(() => MainWindow.NotificationHandler.Update(box, new NotificationInfo("Library is loading", $"{progress}/{total}", true, false)));
                        Dispatcher.Invoke(() => CategoryPanel.Items.Add(track.Artist));
                    }
                    progress++;
                }*/
                    foreach (var thing in MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().OrderBy("Artist").ToList())
                    {
                        if (CategoryPanel.Items.Contains(thing.Artist)) continue;
                        Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Artist));
                        progress++;
                    }
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Remove(box));
            });
        }
        public async void ShowAlbums()
        {
            NotificationBox box = new NotificationBox(new NotificationInfo("Library is loading", "Please wait", true, false));
            await Task.Run(() =>
            {
                int progress = 0;
                int total = MainWindow.Library.Count;
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Add(box));
                /*foreach (string thing in MainWindow.Library)
                {
                    Track track = new Track(thing);
                    if (!CategoryPanel.Items.Contains(track.Album))
                    {
                        Dispatcher.Invoke(() => MainWindow.NotificationHandler.Update(box, new NotificationInfo("Library is loading", $"{progress}/{total}", true, false)));
                        Dispatcher.Invoke(() => CategoryPanel.Items.Add(track.Album));
                    }
                    progress++;
                }*/
                    foreach (var thing in MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().OrderBy("Album").ToList())
                    {
                        if (CategoryPanel.Items.Contains(thing.Album)) continue;
                        Dispatcher.Invoke(() => CategoryPanel.Items.Add(thing.Album));
                        progress++;
                    }
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Remove(box));
            });
        }
        public async void ShowTracksforArtist()
        {
            TracksPanel.Items.Clear();
            var selectedItem = (string)CategoryPanel.SelectedItem;
            int length = 0;
            await Task.Run(() =>
            {
                /*foreach (string thing in MainWindow.Library)
                {
                    Track track = new Track(thing);
                    if (track.Artist == selectedItem)
                    {
                        Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing, track.Artist, track.Album, track.Title)));
                        length += track.Duration;
                    }
                }*/
                    foreach (var thing in MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Artist == selectedItem).OrderBy("Title").ToList())
                    {
                        Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title)));
                    }
            });
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }
        public async void ShowTracksforAlbum()
        {
            TracksPanel.Items.Clear();
            var selectedItem = (string)CategoryPanel.SelectedItem;
            int length = 0;
            await Task.Run(() =>
            {
                /*foreach (string thing in MainWindow.Library)
                {
                    Track track = new Track(thing);
                    if (track.Album == selectedItem)
                    {
                        Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing, track.Artist, track.Album, track.Title)));
                        length += track.Duration;
                    }                  
                }*/
                    foreach (var thing in MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Album == selectedItem).OrderBy("TrackNumber").ToList())
                    {
                        Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing.Path, thing.Artist, thing.Album, thing.Title)));
                    }
            });
            InfoLabel.Text = $"{Properties.Resources.MAINWINDOW_TRACKS}: {TracksPanel.Items.Count} ・ {new TimeSpan(0, 0, 0, length):hh\\:mm\\:ss}";
        }

        private void CategoryPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainWindow.SelectedMenu == SelectedMenus.Artists) ShowTracksforArtist();
            else ShowTracksforAlbum();
        }
        private void MainWindow_TabChanged(object sender, EventArgs e)
        {
            TracksPanel.Items.Clear();
            CategoryPanel.Items.Clear();
            LeftSide.Width = new GridLength(222);
            DetailsPane.Height = new GridLength(45);
            LoadLibrary();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.TabChanged -= MainWindow_TabChanged;
            CategoryPanel.Items.Clear();
            TracksPanel.Items.Clear();
        }

        private void Page_DragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Copy;
        }

        private void Page_Drop(object sender, DragEventArgs e)
        {
            string[] tracks = (string[])e.Data.GetData(DataFormats.FileDrop); // TODO: move logic to mainwindow
            MainWindow.Player.AddQueue(tracks);
            DatabaseHandler.ImportSong(tracks);
            MainWindow.Player.PlayMusic();

            MainWindow.Library.Clear();
            MainWindow.Library = DatabaseHandler.ReadSongs();
            LoadLibrary();
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
