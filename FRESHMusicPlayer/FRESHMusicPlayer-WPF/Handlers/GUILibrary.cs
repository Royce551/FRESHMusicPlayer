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
        public event EventHandler LibraryChanged;

        public bool RaiseLibraryChanged { get; set; } = true; // remove w/ databasev3

        private readonly NotificationHandler notificationHandler;
        private readonly Dispatcher dispatcher;
        public GUILibrary(LiteDatabase library, NotificationHandler notificationHandler, Dispatcher dispatcher) : base(library)
        {
            this.notificationHandler = notificationHandler;
            this.dispatcher = dispatcher;
        }

        public override void Import(List<string> tracks)
        {
            var notification = new Notification { ContentText = $"Importing {tracks.Count} tracks" };
            dispatcher.Invoke(() => notificationHandler.Add(notification));

            if (App.Config.AutoLibrary) tracks = HandleAutoLibrary(tracks.ToArray());

            base.Import(tracks);
            dispatcher.Invoke(() =>
            {
                notificationHandler.Remove(notification);
                if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
            });
        }

        public override void Import(string[] tracks)
        {
            var notification = new Notification { ContentText = $"Importing {tracks.Length} tracks" };
            dispatcher.Invoke(() => notificationHandler.Add(notification));

            if (App.Config.AutoLibrary) tracks = HandleAutoLibrary(tracks).ToArray();

            base.Import(tracks);
            dispatcher.Invoke(() =>
            {
                notificationHandler.Remove(notification);
                if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
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
                if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
            });
        }

        public override void AddTrackToPlaylist(string playlist, string path)
        {
            base.AddTrackToPlaylist(playlist, path);
            if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
        }

        public override DatabasePlaylist CreatePlaylist(string playlist, string path = null)
        {
            var newPlaylist = base.CreatePlaylist(playlist, path);
            if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
            return newPlaylist;
        }

        public override void DeletePlaylist(string playlist)
        {
            base.DeletePlaylist(playlist);
            if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
        }

        public override void Import(string path)
        {
            base.Import(path);

            if (App.Config.AutoLibrary) path = HandleAutoLibrary(new string[] { path })[0];

            if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
        }

        public override void Remove(string path)
        {
            base.Remove(path);
            if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
        }

        public override void RemoveTrackFromPlaylist(string playlist, string path)
        {
            base.RemoveTrackFromPlaylist(playlist, path);
            if (RaiseLibraryChanged) LibraryChanged?.Invoke(null, EventArgs.Empty);
        }

        private List<string> HandleAutoLibrary(string[] tracks)
        {
            var paths = new List<string>();
            foreach (var track in tracks)
            {
                var metadata = new Track(track);
                
                var path = Path.Combine(App.Config.AutoLibraryPath, metadata.Album, metadata.Artist.Split(';')[0]);
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
