using CommunityToolkit.Mvvm.ComponentModel;
using SIADL.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class TracksViewModel : ViewModelBase
    {
        [ObservableProperty]
        private ObservableCollection<DatabaseTrackViewModel> tracks;

        [ObservableProperty]
        private string footerText;

        [ObservableProperty]
        private bool isLibraryEmpty = false;

        public TracksViewModel()
        {

        }

        public override void AfterPageLoaded()
        {
            MainView.Library.TracksUpdated += Library_TracksUpdated;
            

            _ = UpdateTracksAsync();
        }

        private void Library_TracksUpdated(object? sender, IEnumerable<string> e)
        {
            Debug.WriteLine(
                $"tracks updated {string.Join(", ", e)}");
            _ = UpdateTracksAsync();
        }

        public async Task UpdateTracksAsync()
        {
            await Task.Run(() =>
            {
                var libraryTracks = MainView.Library.GetAllTracks();
                IsLibraryEmpty = libraryTracks.Count <= 0;

                var viewModelTracks = libraryTracks.Select(x => new DatabaseTrackViewModel(this, x));
                Tracks = new ObservableCollection<DatabaseTrackViewModel>(viewModelTracks);

                var totalLength = TimeSpan.FromSeconds(Tracks.Sum(x => x.Length));
                FooterText = $"Tracks: {Tracks.Count} • {totalLength}";
            });
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

    public partial class DatabaseTrackViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string path;

        [ObservableProperty]
        private bool hasBeenProcessed;

        [ObservableProperty]
        private string title;

        public string ArtistAlbumLabel => $"{string.Join(", ", Artists)} ・ {Album}";

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ArtistAlbumLabel))]
        private string[] artists;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ArtistAlbumLabel))]
        private string album;

        [ObservableProperty]
        private string[] genres;

        [ObservableProperty]
        private int year;

        [NotifyPropertyChangedFor(nameof(IsTrackNumberPresent))]
        [ObservableProperty]
        private int trackNumber;

        [ObservableProperty]
        private int trackTotal;

        public bool IsTrackNumberPresent => trackNumber != 0;

        [ObservableProperty]
        private int discNumber;

        [ObservableProperty]
        private int discTotal;

        [ObservableProperty]
        private int length;

        private readonly ViewModelBase viewModel;
        public DatabaseTrackViewModel(ViewModelBase viewModel, DatabaseTrack track)
        {
            this.viewModel = viewModel;

            Id = track.Id;
            Path = track.Path;
            HasBeenProcessed = track.HasBeenProcessed;
            Title = track.Title;
            Artists = track.Artists;
            Album = track.Album;
            Genres = track.Genres;
            Year = track.Year;
            TrackNumber = track.TrackNumber;
            TrackTotal = track.TrackTotal;
            DiscNumber = track.DiscNumber;
            DiscTotal = track.DiscTotal;
            Length = track.Length;
        }

        public async void Play()
        {
            viewModel.MainView.Player.Queue.Clear();
            viewModel.MainView.Player.Queue.Add(Path);
            await viewModel.MainView.Player.PlayAsync();
        }

        public void Enqueue()
        {
            viewModel.MainView.Player.Queue.Add(Path);
        }

        public void PlayNext()
        {
            viewModel.MainView.Player.Queue.PlayNext(Path);
        }

        public void OpenInFileExplorer()
        {
            SIADLUtilities.OpenURL(System.IO.Path.GetDirectoryName(Path));
        }

        public void RemoveFromLibrary()
        {
            viewModel.MainView.Library.Remove(Path);
        }

        public void GoToAlbum() => viewModel.MainView.NavigateTo(new AlbumsViewModel(Album));

        public void GoToArtist() => viewModel.MainView.NavigateTo(new ArtistsViewModel(Artists[0]));

        public override string ToString() => Title;
    }
}
