using System;
using System.Collections.Generic;

namespace FRESHMusicPlayer.Pages.Lyrics
{
    public interface ITimedLyricsProvider
    {
        Dictionary<TimeSpan, string> Lines { get; set; }
    }
}
