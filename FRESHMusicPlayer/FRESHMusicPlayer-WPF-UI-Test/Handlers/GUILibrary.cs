using ATL;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Utilities;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers
{
    /// <summary>
    /// Builds on FMP Core's library to provide notifications.
    /// </summary>
    public class GUILibrary : Library
    {
        private readonly NotificationHandler notificationHandler;
        public GUILibrary(LiteDatabase library, NotificationHandler notificationHandler) : base(library)
        {
            this.notificationHandler = notificationHandler;
        }

        public override void Nuke(bool nukePlaylists = true)
        {
            base.Nuke(nukePlaylists);
            notificationHandler.Add(new Notification
            {
                ContentText = Properties.Resources.NOTIFICATION_CLEARSUCCESS,
                Type = NotificationType.Success
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
