using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Backends;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class ArtistsViewModel : ViewModelBase
    {
        public ObservableCollection<ObservableRecipient>? Tracks
        {
            get
            {
                if (SelectedArtist == null) return null;

                var tracksInArtist = MainView.Library.GetTracksForArtist(SelectedArtist.Name);

                var albums = tracksInArtist.Select(x => x.Album).Distinct().ToList();
                albums.Sort();

                ObservableCollection<ObservableRecipient> items;

                if (albums.Count() <= 1)
                {
                    var viewModelTracks = tracksInArtist.Select(x => new DatabaseTrackViewModel(this, x, tracksInArtist.Select(y => y.Path).ToArray()));

                    items = new ObservableCollection<ObservableRecipient>(viewModelTracks);
                }
                else
                {
                    var tempItems = new List<ObservableRecipient>();

                    foreach (var album in albums)
                    {
                        var tracksInAlbum = tracksInArtist.Where(x => x.Album == album);
                        tempItems.Add(new AlbumGroupHeaderViewModel(this, album, [.. tracksInAlbum.Select(x => x.Path)]));
                        tempItems.AddRange(tracksInAlbum.Select(x => new DatabaseTrackViewModel(this, x, null)));
                    }

                    items = new ObservableCollection<ObservableRecipient>(tempItems);

                    var otherTracks = items.OfType<DatabaseTrackViewModel>();
                    foreach (var item in otherTracks)
                    {
                        item.TracksInCollection = otherTracks.Select(x => x.Path).ToArray();
                    }
                }

                var trackItems = items.OfType<DatabaseTrackViewModel>();
                var totalLength = TimeSpan.FromSeconds(trackItems.Sum(x => x.Length));
                FooterText = $"Tracks: {trackItems.Count()} • {totalLength}";

                return items;
            }
        }

        [ObservableProperty]
        private ObservableCollection<DatabaseArtistViewModel> artists;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Tracks))]
        private DatabaseArtistViewModel selectedArtist;

        [ObservableProperty]
        private string footerText;

        [ObservableProperty]
        private bool isLibraryEmpty = false;

        public ArtistsViewModel()
        {

        }

        private string? initialArtist = null;
        public ArtistsViewModel(string? initialArtist)
        {
            this.initialArtist = initialArtist;
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
                var libraryTracks = MainView.Library.GetAllTracks("Artist");
                IsLibraryEmpty = libraryTracks.Count <= 0;

                var viewModelArtists = libraryTracks.SelectMany(y => y.Artists).Select(x => new DatabaseArtistViewModel(this, x)).DistinctBy(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x.Name)).OrderBy(x => x.Name);
                Artists = new ObservableCollection<DatabaseArtistViewModel>(viewModelArtists);

                if (initialArtist != null)
                {
                    var foundArtist = Artists.FirstOrDefault(x => x.Name == initialArtist);
                    if (foundArtist != null)
                    {
                        SelectedArtist = foundArtist;
                        initialArtist = null;
                    }
                }
            });

        }

        private void Library_TracksUpdated(object? sender, IEnumerable<string> e)
        {
            if (SelectedArtist != null) initialArtist = SelectedArtist.Name;
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

    public partial class AlbumGroupHeaderViewModel : ObservableRecipient
    {
        public string Album { get; }

        public Task<Bitmap?> CoverArt => LoadAlbumGroupArt();

        private string[] tracksInAlbum;

        private readonly ArtistsViewModel viewModel;
        public AlbumGroupHeaderViewModel(ArtistsViewModel viewModel, string album, string[] tracksInAlbum)
        {
            Album = album;
            this.viewModel = viewModel;        
            this.tracksInAlbum = tracksInAlbum;
        }

        public async void PlayAll()
        {
            viewModel.MainView.Player.Queue.Clear();
            viewModel.MainView.Player.Queue.Add(tracksInAlbum);
            await viewModel.MainView.Player.PlayAsync();
        }

        public void EnqueueAll()
        {
            viewModel.MainView.Player.Queue.Clear();
            viewModel.MainView.Player.Queue.Add(tracksInAlbum);
        }

        public async Task<Bitmap?> LoadAlbumGroupArt()
        {
            var tracks = viewModel.MainView.Library.GetTracksForAlbum(Album);
            if (tracks.Count == 0) return null;

            var track = tracks[0];
            var cover = await AudioBackendFactory.CreateAndLoadBackendAndGetMetadataAsync(track.Path);
            if (cover.metadata != null || cover.metadata?.CoverArt != null)
                return Bitmap.DecodeToHeight(new MemoryStream(cover.metadata.CoverArt), 20);
            else return null;
        }
    }

    public partial class DatabaseArtistViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string name;

        public Task<Bitmap?> CoverArt => LoadArtistArt();

        private readonly ArtistsViewModel viewModel;
        public DatabaseArtistViewModel(ArtistsViewModel viewModel, string name)
        {
            this.name = name;
            this.viewModel = viewModel;
        }

        public async Task<Bitmap?> LoadArtistArt()
        {
            return null; // TODO: i would like to have artist thumbnails, but it requires an online data source. maybe something for later
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
