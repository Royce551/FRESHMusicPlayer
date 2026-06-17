using Avalonia.Media.Imaging;
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
    public partial class PlaylistManagementViewModel(string path, string[]? collectionPaths = null) : ViewModelBase
    {
        [ObservableProperty]
        public partial ObservableCollection<TrackManagementDatabasePlaylistViewModel>? Playlists { get; set; }

        [ObservableProperty]
        public partial string? PromptText { get; set; } = $"What do you want to do with \"{Path.GetFileName(path)}\"?";

        [ObservableProperty]
        public partial bool IsTrackInLibrary { get; set; }

        public async Task UpdatePlaylistsAsync()
        {
            if (MainView is null) return;

            MainView.IsShiftHeld = false; // covers the case that a shift shortcut was used to open this

            await Task.Run(() =>
            {
                IsTrackInLibrary = MainView.Library.GetAllTracks().Any(x => x.Path == path);

                var playlists = MainView.Library.Database.GetCollection<DatabasePlaylist>(Library.PlaylistsCollectionName).Query().OrderBy("Name").ToList();

                Playlists = new ObservableCollection<TrackManagementDatabasePlaylistViewModel>([.. playlists.Select(x => new TrackManagementDatabasePlaylistViewModel(this, x.Name, x.CoverArt, path, collectionPaths))]);
            });
        }

        public async Task NewPlaylist() // note: copy paste from playlistsviewmodel
        {
            var input = new TextInputDialog("Playlist name");
            var name = await input.ShowDialog<string?>(MainView.MainWindow);
            if (string.IsNullOrWhiteSpace(name)) return; // cancel case

            await MainView.Library.CreatePlaylistAsync(name, false);
            _ = UpdatePlaylistsAsync();
        }
    }

    public class TrackManagementDatabasePlaylistViewModel(ViewModelBase viewModel, string name, byte[] coverArt, string path, string[]? collectionPaths = null) : DatabasePlaylistViewModel(viewModel, name, coverArt)
    {
        public bool IsInPlaylist
        {
            get
            {
                foreach (var track in ViewModel.MainView.Library.GetTracksForPlaylist(Name))
                {
                    if (track.Path == path) return true;
                }
                return false;
            }
        }

        public bool IsAddCollectionAvailable => collectionPaths != null;

        public async Task Add()
        {
            await ViewModel.MainView.Library.AddTrackToPlaylistAsync(Name, path);

            OnPropertyChanged(nameof(IsInPlaylist));
        }
        public async Task AddCollection()
        {
            if (collectionPaths is null) return;

            foreach (var cPath in collectionPaths)
            {
                await ViewModel.MainView.Library.AddTrackToPlaylistAsync(Name, cPath);
            }

            OnPropertyChanged(nameof(IsInPlaylist));
        }

        public void Remove()
        {
            ViewModel.MainView.Library.RemoveTrackFromPlaylist(Name, path);

            OnPropertyChanged(nameof(IsInPlaylist));
        }
    }
}
