using ATL;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace FRESHMusicPlayer.Handlers
{
    /// <summary>
    /// Builds on FMP Core's library to provide notifications.
    /// </summary>
    public class GUILibrary : Library
    {
        public event EventHandler OtherLibraryUpdateOcccured;
        public event EventHandler<IEnumerable<string>> TracksAdded;
        public event EventHandler<IEnumerable<string>> TracksRemoved;
        public event EventHandler<IEnumerable<string>> TracksUpdated;
        public event EventHandler<string> PlaylistAdded;
        public event EventHandler<string> PlaylistRemoved;

        public bool RaiseLibraryChangedEvents { get; set; } = true;

        private readonly NotificationHandler notificationHandler;
        private readonly Dispatcher dispatcher;
        public GUILibrary(LiteDatabase library, NotificationHandler notificationHandler, Dispatcher dispatcher) : base(library)
        {
            this.notificationHandler = notificationHandler;
            this.dispatcher = dispatcher;
        }

        public void TriggerUpdate() => OtherLibraryUpdateOcccured?.Invoke(null, EventArgs.Empty);

        public override async Task ImportAsync(List<string> tracks)
        {
            var notification = new Notification { ContentText = $"Importing {tracks.Count} tracks" };
            dispatcher.Invoke(() => notificationHandler.Add(notification));

            if (App.Config.AutoLibrary) tracks = HandleAutoLibrary(tracks.ToArray());

            await base.ImportAsync(tracks);
            dispatcher.Invoke(() =>
            {
                notificationHandler.Remove(notification);
                if (RaiseLibraryChangedEvents) TracksAdded?.Invoke(null, tracks);
            });
        }

        public override async Task ImportAsync(string[] tracks)
        {
            var notification = new Notification { ContentText = $"Importing {tracks.Length} tracks" };
            dispatcher.Invoke(() => notificationHandler.Add(notification));

            if (App.Config.AutoLibrary) tracks = HandleAutoLibrary(tracks).ToArray();

            await base.ImportAsync(tracks);
            dispatcher.Invoke(() =>
            {
                notificationHandler.Remove(notification);
                if (RaiseLibraryChangedEvents) TracksAdded?.Invoke(null, tracks);
            });
        }

        public override void Nuke(bool nukePlaylists = true)
        {
            base.Nuke(nukePlaylists);
            dispatcher.Invoke(() =>
            {
                notificationHandler.Add(new Notification
                {
                    ContentText = Properties.Resources.NOTIFICATION_CLEARSUCCESS,
                    Type = NotificationType.Success
                });
                if (RaiseLibraryChangedEvents) OtherLibraryUpdateOcccured?.Invoke(null, EventArgs.Empty);
            });
        }

        public override async Task<List<DatabaseTrack>> ProcessDatabaseMetadataAsync(Action<int> progress = null)
        {
            var notification = new Notification { ContentText = "Processing library changes...\n\nNewly added tracks may not appear in the artists or albums tabs until processing is complete", Type = NotificationType.Information };
            dispatcher.Invoke(() => notificationHandler.Add(notification));

            var updatedTracks = await base.ProcessDatabaseMetadataAsync(p =>
            {
                notification.ContentText = $"Processing library changes...\n{p} tracks remaining\n\nNewly added tracks may not appear in the artists or albums tabs until processing is complete";
                dispatcher.Invoke(() => notificationHandler.Update(notification));
            });

            dispatcher.Invoke(() =>
            {
                notificationHandler.Remove(notification);
                if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, updatedTracks.Select(x => x.Path));
            });
            return updatedTracks;
        }

        public override async Task AddTrackToPlaylistAsync(string playlist, string path, bool isSystemPlaylist = false)
        {
            await base.AddTrackToPlaylistAsync(playlist, path, isSystemPlaylist);
            if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, new string[] {path});
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
            await base.ImportAsync(path);

            if (App.Config.AutoLibrary) path = HandleAutoLibrary(new string[] { path })[0];

            if (RaiseLibraryChangedEvents) TracksAdded?.Invoke(null, new string[] { path });
        }

        public override void Remove(string path)
        {
            base.Remove(path);
            if (RaiseLibraryChangedEvents) TracksRemoved?.Invoke(null, new string[] { path });
        }

        public override void RemoveTrackFromPlaylist(string playlist, string path)
        {
            base.RemoveTrackFromPlaylist(playlist, path);
            if (RaiseLibraryChangedEvents) TracksUpdated?.Invoke(null, new string[] { path });
        }

        private List<string> HandleAutoLibrary(string[] tracks)
        {
            var paths = new List<string>();
            foreach (var track in tracks)
            {
                var metadata = new Track(track);
                
                var path = Path.Combine(App.Config.AutoLibraryPath, metadata.Artist.Split(';')[0], metadata.Album);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                var fullPath = Path.Combine(path, Path.GetFileName(track));
                File.Move(track, fullPath);
                paths.Add(fullPath);
            }
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
