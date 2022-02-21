using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace FRESHMusicPlayer.Pages.Lyrics
{
    public class LRCTimedLyricsProvider : ITimedLyricsProvider
    {
        public Dictionary<TimeSpan, string> Lines { get; set; } = new Dictionary<TimeSpan, string>();
        public LRCTimedLyricsProvider(string filePath) => Parse(filePath);
        private void Parse(string path)
        {
            var filetoRead = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".lrc");
            var lines = File.ReadAllLines(filetoRead);

            var lineExpression = new Regex(@"\[(\d+):(\d+).(\d+)\]+\s*(.*)");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!lineExpression.IsMatch(line)) continue; // not a timestamp; not interested

                var match = lineExpression.Match(line);

                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                var hundredths = double.Parse(match.Groups[3].Value);
                var lyric = match.Groups[4].Value;

                var timeStamp = new TimeSpan(0, 0, minutes, seconds, (int)Math.Round(hundredths / 10));

                if (!Lines.ContainsKey(timeStamp)) Lines.Add(timeStamp, lyric);
            }
        }
    }
}
