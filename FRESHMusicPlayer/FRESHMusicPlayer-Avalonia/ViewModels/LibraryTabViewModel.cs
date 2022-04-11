using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FRESHMusicPlayer.Properties;
using ReactiveUI;

namespace FRESHMusicPlayer.ViewModels
{
    public class LibraryTabViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindowWm { get; set; }
        private Tab selectedTab;

        public void Initialize(Tab selectedTab, string initialSearch = null)
        {
            this.selectedTab = selectedTab;
            // TODO: library changed event handling
            LoadLibrary();
            if (initialSearch != null)
            {
                Thread.Sleep(10);
                SelectedCategory = initialSearch;
            }
        }

        private string contentInfo;
        public string ContentInfo
        {
            get => contentInfo;
            set => this.RaiseAndSetIfChanged(ref contentInfo, value);
        }

        private int sidebarWidth = 222;
        public int SidebarWidth
        {
            get => sidebarWidth;
            set
            {
                this.RaiseAndSetIfChanged(ref sidebarWidth, value);
            }
        }

        public ObservableCollection<string> CategoryItems { get; set; } = new();
        private string selectedCategory;
        public string SelectedCategory
        {
            get => selectedCategory;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedCategory, value);
                switch (selectedTab)
                {
                    case Tab.Artists:
                        ShowTracksForArtist();
                        break;
                    case Tab.Albums:
                        ShowTracksForAlbum();
                        break;
                    case Tab.Playlists:
                        ShowTracksForPlaylist();
                        break;
                }
            }
        }

        public ObservableCollection<DatabaseTrack> ContentItems { get; set; } = new();

        public void LoadLibrary()
        {
            ContentInfo = null;
            switch (selectedTab)
            {
                case Tab.Tracks:
                    ShowTracks();
                    break;
                case Tab.Artists:
                    ShowArtists();
                    break;
                case Tab.Albums:
                    ShowAlbums();
                    break;
                case Tab.Playlists:
                    ShowPlaylists();
                    break;
            }
        }

        public void ShowTracks()
        {
            SidebarWidth = 0;

            ContentItems = new ObservableCollection<DatabaseTrack>(MainWindowWm.Library.Read());
            this.RaisePropertyChanged(nameof(ContentItems));
            var lengthTimeSpan = TimeSpan.FromSeconds(ContentItems.Sum(x => x.Length));
            var lengthString = lengthTimeSpan.Days != 0 ? lengthTimeSpan.ToString(@"d\:hh\:mm\:ss") : lengthTimeSpan.ToString(@"hh\:mm\:ss");
            ContentInfo = $"{Resources.Tracks}: {ContentItems.Count} ・ {lengthString}";
        }

        public void ShowArtists()
        {
            SidebarWidth = 222;

            foreach (var thing in MainWindowWm.Library.Read("Artist"))
            {
                if (CategoryItems.Contains(thing.Artist)) continue;
                CategoryItems.Add(thing.Artist);
            }
        }

        public void ShowAlbums()
        {
            SidebarWidth = 222;

            foreach (var thing in MainWindowWm.Library.Read("Album"))
            {
                if (CategoryItems.Contains(thing.Album)) continue;
                CategoryItems.Add(thing.Album);
            }
        }

        public void ShowPlaylists()
        {
            SidebarWidth = 222;

            var playlists = MainWindowWm.Library.Database.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToList();
            if (playlists.Count == 0) MainWindowWm.Library.CreatePlaylist("Liked");
            foreach (var thing in playlists)
            {
                if (CategoryItems.Contains(thing.Name)) continue;
                CategoryItems.Add(thing.Name);
            }
        }

        public void ShowTracksForArtist()
        {
            ContentItems = new ObservableCollection<DatabaseTrack>(MainWindowWm.Library.ReadTracksForArtist(selectedCategory));
            this.RaisePropertyChanged(nameof(ContentItems));
            var lengthTimeSpan = TimeSpan.FromSeconds(ContentItems.Sum(x => x.Length));
            var lengthString = lengthTimeSpan.Days != 0 ? lengthTimeSpan.ToString(@"d\:hh\:mm\:ss") : lengthTimeSpan.ToString(@"hh\:mm\:ss");
            ContentInfo = $"{Resources.Tracks}: {ContentItems.Count} ・ {lengthString}";
        }

        public void ShowTracksForAlbum()
        {
            ContentItems = new ObservableCollection<DatabaseTrack>(MainWindowWm.Library.ReadTracksForAlbum(selectedCategory));
            this.RaisePropertyChanged(nameof(ContentItems));
            var lengthTimeSpan = TimeSpan.FromSeconds(ContentItems.Sum(x => x.Length));
            var lengthString = lengthTimeSpan.Days != 0 ? lengthTimeSpan.ToString(@"d\:hh\:mm\:ss") : lengthTimeSpan.ToString(@"hh\:mm\:ss");
            ContentInfo = $"{Resources.Tracks}: {ContentItems.Count} ・ {lengthString}";
        }

        public async void ShowTracksForPlaylist()
        {
            ContentItems.Clear();
            List<DatabaseTrack> playlists = null; // this weird oddity can be removed when library api becomes asyncified
            await Task.Run(() =>
            {
                playlists = MainWindowWm.Library.ReadTracksForPlaylist(selectedCategory);
            });
            ContentItems = new ObservableCollection<DatabaseTrack>(playlists);
            this.RaisePropertyChanged(nameof(ContentItems));
            var lengthTimeSpan = TimeSpan.FromSeconds(ContentItems.Sum(x => x.Length));
            var lengthString = lengthTimeSpan.Days != 0 ? lengthTimeSpan.ToString(@"d\:hh\:mm\:ss") : lengthTimeSpan.ToString(@"hh\:mm\:ss");
            ContentInfo = $"{Resources.Tracks}: {ContentItems.Count} ・ {lengthString}";
        }

        public void EnqueueAllCommand()
        {
            string[] tracks = ContentItems.Select(x => x.Path).ToArray();
            MainWindowWm.Player.Queue.Add(tracks);
        }

        public async void PlayAllCommand()
        {
            MainWindowWm.Player.Queue.Clear();
            string[] tracks = ContentItems.Select(x => x.Path).ToArray();
            MainWindowWm.Player.Queue.Add(tracks);
            await MainWindowWm.Player.PlayAsync();
        }
    }
}
