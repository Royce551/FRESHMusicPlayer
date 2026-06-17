using ATL.Playlist;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class PlaylistsViewModel : ViewModelBase
    {
        [ObservableProperty]
        public partial AvaloniaList<DatabaseTrackViewModel> Tracks { get; set; } = new();

        private void UpdateTracks()
        {
            if (SelectedPlaylist == null) return;
            PlaylistEditMode = false;

            var tracksInPlaylist = MainView.Library.GetTracksForPlaylist(SelectedPlaylist.Name);

            var albums = tracksInPlaylist.Select(x => x.Album).Distinct().ToList();
            albums.Sort();

            var viewModelTracks = tracksInPlaylist.Select(x => new DatabaseTrackViewModel(this, x, tracksInPlaylist.Select(y => y.Path).ToArray(), ArtistAlbumLabelType.ArtistAndAlbum)).ToArray();

            var totalLength = TimeSpan.FromSeconds(viewModelTracks.Sum(x => x.Length));
            FooterText = $"Tracks: {viewModelTracks.Count()} • {totalLength}";

            Tracks = new AvaloniaList<DatabaseTrackViewModel>(viewModelTracks);
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
                OnPropertyChanged(nameof(IsRemovePlaylistCoverArtAvailable));
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
            _ = UpdatePlaylistsAsync();
        }

        public override void OnNavigatingAway()
        {
            MainView.Library.TracksUpdated -= Library_TracksUpdated;
        }

        public async Task UpdatePlaylistsAsync()
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
            var input = new TextInputDialog("Playlist name");
            var name = await input.ShowDialog<string?>(MainView.MainWindow);
            if (string.IsNullOrWhiteSpace(name)) return; // cancel case

            await MainView.Library.CreatePlaylistAsync(name, false);
            _ = UpdatePlaylistsAsync();

            PlaylistCreateMode = false;
        }

        public async Task ImportNewPlaylist()
        {
            var topLevel = TopLevel.GetTopLevel(MainView.MainWindow);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [FilePickerFileTypes.All] // TODO: fix
            });

            if (files.Count >= 1)
            {
                var reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(files[0].Path.LocalPath);

                foreach (var path in reader.FilePaths)
                {
                    if (!File.Exists(path))
                    {
                        MainView.Notifications.Add(new Handlers.Notification(MainView)
                        {
                            ContentText = $"{path} was excluded from the playlist because it could not be found.",
                            Type = Handlers.NotificationType.Failure,
                            DisplayAsToast = true
                        });
                        continue;
                    }
                    await MainView.Library.AddTrackToPlaylistAsync(Path.GetFileNameWithoutExtension(files[0].Path.LocalPath), path);
                }
            }
            _ = UpdatePlaylistsAsync();

            PlaylistCreateMode = false;
        }

        private void Library_TracksUpdated(object? sender, IEnumerable<string> e)
        {
            if (SelectedPlaylist != null) initialPlaylist = SelectedPlaylist.Name;
            _ = UpdatePlaylistsAsync();
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

        [ObservableProperty]
        public partial bool PlaylistEditMode { get; set; } = false;

        public void OpenPlaylistEdit() => PlaylistEditMode = true;
        public void ClosePlaylistEdit() => PlaylistEditMode = false;

        public async Task RenamePlaylist()
        {
            var input = new TextInputDialog("Playlist name", SelectedPlaylist.Name);
            var name = await input.ShowDialog<string?>(MainView.MainWindow);
            if (string.IsNullOrWhiteSpace(name)) return; // cancel case

            var playlist = MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Query().ToEnumerable().FirstOrDefault(x => x.Name == SelectedPlaylist.Name)!;
            playlist.Name = name;
            MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Update(playlist);

            SelectedPlaylist.Name = name;
        }

        public async Task DeletePlaylist()
        {
            if (!PlaylistEditMode) return;

            MainView.Library.DeletePlaylist(SelectedPlaylist.Name);
            await UpdatePlaylistsAsync();

            SelectedPlaylist = Playlists.FirstOrDefault()!; // null is ok

            PlaylistEditMode = false;
        }

        public async Task ChangePlaylistCoverArt()
        {
            var topLevel = TopLevel.GetTopLevel(MainView.MainWindow);
            var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                AllowMultiple = false,
                FileTypeFilter = [FilePickerFileTypes.ImageAll]
            });

            if (files.Count >= 1)
            {
                var playlist = MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Query().ToEnumerable().FirstOrDefault(x => x.Name == SelectedPlaylist.Name)!;
                playlist.CoverArt = await File.ReadAllBytesAsync(files[0].Path.LocalPath);
                MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Update(playlist);

                initialPlaylist = SelectedPlaylist.Name;
                _ = UpdatePlaylistsAsync();
            }
        }

        public bool IsRemovePlaylistCoverArtAvailable
        {
            get
            {
                if (MainView is null || SelectedPlaylist is null) return false;

                var playlist = MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Query().ToEnumerable().FirstOrDefault(x => x.Name == SelectedPlaylist.Name);
                return playlist != null && playlist.CoverArt != null;
            }
        }

        public void RemovePlaylistCoverArt()
        {
            var playlist = MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Query().ToEnumerable().FirstOrDefault(x => x.Name == SelectedPlaylist.Name)!;
            playlist.CoverArt = null;
            MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Update(playlist);
            initialPlaylist = SelectedPlaylist.Name;
            _ = UpdatePlaylistsAsync();
        }

        public async Task ExportPlaylist()
        {
            var topLevel = TopLevel.GetTopLevel(MainView.MainWindow);

            var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                SuggestedFileName = SelectedPlaylist.Name,
                DefaultExtension = "m3u8",
            });
            if (file is not null)
            {
                var tracks = MainView.Library.GetTracksForPlaylist(SelectedPlaylist.Name);

                var playlist = PlaylistIOFactory.GetInstance().GetPlaylistIO(file.Path.LocalPath);
                var pathsToWrite = new List<string>();
                foreach (var track in tracks)
                {
                    pathsToWrite.Add(track.Path);
                }
                playlist.FilePaths = pathsToWrite;
            }
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