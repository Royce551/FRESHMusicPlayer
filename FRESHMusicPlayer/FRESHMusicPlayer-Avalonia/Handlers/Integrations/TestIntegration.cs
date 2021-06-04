using ATL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class TestIntegration : IPlaybackIntegration
    {
        public event EventHandler UINeedsUpdate;

        public void Dispose()
        {
            Console.WriteLine("disposing");
        }

        public void Update(Track track, PlaybackStatus status)
        {
            Console.WriteLine($"{track.Artist} - {track.Title}; Now {status}");
        }
    }
}
