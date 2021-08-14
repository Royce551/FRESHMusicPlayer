using System;
using System.Collections.Generic;

namespace FRESHMusicPlayer.Handlers.Lyrics
{
    public interface ITimedLyricsProvider
    {
        Dictionary<TimeSpan, string> Lines { get; set; }
    }
}
