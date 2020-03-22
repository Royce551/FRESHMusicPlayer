using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers
{
    public class PlaybackExceptionEventArgs : EventArgs
    {
        public string Details { get; set; }
    }
}
