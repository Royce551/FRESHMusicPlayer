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
    public class DatabaseHandlerX // TODO: move this to FMP Core
    {
        public LiteDatabase Library { get; private set; }
        private readonly NotificationHandler notificationHandler;
        public DatabaseHandlerX(LiteDatabase library, NotificationHandler notificationHandler)
        {
            Library = library;
            this.notificationHandler = notificationHandler;
        }
        public List<DatabaseTrack> Read(string filter = "Title") => Library.GetCollection<DatabaseTrack>("tracks").Query().OrderBy(filter).ToList();
        public List<DatabaseTrack> ReadTracksForArtist(string artist) => Library.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Artist == artist).OrderBy("Title").ToList();
        public List<DatabaseTrack> ReadTracksForAlbum(string album) => Library.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Album == album).OrderBy("TrackNumber").ToList();
        public List<DatabaseTrack> ReadTracksForPlaylist(string playlist)
        {
            var x = Library.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
            var z = new List<DatabaseTrack>();
            foreach (string path in x.Tracks) z.Add(GetFallbackTrack(path));
            return z;
        }
        public void AddTrackToPlaylist(string playlist, string path)
        {
            var x = Library.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
            if (Library.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist) is null)
            {
                x = CreatePlaylist(playlist, path);
                x.Tracks.Add(path);
            }
            else
            {
                x.Tracks.Add(path);
                Library.GetCollection<DatabasePlaylist>("playlists").Update(x);
            }
        }
        public void RemoveTrackFromPlaylist(string playlist, string path)
        {
            var x = Library.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
            x.Tracks.Remove(path);
            Library.GetCollection<DatabasePlaylist>("playlists").Update(x);
        }
        public DatabasePlaylist CreatePlaylist(string playlist, string path = null)
        {
            var newplaylist = new DatabasePlaylist
            {
                Name = playlist,
                Tracks = new List<string>()
            };
            if (Library.GetCollection<DatabasePlaylist>("playlists").Count() == 0) newplaylist.DatabasePlaylistID = 0;
            else newplaylist.DatabasePlaylistID = Library.GetCollection<DatabasePlaylist>("playlists").Query().ToList().Last().DatabasePlaylistID + 1;
            if (path != null) newplaylist.Tracks.Add(path);
            Library.GetCollection<DatabasePlaylist>("playlists").Insert(newplaylist);
            return newplaylist;
        }
        public void DeletePlaylist(string playlist) => Library.GetCollection<DatabasePlaylist>("playlists").DeleteMany(x => x.Name == playlist);
        public void Import(string[] tracks)
        {
            var stufftoinsert = new List<DatabaseTrack>();
            int count = 0;
            foreach (string y in tracks)
            {
                var track = new Track(y);
                stufftoinsert.Add(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration });
                count++;
            }
            Library.GetCollection<DatabaseTrack>("tracks").InsertBulk(stufftoinsert);
        }
        public void Import(List<string> tracks)
        {
            var stufftoinsert = new List<DatabaseTrack>();
            foreach (string y in tracks)
            {
                var track = new Track(y);
                stufftoinsert.Add(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration });
            }
            Library.GetCollection<DatabaseTrack>("tracks").InsertBulk(stufftoinsert);
        }
        public void Import(string path)
        {
            var track = new Track(path);
            Library.GetCollection<DatabaseTrack>("tracks")
                                .Insert(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration });
        }
        public void Remove(string path)
        {
            Library.GetCollection<DatabaseTrack>("tracks").DeleteMany(x => x.Path == path);
        }
        public void Nuke(bool nukePlaylists = true)
        {
            Library.GetCollection<DatabaseTrack>("tracks").DeleteAll();
            if (nukePlaylists) Library.GetCollection<DatabasePlaylist>("playlists").DeleteAll();
            notificationHandler.Add(new Notification
            {
                ContentText = Properties.Resources.NOTIFICATION_CLEARSUCCESS,
                Type = NotificationType.Success
            });
        }
        public async void Convertv1Tov2()
        {
            var notification = new Notification
            {
                ContentText = "Beginning migration to libraryv2",
                IsImportant = true,
                DisplayAsToast = true
            };
            notificationHandler.Add(notification);
            await Task.Run(() =>
            {
                var oldlibrary = DatabaseHandler.ReadSongs();
                var newlibrary = new List<DatabaseTrack>();
                foreach (string x in oldlibrary)
                {
                    var track = new Track(x);
                    newlibrary.Add(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration });
                }
                Library.GetCollection<DatabaseTrack>("tracks").InsertBulk(newlibrary);
            });
            notification.ContentText = "Successfully imported your v1 library!";
            notification.Type = NotificationType.Success;
            notificationHandler.Update(notification);
        }
        public DatabaseTrack GetFallbackTrack(string path)
        {
            var dbTrack = Library.GetCollection<DatabaseTrack>("tracks").FindOne(x => path == x.Path);
            if (dbTrack != null) return dbTrack;
            else
            {
                var track = new Track(path);
                return new DatabaseTrack { Artist = track.Artist, Title = track.Title, Album = track.Album, Length = track.Duration, Path = path, TrackNumber = track.TrackNumber };
            }
        }
    }
}
