using System.Collections.Generic;

namespace FRESHMusicPlayer.Utilities
{
    public class DatabaseFormat
    {
        public int Version { get; set; }
        public List<string> Songs { get; set; }
    }
}