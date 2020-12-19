using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Pages.Lyrics
{
    public class LRCTimedLyricsProvider : ITimedLyricsProvider
    {
        public Dictionary<TimeSpan, string> Lines { get; set; } = new Dictionary<TimeSpan, string>();
        public LRCTimedLyricsProvider() => Parse(MainWindow.Player.FilePath);
        private void Parse(string path)
        {
            var filetoRead = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".lrc");
            var lines = File.ReadAllLines(filetoRead);
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var minutes = int.Parse(line.Substring(1, 2));
                var seconds = int.Parse(line.Substring(4, 2));
                var hundredths = double.Parse(line.Substring(7, 2));
                var timeStamp = new TimeSpan(0, 0, minutes, seconds, (int)Math.Round(hundredths / 10));
                var lyrics = line.Substring(10);
                //if (Lines.ContainsKey(timeStamp)) Lines.Add(TimeSpan.FromMilliseconds(timeStamp.Milliseconds + 100), lyrics);
                //else
                try
                {
                    Lines.Add(timeStamp, lyrics);
                }
                catch
                {

                }
            }
        }
    }
}
