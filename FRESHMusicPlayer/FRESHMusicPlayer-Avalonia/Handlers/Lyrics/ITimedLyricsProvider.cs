using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Lyrics
{
    public interface ITimedLyricsProvider
    {
        Dictionary<TimeSpan, string> Lines { get; set; }
    }
}
