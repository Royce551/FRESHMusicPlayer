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
using FRESHMusicPlayer.Handlers.Notifications;
using System.Threading;

namespace FRESHMusicPlayer_WPF_UI_Test.Pages.Library
{
    /// <summary>
    /// Interaction logic for LibraryPage.xaml
    /// </summary>
    public partial class LibraryPage : Page
    {
        private List<string> category = new List<string>();
        public LibraryPage()
        {
            InitializeComponent();
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
            NotificationBox box = new NotificationBox(new NotificationInfo("Library is loading", "Please wait", true, true));
            LeftSide.Width = new GridLength(0);
            Splitter.Width = new GridLength(0);
            DetailsPane.Height = new GridLength(0);
            await Task.Run(() =>
            {
                int progress = 0;
                int total = MainWindow.Library.Count;
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Add(box));
                foreach (string thing in MainWindow.Library)
                {
                    Dispatcher.Invoke(() => MainWindow.NotificationHandler.Update(box, new NotificationInfo("Library is loading", $"{progress}/{total}")));
                    Track track = new Track(thing);
                    Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing, track.Artist, track.Album, track.Title)));
                    progress++;
                }
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Remove(box));
            });
        }
        public async void ShowArtists()
        {
            NotificationBox box = new NotificationBox(new NotificationInfo("Library is loading", "Please wait", true, true));
            await Task.Run(() =>
            {
                int progress = 0;
                int total = MainWindow.Library.Count;
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Add(box));
                foreach (string thing in MainWindow.Library)
                {
                    Dispatcher.Invoke(() => MainWindow.NotificationHandler.Update(box, new NotificationInfo("Library is loading", $"{progress}/{total}")));
                    Track track = new Track(thing);
                    if (CategoryPanel.Items.Contains(track.Artist)) continue;
                    Dispatcher.Invoke(() => CategoryPanel.Items.Add(track.Artist));
                    category.Add(track.Artist);
                    progress++;
                }
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Remove(box));
            });
        }
        public async void ShowAlbums()
        {
            NotificationBox box = new NotificationBox(new NotificationInfo("Library is loading", "Please wait", true, true));
            await Task.Run(() =>
            {
                int progress = 0;
                int total = MainWindow.Library.Count;
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Add(box));
                foreach (string thing in MainWindow.Library)
                {
                    Dispatcher.Invoke(() => MainWindow.NotificationHandler.Update(box, new NotificationInfo("Library is loading", $"{progress}/{total}")));
                    Track track = new Track(thing);
                    if (CategoryPanel.Items.Contains(track.Album)) continue;
                    Dispatcher.Invoke(() => CategoryPanel.Items.Add(track.Album));
                    category.Add(track.Album);
                    progress++;
                }
                Dispatcher.Invoke(() => MainWindow.NotificationHandler.Remove(box));
            });
        }
        public async void ShowTracksforArtist(string artist)
        {
            TracksPanel.Items.Clear();
            await Task.Run(() =>
            {
                int progress = 0;
                foreach (string thing in MainWindow.Library)
                {
                    Track track = new Track(thing);
                    if (track.Album != category[progress]) Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing, track.Artist, track.Album, track.Title)));
                    progress++;
                }
            });
        }
        public async void ShowTracksforAlbum(string album)
        {
            TracksPanel.Items.Clear();
            await Task.Run(() =>
            {
                int progress = 0;
                foreach (string thing in MainWindow.Library)
                {
                    Track track = new Track(thing);
                    if (track.Album != category[progress]) Dispatcher.Invoke(() => TracksPanel.Items.Add(new SongEntry(thing, track.Artist, track.Album, track.Title)));
                    progress++;
                }
            });
        }

        private void CategoryPanel_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainWindow.SelectedMenu == SelectedMenus.Artists) ShowTracksforArtist(category[CategoryPanel.SelectedIndex]);
            else ShowTracksforAlbum(category[CategoryPanel.SelectedIndex]);
        }
    }
}
