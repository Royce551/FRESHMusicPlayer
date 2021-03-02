using ATL;
using FRESHMusicPlayer.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace FRESHMusicPlayer.Handlers
{
    public class PlaytimeTrackingHandler
    {
        public TrackingFile TrackingFile;
        public string FilePath;

        private readonly MainWindow window;
        public PlaytimeTrackingHandler(MainWindow window)
        {
            FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Tracking");
            TrackingFile = Read();
            this.window = window;

            window.Player.SongChanged += Player_SongChanged;
        }
        public void Close()
        {
            window.Player.SongChanged -= Player_SongChanged;
            Write(TrackingFile);
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            try
            {
                var trackingEntry = new TrackingEntry
                {
                    DatePlayed = DateTime.Now,
                    Track = new DatabaseTrack
                    {
                        Path = window.Player.FilePath,
                        Artist = window.CurrentTrack.Artist,
                        Title = window.CurrentTrack.Title,
                        Album = window.CurrentTrack.Album,
                        TrackNumber = window.CurrentTrack.TrackNumber,
                        Length = window.CurrentTrack.Duration
                    }
                };
                TrackingFile.Entries.Add(trackingEntry);
            }
            catch
            {
                // ignored
            }
        }
        private TrackingFile Read()
        {
            if (!File.Exists(Path.Combine(FilePath, "tracking.json")))
            {
                Write(new TrackingFile());
            }
            using (StreamReader file = File.OpenText(Path.Combine(FilePath, "tracking.json")))
            {
                var jsonSerializer = new JsonSerializer();
                return (TrackingFile)jsonSerializer.Deserialize(file, typeof(TrackingFile));
            }
        }
        private void Write(TrackingFile trackingFile)
        {
            if (!Directory.Exists(FilePath)) Directory.CreateDirectory(FilePath);
            using (StreamWriter file = File.CreateText(Path.Combine(FilePath, "tracking.json")))
            {
                new JsonSerializer().Serialize(file, trackingFile);
            }
        }
    }
    public class TrackingFile
    {
        public int Version { get; } = 1;
        public List<TrackingEntry> Entries { get; set; } = new List<TrackingEntry>();
    }
    public class TrackingEntry
    {
        public DatabaseTrack Track { get; set; }
        public DateTime DatePlayed { get; set; }
    }
}
