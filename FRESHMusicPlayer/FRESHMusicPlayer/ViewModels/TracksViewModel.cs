using Avalonia.Controls;
using Avalonia.Platform.Storage;
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

                var viewModelTracks = libraryTracks.Select(x => new DatabaseTrackViewModel(this, x, libraryTracks.Select(y => y.Path).ToArray()));
                Tracks = new ObservableCollection<DatabaseTrackViewModel>(viewModelTracks);

                var totalLength = TimeSpan.FromSeconds(Tracks.Sum(x => x.Length));
                FooterText = $"Tracks: {Tracks.Count} • {totalLength}";
            });
        }

        public async void PlayAll()
        {
            MainView.Player.Queue.Clear();
            var filePaths = Tracks.Select(x => x.Path);
            MainView.AddToQueueAndHandleAutoQueue(filePaths.ToArray());
            await MainView.Player.PlayAsync();
        }

        public void EnqueueAll()
        {
            var filePaths = Tracks.Select(x => x.Path);
            MainView.AddToQueueAndHandleAutoQueue(filePaths.ToArray());
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

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Opacity))]
        private bool isMissing = false;

        public double Opacity => IsMissing ? 0.6 : 1;

        public string[]? TracksInCollection { get; set; }

        private readonly ViewModelBase viewModel;
        public DatabaseTrackViewModel(ViewModelBase viewModel, DatabaseTrack track, string[]? tracksInCollection)
        {
            this.viewModel = viewModel;

            TracksInCollection = tracksInCollection;
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

            // TODO: i want to get this information from the backend, but this logic from fmp12 is ok for now
            if (!Path.StartsWith("http") && !File.Exists(Path)) IsMissing = true;
        }

        public async void Play()
        {
            if (IsMissing)
            {
                viewModel.MainView.Notifications.Add(new Handlers.Notification(viewModel.MainView)
                {
                    ContentText = "The file you tried to play could not be found. If you moved it, you can update the library entry for it.",
                    ButtonText = "Locate file",
                    Type = Handlers.NotificationType.Failure,
                    DisplayAsToast = true,
                    OnButtonClicked = () =>
                    {
                        var topLevel = TopLevel.GetTopLevel(viewModel.MainView.MainWindow);
                        var files = topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                        {
                            FileTypeFilter = [FilePickerFileTypes.All] // TODO: do this correctly
                        }).Result;

                        if (files.Count >= 1)
                        {
                            var track = viewModel.MainView.Library.GetAllTracks().FirstOrDefault(x => x.Id == Id);
                            track.Path = files[0].Path.LocalPath;
                            Path = track.Path;
                            viewModel.MainView.Library.Database.GetCollection<DatabaseTrack>(Library.TracksCollectionName).Update(track);

                            viewModel.MainView.Library.TriggerUpdate();
                            _ = viewModel.MainView.Player.PlayAsync(Path);

                            return true;
                        }

                        return false;
                    }
                });

                return;
            }

            viewModel.MainView.Player.Queue.Clear();
            if (viewModel.MainView.Config.AutoQueue && TracksInCollection != null)
            {
                var shuffle = viewModel.MainView.Player.Queue.Shuffle;

                if (shuffle) viewModel.MainView.Player.Queue.Shuffle = false;

                var thisTrackIndex = TracksInCollection.ToList().FindIndex(x => x == Path);

                viewModel.MainView.AddToQueueAndHandleAutoQueue(TracksInCollection);
                viewModel.MainView.Player.Queue.Position = thisTrackIndex;

                viewModel.MainView.Player.Queue.Shuffle = shuffle;
            }
            else
            {
                viewModel.MainView.AddToQueueAndHandleAutoQueue(Path);
            }

            await viewModel.MainView.Player.PlayAsync();
            if (viewModel.MainView.Config.AutoQueue) viewModel.MainView.AutoQueueIsQueued = true;
        }

        public void Enqueue()
        {
            viewModel.MainView.AddToQueueAndHandleAutoQueue(Path);
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
