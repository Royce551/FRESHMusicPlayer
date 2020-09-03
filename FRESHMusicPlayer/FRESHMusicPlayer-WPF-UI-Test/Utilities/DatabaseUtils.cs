using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Utilities
{
    class DatabaseUtils // While you'd expect this to be in FMP Core, this uses ATL; I eventually want to remove FMP Core's dependence on ATL.
    {
        public static List<DatabaseTrack> Read(string filter = "Title")
        {
            return MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().OrderBy(filter).ToList();
        }
        public static List<DatabaseTrack> ReadTracksForArtist(string artist)
        {
            return MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Artist == artist).OrderBy("Title").ToList();
        }
        public static List<DatabaseTrack> ReadTracksForAlbum(string album)
        {
            return MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Artist == album).OrderBy("TrackNumber").ToList();
        }
        public static void Import(string[] tracks)
        {
            var stufftoinsert = new List<DatabaseTrack>();
            foreach (string y in tracks)
            {
                Track track = new Track(y);
                stufftoinsert.Add(new DatabaseTrack { Title = track.Title, Artist = track.Artist, Album = track.Album, Path = track.Path, TrackNumber = track.TrackNumber, Length = track.Duration});
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
        public async static void Convertv1Tov2()
        {
            MainWindow.NotificationHandler.Add(new NotificationBox(new NotificationInfo("Migration", "Beginning migration to libraryv2", true, true)));
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
            MainWindow.NotificationHandler.Add(new NotificationBox(new NotificationInfo("Migration successful", "Successfully converted your v1 library to the new format", true, true)));
        }
    }
}
