using ATL;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using LiteDB;
using System;
using System.Collections.Generic;
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
            base.Import(tracks);
            dispatcher.Invoke(() =>
            {
                notificationHandler.Remove(notification);
                LibraryChanged?.Invoke(null, EventArgs.Empty);
            });
        }

        public override void Import(string[] tracks)
        {
            var notification = new Notification { ContentText = $"Importing {tracks.Length} tracks" };
            dispatcher.Invoke(() => notificationHandler.Add(notification));
            base.Import(tracks);
            dispatcher.Invoke(() =>
            {
                notificationHandler.Remove(notification);
                LibraryChanged?.Invoke(null, EventArgs.Empty);
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
                LibraryChanged?.Invoke(null, EventArgs.Empty);
            });
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
