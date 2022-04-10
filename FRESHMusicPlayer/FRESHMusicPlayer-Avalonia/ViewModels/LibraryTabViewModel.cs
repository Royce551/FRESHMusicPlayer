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

        public void Initialize(Tab selectedTab, string initialSearch = null)
        {
            // TODO: library changed event handling
            LoadLibrary(selectedTab);
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
                selectedCategory = value;
            }
        }

        public ObservableCollection<DatabaseTrack> ContentItems { get; set; } = new();

        public async void LoadLibrary(Tab selectedTab)
        {
            ContentInfo = null;
            switch (selectedTab)
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
            SidebarWidth = 0;

            ContentItems = new ObservableCollection<DatabaseTrack>(MainWindowWm.Library.Read());
            this.RaisePropertyChanged(nameof(ContentItems));
            var lengthTimeSpan = TimeSpan.FromSeconds(ContentItems.Sum(x => x.Length));
            var lengthString = lengthTimeSpan.Days != 0 ? lengthTimeSpan.ToString(@"d\:hh\:mm\:ss") : lengthTimeSpan.ToString(@"hh\:mm\:ss");
            ContentInfo = $"{Resources.Tracks}: {ContentItems.Count} ・ {lengthString}";
        }

        public async Task ShowArtists()
        {

        }

        public async Task ShowAlbums()
        {

        }

        public async Task ShowPlaylists()
        {

        }
    }
}
