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
        public ObservableCollection<DatabaseTrackViewModel>? Tracks
        {
            get
            {
                if (SelectedAlbum == null) return null;

                var tracksInAlbum = MainView.Library.GetTracksForAlbum(selectedAlbum.Name);
                var viewModelTracks = tracksInAlbum.Select(x => new DatabaseTrackViewModel(this, x));

                var tracks = new ObservableCollection<DatabaseTrackViewModel>(viewModelTracks);

                var totalLength = TimeSpan.FromSeconds(tracks.Sum(x => x.Length));
                FooterText = $"Tracks: {tracks.Count} • {totalLength}";

                return tracks;
            }
        }

        [ObservableProperty]
        private ObservableCollection<DatabaseAlbumViewModel> albums;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Tracks))]
        private DatabaseAlbumViewModel selectedAlbum;

        [ObservableProperty]
        private string footerText;

        public AlbumsViewModel()
        {

        }

        public override async void AfterPageLoaded()
        {
            var libraryTracks = MainView.Library.GetAllTracks("Album");

            var viewModelAlbums = libraryTracks.Select(x => new DatabaseAlbumViewModel(this, x.Album)).DistinctBy(x => x.Name);
            Albums = new ObservableCollection<DatabaseAlbumViewModel>(viewModelAlbums);

            //var viewModelTracks = libraryTracks.Select(x => new DatabaseTrackViewModel(this, x));
            //Tracks = new ObservableCollection<DatabaseTrackViewModel>(viewModelTracks);

            //var totalLength = TimeSpan.FromSeconds(Tracks.Sum(x => x.Length));
            //FooterText = $"Tracks: {Tracks.Count} • {totalLength}";
        }

        public async void PlayAll()
        {
            MainView.Player.Queue.Clear();
            var filePaths = Tracks.Select(x => x.Path);
            MainView.Player.Queue.Add(filePaths.ToArray());
            await MainView.Player.PlayAsync();
        }

        public void EnqueueAll()
        {
            var filePaths = Tracks.Select(x => x.Path);
            MainView.Player.Queue.Add(filePaths.ToArray());
        }
    }

    public partial class DatabaseAlbumViewModel : ViewModelBase
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
            if (cover.metadata.CoverArt != null)
                return Bitmap.DecodeToHeight(new MemoryStream(cover.metadata.CoverArt), 20);
            else return null;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
