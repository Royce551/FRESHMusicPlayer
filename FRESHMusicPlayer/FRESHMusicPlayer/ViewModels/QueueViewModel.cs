using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class QueueViewModel : ViewModelBase
    {
        public override void AfterPageLoaded()
        {
            MainView.Player.Queue.QueueChanged += Queue_QueueChanged;
            MainView.Player.SongChanged += Player_SongChanged;
            MainView.Player.SongStopped += Player_SongStopped;
            MainView.ProgressTimer.Tick += ProgressTimer_Tick;

            _ = UpdateQueueAsync();

            base.AfterPageLoaded();
        }

        private void ProgressTimer_Tick(object? sender, EventArgs e)
        {
            if (Tracks is null) return;

            var totalLength = new TimeSpan();
            for (int i = 0; i < Tracks.Count; i++)
            {
                if ((i + 1) < MainView.Player.Queue.Position) continue;
                var track = Tracks[i];
                if (i != (Tracks.Count - 1)) totalLength += TimeSpan.FromSeconds(track.Length);
            }
            totalLength += (MainView.Player.TotalTime - MainView.Player.CurrentTime);
            EndsAt = $"Ends at {DateTime.Now + totalLength:t}";
        }

        [ObservableProperty]
        private string? endsAt = null;

        private void Player_SongStopped(object? sender, PlaybackStoppedEventArgs e) => _ = UpdateQueueAsync();

        private void Player_SongChanged(object? sender, EventArgs e) => _ = UpdateQueueAsync();

        public void UpdateNowPlayingState()
        {
            if (Tracks is null) return;

            foreach (var track in Tracks)
                track.Update();
        }

        [ObservableProperty]
        private ObservableCollection<QueueTrackViewModel> tracks = default!;

        private void Queue_QueueChanged(object? sender, EventArgs e) => _ = UpdateQueueAsync();

        public async Task UpdateQueueAsync()
        {
            if (!MainView.Player.FileLoaded) return;

            if (Tracks != null) Tracks.CollectionChanged -= Tracks_CollectionChanged;
            await Task.Run(async () =>
            {
                try
                {
                    var queueItems = new List<QueueTrackViewModel>();


                    int position = 1;
                    foreach (var track in MainView.Player.Queue.Queue)
                    {
                        var correspondingLibraryEntry = await MainView.Library.GetFallbackTrackAsync(track);

                        queueItems.Add(new QueueTrackViewModel(this, correspondingLibraryEntry, position));

                        position++;
                    }

                    Tracks = new ObservableCollection<QueueTrackViewModel>(queueItems);
                }
                catch (InvalidOperationException)
                {
                    // this can happen if the user updates the queue quickly; it'll be updated by the already running loop
                }
            });
            if (Tracks != null) Tracks.CollectionChanged += Tracks_CollectionChanged;
        }

        private int oldIndex;

        private void Tracks_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // since the collection is recreated when the queue is updated, this is only raised when the queue is drag/drop reordered
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                oldIndex = e.OldStartingIndex;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                MainView.Player.Queue.Queue = Tracks.Select(x => x.Path).ToList();

                Debug.WriteLine(oldIndex);
                Debug.WriteLine(e.NewStartingIndex);

                var positionIndex = MainView.Player.Queue.Position - 1;

                if (oldIndex == positionIndex)
                    MainView.Player.Queue.Position = e.NewStartingIndex + 1;
                else if (oldIndex < positionIndex && e.NewStartingIndex >= positionIndex)
                    MainView.Player.Queue.Position--;
                else if (oldIndex > positionIndex && e.NewStartingIndex <= positionIndex)
                    MainView.Player.Queue.Position++;
                
                // TODO: if shuffle is on, fmp-core will shuffle the queue because it is changed here; should be a way to modify the queue without triggering a shuffle

                _ = UpdateQueueAsync();
            }
        }
    }

    public partial class QueueTrackViewModel : ObservableRecipient
    {
        [ObservableProperty]
        private string path;

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ArtistAlbumLabel))]
        private string[] artists;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ArtistAlbumLabel))]
        private string album;

        public string ArtistAlbumLabel => $"{string.Join(", ", Artists)} ・ {Album}";

        [ObservableProperty]
        private int length;

        public string PositionString => IsNowPlaying ? ">" : Position.ToString();

        public FontWeight FontWeight => IsNowPlaying ? FontWeight.Bold : FontWeight.Normal; // this is super lazy. but it works

        public bool IsNowPlaying => viewModel.MainView.Player.Queue.Position == Position;

        [ObservableProperty]
        public int position;

        private readonly QueueViewModel viewModel;
        public QueueTrackViewModel(QueueViewModel viewModel, DatabaseTrack track, int position)
        {
            this.viewModel = viewModel;

            Path = track.Path;
            Title = track.Title;
            Artists = track.Artists;
            Album = track.Album;
            Length = track.Length;
            this.Position = position;
            Update();
        }

        public void Update()
        {
            OnPropertyChanged(nameof(IsNowPlaying));
            OnPropertyChanged(nameof(FontWeight));
            OnPropertyChanged(nameof(PositionString));
        }

        public async void JumpTo()
        {
            viewModel.MainView.Player.Queue.Position = Position - 1;
            await viewModel.MainView.Player.PlayAsync();
        }

        public void RemoveFromQueue() => viewModel.MainView.Player.Queue.Remove(Position - 1);
    }
}
