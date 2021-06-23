using ATL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class PlaytimeLoggingIntegration : IPlaybackIntegration
    {
        private Player player;
        private TrackingFile trackingFile;
        private string filePath;

        public PlaytimeLoggingIntegration(Player player)
        {
            this.player = player;
            filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Tracking");
            trackingFile = Read();
            LoggingHandler.Log("Playtime Logging: Starting!");
        }

        public void Update(Track track, PlaybackStatus status)
        {
            try
            {
                var trackingEntry = new TrackingEntry
                {
                    DatePlayed = DateTime.Now,
                    Track = new DatabaseTrack
                    {
                        Path = player.FilePath,
                        Artist = track.Artist,
                        Title = track.Title,
                        Album = track.Album,
                        TrackNumber = track.TrackNumber,
                        Length = track.Duration
                    }
                };
                trackingFile.Entries.Add(trackingEntry);
                LoggingHandler.Log("Playtime Logging: Entry created!");
            }
            catch (Exception e)
            {
                LoggingHandler.Log($"Playtime Logging: Exception was thrown - {e}");
                // ignored
            }
        }

        public void Dispose()
        {
            Write(trackingFile);
        }

        private TrackingFile Read()
        {
            if (!File.Exists(Path.Combine(filePath, "tracking.json")))
            {
                Write(new TrackingFile());
            }
            using (StreamReader file = File.OpenText(Path.Combine(filePath, "tracking.json")))
            {
                var jsonSerializer = new JsonSerializer();
                return (TrackingFile)jsonSerializer.Deserialize(file, typeof(TrackingFile));
            }
        }
        private void Write(TrackingFile trackingFile)
        {
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
            using (StreamWriter file = File.CreateText(Path.Combine(filePath, "tracking.json")))
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
