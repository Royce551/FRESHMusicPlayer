using CommunityToolkit.Mvvm.ComponentModel;
using SIADL.Avalonia;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public TracksViewModel()
        {

        }

        public override void AfterPageLoaded()
        {
            var libraryTracks = MainView.Library.GetAllTracks();
            var viewModelTracks = libraryTracks.Select(x => new DatabaseTrackViewModel(this, x));
            Tracks = new ObservableCollection<DatabaseTrackViewModel>(viewModelTracks);
        }

        public void Test()
        {

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

        [ObservableProperty]
        private int trackNumber;

        [ObservableProperty]
        private int trackTotal;

        [ObservableProperty]
        private int discNumber;

        [ObservableProperty]
        private int discTotal;

        [ObservableProperty]
        private int length;

        private readonly TracksViewModel viewModel;
        public DatabaseTrackViewModel(TracksViewModel viewModel, DatabaseTrack track)
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
    }
}
