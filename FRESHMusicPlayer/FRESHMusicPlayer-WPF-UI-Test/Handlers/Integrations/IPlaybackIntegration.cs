using ATL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public interface IPlaybackIntegration
    {
        void Update(Track track, PlaybackStatus status);
        void Close();
    }
    public enum PlaybackStatus
    {
        Changing = 1,
        Closed = 0,
        Paused = 4,
        Playing = 3,
        Stopped = 2
    }
}
