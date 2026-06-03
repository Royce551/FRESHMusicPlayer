using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class PlaylistsViewModel : ViewModelBase
    {
        [ObservableProperty]
        public partial ObservableCollection<DatabaseTrackViewModel> Tracks { get; set; } = new();

        private void UpdateTracks()
        {
            if (SelectedPlaylist == null) return;

            var tracksInPlaylist = MainView.Library.GetTracksForPlaylist(SelectedPlaylist.Name);

            var albums = tracksInPlaylist.Select(x => x.Album).Distinct().ToList();
            albums.Sort();

            var viewModelTracks = tracksInPlaylist.Select(x => new DatabaseTrackViewModel(this, x, tracksInPlaylist.Select(y => y.Path).ToArray(), ArtistAlbumLabelType.ArtistAndAlbum)).ToArray();

            var totalLength = TimeSpan.FromSeconds(viewModelTracks.Sum(x => x.Length));
            FooterText = $"Tracks: {viewModelTracks.Count()} • {totalLength}";

            Tracks = new ObservableCollection<DatabaseTrackViewModel>(viewModelTracks);
            Tracks.CollectionChanged += Tracks_CollectionChanged;
        }

        private void Tracks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                var playlist = MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Query().ToEnumerable().FirstOrDefault(x => x.Name == SelectedPlaylist.Name);

                if (playlist == null || Tracks is null) return;

                playlist.Tracks = Tracks.Select(x => x.Id).ToList();

                MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Update(playlist);
            }
        }

        [ObservableProperty]
        public partial ObservableCollection<DatabasePlaylistViewModel> Playlists { get; set; }

        private DatabasePlaylistViewModel selectedPlaylist;

        public DatabasePlaylistViewModel SelectedPlaylist
        {
            get => selectedPlaylist;
            set
            {
                SetProperty(ref selectedPlaylist, value);
                UpdateTracks();
            }
        }

        [ObservableProperty]
        public partial string FooterText { get; set; }

        [ObservableProperty]
        public partial bool IsLibraryEmpty { get; set; } = false;

        public PlaylistsViewModel()
        {

        }

        private string? initialPlaylist = null;
        public PlaylistsViewModel(string? initialPlaylist)
        {
            this.initialPlaylist = initialPlaylist;
        }

        public override void AfterPageLoaded()
        {
            MainView.Library.TracksUpdated += Library_TracksUpdated;
            _ = UpdateAlbumsAsync();
        }

        public override void OnNavigatingAway()
        {
            MainView.Library.TracksUpdated -= Library_TracksUpdated;
        }

        public async Task UpdateAlbumsAsync()
        {
            await Task.Run(() =>
            {
                var libraryTracks = MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Query().OrderBy("Name").ToList();
                IsLibraryEmpty = libraryTracks.Count <= 0;

                var viewModelPlaylists = libraryTracks.Select(x => new DatabasePlaylistViewModel(this, x.Name, x.CoverArt)).DistinctBy(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderBy(x => x.Name);
                Playlists = new ObservableCollection<DatabasePlaylistViewModel>(viewModelPlaylists);
            });
            Dispatcher.UIThread.Invoke(() =>
            {
                if (initialPlaylist != null)
                {
                    var foundPlaylist = Playlists.FirstOrDefault(x => x.Name == initialPlaylist);
                    if (foundPlaylist != null)
                    {
                        SelectedPlaylist = foundPlaylist;
                        initialPlaylist = null;
                    }
                }
            }, DispatcherPriority.ApplicationIdle);
        }

        [ObservableProperty]
        public partial bool PlaylistCreateMode { get; set; } = false;

        [ObservableProperty]
        public partial string? PlaylistName { get; set; }

        public void OpenPlaylistCreation() => PlaylistCreateMode = true;

        public void ClosePlaylistCreation() => PlaylistCreateMode = false;

        public async Task CreateNewPlaylist()
        {
            if (string.IsNullOrWhiteSpace(PlaylistName)) return;

            await MainView.Library.CreatePlaylistAsync(PlaylistName, false);
            _ = UpdateAlbumsAsync();

            PlaylistName = null;
            PlaylistCreateMode = false;
        }

        public void ImportPlaylist()
        {
            PlaylistCreateMode = false;
        }

        private void Library_TracksUpdated(object? sender, IEnumerable<string> e)
        {
            if (SelectedPlaylist != null) initialPlaylist = SelectedPlaylist.Name;
            _ = UpdateAlbumsAsync();
        }
        public async void PlayAll()
        {
            MainView.Player.Queue.Clear();
            var filePaths = Tracks.OfType<DatabaseTrackViewModel>().Select(x => x.Path);
            MainView.AddToQueueAndHandleAutoQueue(filePaths.ToArray());
            await MainView.Player.PlayAsync();
        }

        public void EnqueueAll()
        {
            var filePaths = Tracks.OfType<DatabaseTrackViewModel>().Select(x => x.Path);
            MainView.AddToQueueAndHandleAutoQueue(filePaths.ToArray());
        }
    }

    public partial class DatabasePlaylistViewModel : ObservableRecipient
    {
        [ObservableProperty]
        public partial string Name { get; set; }

        public Task<Bitmap?> CoverArt => LoadArtistArt();

        public ViewModelBase ViewModel { get; }

        private readonly byte[]? coverArt;
        public DatabasePlaylistViewModel(ViewModelBase viewModel, string name, byte[] coverArt)
        {
            Name = name;
            this.ViewModel = viewModel;
            this.coverArt = coverArt;
        }

        public async Task<Bitmap?> LoadArtistArt()
        {
            if (coverArt != null) return Bitmap.DecodeToHeight(new MemoryStream(coverArt), 48);

            var firstTrackInPlaylist = ViewModel.MainView.Library.GetTracksForPlaylist(Name).FirstOrDefault();
            // TODO: claen this up
            return firstTrackInPlaylist != null ? Bitmap.DecodeToHeight(new MemoryStream(await ViewModel.MainView.Library.GetCoverArtThumbnail(firstTrackInPlaylist.Album)), 48) : null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}