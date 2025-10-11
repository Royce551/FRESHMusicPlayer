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

namespace FRESHMusicPlayer.ViewModels
{
    public partial class AlbumsViewModel : ViewModelBase
    {
        public ObservableCollection<ObservableRecipient>? Tracks
        {
            get
            {
                if (SelectedAlbum == null) return null;

                var tracksInAlbum = MainView.Library.GetTracksForAlbum(selectedAlbum.Name);

                var discs = tracksInAlbum.Select(x => x.DiscNumber).Distinct().ToList();
                discs.Sort();

                ObservableCollection<ObservableRecipient> items;

                if (discs.Count() <= 1)
                {
                    var viewModelTracks = tracksInAlbum.Select(x => new DatabaseTrackViewModel(this, x));

                    items = new ObservableCollection<ObservableRecipient>(viewModelTracks);  
                }
                else
                {
                    var tempItems = new List<ObservableRecipient>();

                    foreach (var disc in discs)
                    {
                        var tracksInDisc = tracksInAlbum.Where(x => x.DiscNumber == disc);
                        tempItems.Add(new DiscGroupHeaderViewModel(this, disc, [.. tracksInDisc.Select(x => x.Path)]));
                        tempItems.AddRange(tracksInDisc.Select(x => new DatabaseTrackViewModel(this, x)));
                    }

                    items = new ObservableCollection<ObservableRecipient>(tempItems);
                }

                var trackItems = items.OfType<DatabaseTrackViewModel>();
                var totalLength = TimeSpan.FromSeconds(trackItems.Sum(x => x.Length));
                FooterText = $"Tracks: {trackItems.Count()} • {totalLength}";

                return items;
            }
        }

        [ObservableProperty]
        private ObservableCollection<DatabaseAlbumViewModel> albums;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Tracks))]
        private DatabaseAlbumViewModel selectedAlbum;

        [ObservableProperty]
        private string footerText;

        [ObservableProperty]
        private bool isLibraryEmpty = false;

        public AlbumsViewModel()
        {

        }

        private string? initialAlbum = null;
        public AlbumsViewModel(string initialAlbum)
        {
            this.initialAlbum = initialAlbum;
        }

        public override void AfterPageLoaded()
        {
            MainView.Library.TracksUpdated += Library_TracksUpdated;

            _ = UpdateAlbumsAsync();
        }

        public async Task UpdateAlbumsAsync()
        {
            await Task.Run(() =>
            {
                var libraryTracks = MainView.Library.GetAllTracks("Album");
                IsLibraryEmpty = libraryTracks.Count <= 0;

                var viewModelAlbums = libraryTracks.Select(x => new DatabaseAlbumViewModel(this, x.Album)).DistinctBy(x => x.Name).Where(x => !string.IsNullOrWhiteSpace(x.Name));
                Albums = new ObservableCollection<DatabaseAlbumViewModel>(viewModelAlbums);

                if (initialAlbum != null)
                {
                    var foundAlbum = Albums.FirstOrDefault(x => x.Name == initialAlbum);
                    if (foundAlbum != null) SelectedAlbum = foundAlbum;
                }
            });
           
        }

        private void Library_TracksUpdated(object? sender, IEnumerable<string> e)
        {
            if (SelectedAlbum != null) initialAlbum = SelectedAlbum.Name;
            _ = UpdateAlbumsAsync();
        }
        public async void PlayAll()
        {
            MainView.Player.Queue.Clear();
            var filePaths = Tracks.OfType<DatabaseTrackViewModel>().Select(x => x.Path);
            MainView.Player.Queue.Add(filePaths.ToArray());
            await MainView.Player.PlayAsync();
        }

        public void EnqueueAll()
        {
            var filePaths = Tracks.OfType<DatabaseTrackViewModel>().Select(x => x.Path);
            MainView.Player.Queue.Add(filePaths.ToArray());
        }
    }

    public partial class DiscGroupHeaderViewModel : ObservableRecipient
    {
        public string DiscNumString => $"Disc {discNum}";

        private int discNum;

        private string[] tracksInDisc;

        private readonly AlbumsViewModel viewModel;
        public DiscGroupHeaderViewModel(AlbumsViewModel viewModel, int discNum, string[] tracksInDisc)
        {
            this.viewModel = viewModel;
            this.discNum = discNum;
            this.tracksInDisc = tracksInDisc;
        }

        public async void PlayAll()
        {
            viewModel.MainView.Player.Queue.Clear();
            viewModel.MainView.Player.Queue.Add(tracksInDisc);
            await viewModel.MainView.Player.PlayAsync();
        }

        public void EnqueueAll()
        {
            viewModel.MainView.Player.Queue.Clear();
            viewModel.MainView.Player.Queue.Add(tracksInDisc);
        }
    }

    public partial class DatabaseAlbumViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string name;

        public Task<Bitmap?> CoverArt => LoadAlbumArt();

        private readonly AlbumsViewModel viewModel;
        public DatabaseAlbumViewModel(AlbumsViewModel viewModel, string name)
        {
            this.name = name;
            this.viewModel = viewModel;
        }

        public async Task<Bitmap?> LoadAlbumArt()
        {
            var tracks = viewModel.MainView.Library.GetTracksForAlbum(Name);
            if (tracks.Count == 0) return null;

            var track = tracks[0];
            var cover = await AudioBackendFactory.CreateAndLoadBackendAndGetMetadataAsync(track.Path);
            if (cover.metadata != null || cover.metadata?.CoverArt != null)
                return Bitmap.DecodeToHeight(new MemoryStream(cover.metadata.CoverArt), 24);
            else return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
