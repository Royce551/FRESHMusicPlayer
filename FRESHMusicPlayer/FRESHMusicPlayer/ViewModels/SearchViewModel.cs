using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class SearchViewModel : ViewModelBase
    {
        public Task<ObservableCollection<DatabaseTrackViewModel>>? Tracks => GetTracksAsync();

        private async Task<ObservableCollection<DatabaseTrackViewModel>>? GetTracksAsync()
        {
            if (Search is null || string.IsNullOrEmpty(Search) || MainView is null) return null!;

            var tracks = await Task.Run(() =>
            {
                var tracks = MainView.Library.Database.GetCollection<DatabaseTrack>(Library.TracksCollectionName)
                .Query()
                .Where(x => x.Title.Contains(Search, StringComparison.CurrentCultureIgnoreCase) || x.Artists.Any(y => y.Contains(Search, StringComparison.CurrentCultureIgnoreCase)) || x.Album.Contains(Search, StringComparison.CurrentCultureIgnoreCase))
                .OrderBy("Title")
                .ToList();

                var totalLength = TimeSpan.FromSeconds(tracks.Sum(x => x.Length));
                FooterText = $"Tracks: {tracks.Count} • {totalLength}";

                return tracks.Select(x => new DatabaseTrackViewModel(this, x, [.. tracks.Select(x => x.Path)]));
            });
            return new ObservableCollection<DatabaseTrackViewModel>(tracks);
        }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Tracks))]
        private string? search;

        [ObservableProperty]
        private string footerText;

        public async Task PlayAll()
        {
            MainView.Player.Queue.Clear();
            var filePaths = (await GetTracksAsync()).Select(x => x.Path);
            MainView.AddToQueueAndHandleAutoQueue(filePaths.ToArray());
            await MainView.Player.PlayAsync();
        }

        public async Task EnqueueAll()
        {
            var filePaths = (await GetTracksAsync()).Select(x => x.Path);
            MainView.AddToQueueAndHandleAutoQueue(filePaths.ToArray());
        }
    }
}
