using ATL;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.ViewModels;
using FRESHMusicPlayer.Views;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers
{
    /// <summary>
    /// Builds on FMP Core's library to provide notifications.
    /// </summary>
    public class GUILibrary : Library
    {
        public event EventHandler<IEnumerable<string>> TracksUpdated;
        public event EventHandler<string> PlaylistAdded;
        public event EventHandler<string> PlaylistRemoved;

        public bool RaiseLibraryChangedEvents { get; set; } = true;

        private readonly MainViewModel viewModel;
        public GUILibrary(LiteDatabase library, MainViewModel viewModel) : base(library)
        {
            this.viewModel = viewModel;
        }

        public void TriggerUpdate() => TracksUpdated?.Invoke(null, []);

        public override async Task ImportAsync(List<string> tracks)
        {
            //if (App.Config.AutoLibrary) tracks = HandleAutoLibrary(tracks.ToArray());

            LoggingHandler.Log($"Importing {string.Join(", ", tracks)}");
            await base.ImportAsync(tracks);

            Dispatcher.UIThread.Invoke(() => { if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, tracks); });
            //if (App.Config.ProcessReplayGainAfterImporting) window.ScanLibraryForReplayGain();
        }

        public override async Task ImportAsync(string[] tracks)
        {
            //if (App.Config.AutoLibrary) tracks = HandleAutoLibrary(tracks).ToArray();

            LoggingHandler.Log($"Importing {string.Join(", ", tracks)}");
            await base.ImportAsync(tracks);

            Dispatcher.UIThread.Invoke(() => { if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, tracks); });
            //if (App.Config.ProcessReplayGainAfterImporting) window.ScanLibraryForReplayGain();
        }

        public override void Nuke(bool nukePlaylists = true)
        {
            base.Nuke(nukePlaylists);
            Dispatcher.UIThread.Invoke(() =>
            {
                viewModel.Notifications.Add(new Notification(viewModel)
                {
                    ContentText = "Successfully cleared your library!",
                    Type = NotificationType.Success,
                    DisplayAsToast = true,
                    ToastDisplayTime = null
                });
                if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, []);
            });
        }


        // TODO: backport this into FMP core
        private async Task<List<DatabaseTrack>> processDatabaseMetadataAsync(Action<int> progress = null)
        {
            var tracksToProcess = Database.GetCollection<DatabaseTrack>(TracksCollectionName).Query().Where(x => !x.HasBeenProcessed).ToList();
            var remainingTracksToProcess = tracksToProcess.Count;

            var cancellationTokenSource = new CancellationTokenSource();
            var token = cancellationTokenSource.Token;

            await Parallel.ForEachAsync(tracksToProcess, async (track, token) =>
            {
                Debug.WriteLine("Thread started");
                try
                {
                    IMetadataProvider metadata = (await BackendManager.CreateAndLoadBackendAndGetMetadataAsync(track.Path)).metadata;

                    if (metadata is null) metadata = new FileMetadataProvider(track.Path);

                    track.UpdateFieldsFrom(metadata);
                    track.HasBeenProcessed = true;
                    if (!Database.GetCollection<DatabaseTrack>(TracksCollectionName).Update(track)) throw new Exception("Fueh?!?!?!");
                }
                catch
                {
                    // ignored for now
                    Debug.WriteLine("Error occured processing metadata");
                }

                remainingTracksToProcess--;
                progress?.Invoke(remainingTracksToProcess);
            });

            return tracksToProcess;
        }

        public override async Task<List<DatabaseTrack>> ProcessDatabaseMetadataAsync(Action<int> progress = null)
        {
            var notification = new Notification(viewModel)
            {
                ContentText = "Processing library changes...\n??? tracks remaining\n\nNewly added tracks won't show in the artists or albums tabs until processing is complete.",
                StatusBarText = "Processing library changes...",
                Type = NotificationType.Progress,
                DisplayAsToast = true,
                ToastDisplayTime = TimeSpan.FromSeconds(5)
            };
            Dispatcher.UIThread.Invoke(() => viewModel.Notifications.Add(notification));

            LoggingHandler.Log("Processing library metadata...");

            var startTime = DateTime.Now;
            int? tracksToProcess = null;
            var updatedTracks = await processDatabaseMetadataAsync(p =>
            {
                if (tracksToProcess is null) tracksToProcess = p;

                Dispatcher.UIThread.Invoke(() =>
                {
                    notification.ContentText = $"Processing library changes...\n{p} tracks remaining\n\nNewly added tracks won't show in the artists or albums tabs until processing is complete.";
                });
            });    

            Dispatcher.UIThread.Invoke(() =>
            {
                viewModel.Notifications.Remove(notification);
                if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, updatedTracks.Select(x => x.Path));
            });

            return updatedTracks;
        }

        public override async Task AddTrackToPlaylistAsync(string playlist, string path, bool isSystemPlaylist = false)
        {
            await base.AddTrackToPlaylistAsync(playlist, path, isSystemPlaylist);
            if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, new string[] { path });
        }
        public override async Task<DatabasePlaylist> CreatePlaylistAsync(string playlist, bool isSystemPlaylist, string path = null)
        {
            var newPlaylist = await base.CreatePlaylistAsync(playlist, isSystemPlaylist, path);
            if (RaiseLibraryChangedEvents) PlaylistAdded?.Invoke(null, playlist);
            return newPlaylist;
        }

        public override void DeletePlaylist(string playlist)
        {
            base.DeletePlaylist(playlist);
            if (RaiseLibraryChangedEvents) PlaylistRemoved?.Invoke(null, playlist);
        }

        public override async Task ImportAsync(string path)
        {
            //if (App.Config.AutoLibrary) path = HandleAutoLibrary(new string[] { path })[0];

            //LoggingHandler.Log($"Importing {path}");
            await base.ImportAsync(path);

            Dispatcher.UIThread.Invoke(() => { if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, [path]); });
        }

        public override void Remove(string path)
        {
            base.Remove(path);
            TracksUpdated?.Invoke(null, new string[] { path });
        }

        public override void RemoveTrackFromPlaylist(string playlist, string path)
        {
            base.RemoveTrackFromPlaylist(playlist, path);
            TracksUpdated?.Invoke(null, new string[] { path });
        }

        private List<string> HandleAutoLibrary(string[] tracks)
        {
            var paths = new List<string>();
            //foreach (var track in tracks)
            //{
            //    var metadata = new Track(track);

            //    var path = Path.Combine(App.Config.AutoLibraryPath, metadata.Artist.Split(';')[0], metadata.Album);
            //    if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            //    var fullPath = Path.Combine(path, Path.GetFileName(track));
            //    File.Move(track, fullPath);
            //    paths.Add(fullPath);
            //}
            return paths;
        }

        //public List<DatabaseQueue> GetAllQueues() FMP 10.2
        //{
        //    return Database.GetCollection<DatabaseQueue>("queues").Query().ToList();
        //}

        //public DatabaseQueue CreateQueue(List<string> queue, int position)
        //{
        //    var newQueue = new DatabaseQueue { Queue = queue, QueuePosition = position };
        //    Database.GetCollection<DatabaseQueue>("queues").Insert(newQueue);
        //    return newQueue;
        //}
    }

    //public class DatabaseQueue
    //{
    //    public int DatabaseQueueId { get; set; }
    //    public List<string> Queue { get; set; }
    //    public int QueuePosition { get; set; }
    //}
}
