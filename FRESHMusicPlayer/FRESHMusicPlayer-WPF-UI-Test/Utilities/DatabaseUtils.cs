using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Windows;

namespace FRESHMusicPlayer.Utilities
{
    public class PlaylistSong
    {
        public string Path { get; set; }
    }
    class DatabaseUtils // While you'd expect this to be in FMP Core, this uses ATL; I eventually want to remove FMP Core's dependence on ATL.
    {
        public static List<DatabaseTrack> Read(string filter = "Title") => MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().OrderBy(filter).ToList();
        public static List<DatabaseTrack> ReadTracksForArtist(string artist) => MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Artist == artist).OrderBy("Title").ToList();
        public static List<DatabaseTrack> ReadTracksForAlbum(string album) => MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Album == album).OrderBy("TrackNumber").ToList();
        public static List<DatabaseTrack> ReadTracksForPlaylist(string playlist)
        {
            var x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
            var z = new List<DatabaseTrack>();
            foreach (string path in x.Tracks)
            {
                var dbTrack = MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").FindOne(y => y.Path == path);
                if (dbTrack != null) z.Add(dbTrack); // If track is in db, pull metadata from it (much faster than having to use ATL)
                else
                {
                    var track = new Track(path);
                    z.Add(new DatabaseTrack
                    {
                        Artist = track.Artist,
                        Title = track.Title,
                        Album = track.Album,
                        Length = track.Duration,
                        Path = path,
                        TrackNumber = track.TrackNumber
                    });
                }
            }
            return z;
        }
        public static void AddTrackToPlaylist(string playlist, string path)
        {
            var x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);    
            if (MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist) is null)
            {
                x = CreatePlaylist(playlist, path);
                x.Tracks.Add(path);
            }
            else
            {
                x.Tracks.Add(path);
                MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Update(x);
            }
        }
        public static void RemoveTrackFromPlaylist(string playlist, string path)
        {
            var x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
            x.Tracks.Remove(path);
            MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Update(x);
        }
        public static DatabasePlaylist CreatePlaylist(string playlist, string path = null)
        {
            var newplaylist = new DatabasePlaylist
            {
                Name = playlist,
                Tracks = new List<string>()
            };
            if (MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Count() == 0) newplaylist.DatabasePlaylistID = 0;
            else newplaylist.DatabasePlaylistID = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Query().ToList().Last().DatabasePlaylistID + 1;
            if (path != null) newplaylist.Tracks.Add(path);      
            MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Insert(newplaylist);
            return newplaylist;
        }
        public static void DeletePlaylist(string playlist) => MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").DeleteMany(x => x.Name == playlist);
        public static void Import(string[] tracks)
        {
            var stufftoinsert = new List<DatabaseTrack>();
            int count = 0;
            foreach (string y in tracks)
            {
                Track track = new Track(y);
                stufftoinsert.Add(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration});
                count++;
            }            
            MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").InsertBulk(stufftoinsert);
        }
        public static void Import(List<string> tracks)
        {
            var stufftoinsert = new List<DatabaseTrack>();
            foreach (string y in tracks)
            {
                Track track = new Track(y);
                stufftoinsert.Add(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration });
            }
            MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").InsertBulk(stufftoinsert);
        }
        public static void Import(string path)
        {
            Track track = new Track(path);
            MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks")
                                .Insert(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration });
        }
        public static void Remove(string path)
        {
            MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").DeleteMany(x => x.Path == path);
        }
        public static void Nuke()
        {
            MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").DeleteAll();
            MainWindow.NotificationHandler.Add(new Notification
            {
                HeaderText = "Nuke successful",
                ContentText = "Successfully cleared your library",
                Type = NotificationType.Success
            });
        }
        public async static void Convertv1Tov2()
        {
            Notification notification = new Notification
            {
                HeaderText = "Migration",
                ContentText = "Beginning migration to libraryv2",
                IsImportant = true,
                DisplayAsToast = true
            };
            MainWindow.NotificationHandler.Add(notification);
            await Task.Run(() =>
            {
                var oldlibrary = DatabaseHandler.ReadSongs();
                var newlibrary = new List<DatabaseTrack>();
                foreach (string x in oldlibrary)
                {
                    Track track = new Track(x);
                    newlibrary.Add(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration });
                }
                MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").InsertBulk(newlibrary);        
            });
            notification.HeaderText = "Migration successful";
            notification.ContentText = "Successfully converted your v1 library!";
            notification.Type = NotificationType.Success;
            MainWindow.NotificationHandler.Update(notification);
        }
    }
}
