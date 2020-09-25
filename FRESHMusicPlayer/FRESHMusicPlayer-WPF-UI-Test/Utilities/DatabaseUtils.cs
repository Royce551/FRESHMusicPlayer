using ATL;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Notifications;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Globalization.DateTimeFormatting;

namespace FRESHMusicPlayer.Utilities
{
    public class PlaylistSong
    {
        public string Path { get; set; }
    }
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
        public static List<DatabaseTrack> ReadTracksForAlbum(string album) => MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(x => x.Album == album).OrderBy("TrackNumber").ToList();
        public static List<DatabaseTrack> ReadTracksForPlaylist(string playlist)
        {
            DatabasePlaylist x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
            if (x is null) MainWindow.NotificationHandler.Add(new Notification { ContentText = "debug" });
            List<DatabaseTrack> z = new List<DatabaseTrack>();
            foreach (string path in x.Tracks)
            {
                z = MainWindow.Libraryv2.GetCollection<DatabaseTrack>("tracks").Query().Where(y => y.Path == path).ToList();
                if (z is null) MessageBox.Show("debug");
            }
            return z;
        }
        public static void AddTrackToPlaylist(string playlist, string path)
        {
            MainWindow.NotificationHandler.Add(new Notification { ContentText = playlist });
            var x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Query().Where(y => y.Name == playlist).ToList();
            //if (x is null)
            //{
                //MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Insert(new DatabasePlaylist { Name = playlist, Tracks = new List<string>() });
                //x = MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").FindOne(y => y.Name == playlist);
                if (x is null) MainWindow.NotificationHandler.Add(new Notification { ContentText = "debug" });
            //}
            x[0].Tracks.Add(path);
            MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").DeleteMany(y => y.Name == playlist); // roundabout method because Update() doesn't work
            MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Insert(x[0]);
        }
        public static void CreatePlaylist(string playlist)
        {
            //MainWindow.Libraryv2.GetCollection<DatabasePlaylist>("playlists").Insert(new DatabasePlaylist { Name = playlist });
        }
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
