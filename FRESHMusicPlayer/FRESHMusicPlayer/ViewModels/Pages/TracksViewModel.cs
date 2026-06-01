using Avalonia.Controls;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Views;
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
        public partial ObservableCollection<DatabaseTrackViewModel> Tracks { get; set; }

        [ObservableProperty]
        public partial string FooterText { get; set; }

        [ObservableProperty]
        public partial bool IsLibraryEmpty { get; set; } = false;

        public TracksViewModel()
        {

        }

        public override void AfterPageLoaded()
        {
            MainView.Library.TracksUpdated += Library_TracksUpdated;
            
            _ = UpdateTracksAsync();
        }

        public override void OnNavigatingAway()
        {
            MainView.Library.TracksUpdated -= Library_TracksUpdated;
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
        public partial int Id { get; set; }

        [ObservableProperty]
        public partial string Path { get; set; }

        [ObservableProperty]
        public partial bool HasBeenProcessed { get; set; }

        [ObservableProperty]
        public partial string Title { get; set; }

        public string ArtistAlbumLabel => labelType switch
        {
            ArtistAlbumLabelType.Artist => string.Join(", ", Artists),
            ArtistAlbumLabelType.Album => Album,
            _ => $"{string.Join(", ", Artists)} ・ {Album}"
        };

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ArtistAlbumLabel))]
        public partial string[] Artists { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ArtistAlbumLabel))]
        public partial string Album { get; set; }

        [ObservableProperty]
        public partial string[] Genres { get; set; }

        [ObservableProperty]
        public partial int Year { get; set; }

        [NotifyPropertyChangedFor(nameof(IsTrackNumberPresent))]
        [ObservableProperty]
        public partial int TrackNumber { get; set; }

        [ObservableProperty]
        public partial int TrackTotal { get; set; }

        public bool IsTrackNumberPresent => TrackNumber != 0;

        [ObservableProperty]
        public partial int DiscNumber { get; set; }

        [ObservableProperty]
        public partial int DiscTotal { get; set; }

        [ObservableProperty]
        public partial int Length { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Opacity))]
        public partial bool IsMissing { get; set; } = false;

        public double Opacity => IsMissing ? 0.6 : 1;

        public string[]? TracksInCollection { get; set; }

        public ViewModelBase ViewModel { get; }

        private readonly ArtistAlbumLabelType labelType;
        public DatabaseTrackViewModel(ViewModelBase viewModel, DatabaseTrack track, string[]? tracksInCollection, ArtistAlbumLabelType labelType = ArtistAlbumLabelType.ArtistAndAlbum)
        {
            this.ViewModel = viewModel;
            this.labelType = labelType;

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
                ViewModel.MainView.Notifications.Add(new Handlers.Notification(ViewModel.MainView)
                {
                    ContentText = "The file you tried to play could not be found. If you moved it, you can update the library entry for it.",
                    ButtonText = "Locate file",
                    Type = Handlers.NotificationType.Failure,
                    DisplayAsToast = true,
                    OnButtonClicked = () =>
                    {
                        var topLevel = TopLevel.GetTopLevel(ViewModel.MainView.MainWindow);
                        var files = topLevel.StorageProvider.OpenFilePickerAsync(new Avalonia.Platform.Storage.FilePickerOpenOptions
                        {
                            FileTypeFilter = [FilePickerFileTypes.All] // TODO: do this correctly
                        }).Result;

                        if (files.Count >= 1)
                        {
                            var track = ViewModel.MainView.Library.GetAllTracks().FirstOrDefault(x => x.Id == Id);
                            track.Path = files[0].Path.LocalPath;
                            Path = track.Path;
                            ViewModel.MainView.Library.Database.GetCollection<DatabaseTrack>(Library.TracksCollectionName).Update(track);

                            ViewModel.MainView.Library.TriggerUpdate();
                            _ = ViewModel.MainView.Player.PlayAsync(Path);

                            return true;
                        }

                        return false;
                    }
                });

                return;
            }

            ViewModel.MainView.Player.Queue.Clear();
            if (ViewModel.MainView.Config.AutoQueue && TracksInCollection != null)
            {
                var shuffle = ViewModel.MainView.Player.Queue.Shuffle;

                if (shuffle) ViewModel.MainView.Player.Queue.Shuffle = false;

                var thisTrackIndex = TracksInCollection.ToList().FindIndex(x => x == Path);

                ViewModel.MainView.AddToQueueAndHandleAutoQueue(TracksInCollection);
                ViewModel.MainView.Player.Queue.Position = thisTrackIndex;

                ViewModel.MainView.Player.Queue.Shuffle = shuffle;
            }
            else
            {
                ViewModel.MainView.AddToQueueAndHandleAutoQueue(Path);
            }

            await ViewModel.MainView.Player.PlayAsync();
            if (ViewModel.MainView.Config.AutoQueue) ViewModel.MainView.AutoQueueIsQueued = true;
        }

        public void Enqueue()
        {
            ViewModel.MainView.AddToQueueAndHandleAutoQueue(Path);
        }

        public void PlayNext()
        {
            ViewModel.MainView.Player.Queue.PlayNext(Path);
        }

        public void OpenInFileExplorer()
        {
            SIADLUtilities.OpenURL(System.IO.Path.GetDirectoryName(Path));
        }

        public void RemoveFromLibrary()
        {
            ViewModel.MainView.Library.Remove(Path);
        }

        public async Task PlaylistManagement()
        {
            var window = new PlaylistManagementWindow();
            var vm = new PlaylistManagementViewModel(Path, TracksInCollection)
            {
                MainView = ViewModel.MainView
            };
            window.DataContext = vm;
            await vm.UpdatePlaylistsAsync();

            await window.ShowDialog(ViewModel.MainView.MainWindow);
        }

        public void GoToAlbum() => ViewModel.MainView.NavigateTo(Page.Albums, Album, true);

        public void GoToArtist() => ViewModel.MainView.NavigateTo(Page.Artists, Artists[0], true);

        public override string ToString() => Title;
    }

    public enum ArtistAlbumLabelType
    {
        Artist,
        Album,
        ArtistAndAlbum
    }
}
