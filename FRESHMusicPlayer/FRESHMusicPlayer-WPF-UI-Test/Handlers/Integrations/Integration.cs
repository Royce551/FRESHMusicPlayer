using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
namespace FRESHMusicPlayer.Handlers.Integrations
{
    
    class IntegrationData
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public int TrackNumber { get; set; }
        public int DiscNumber { get; set; }
        public int Bitrate { get; set; }
        //public Image AlbumArt { get; set; }
        
    }
    class IntegrationQuery
    {
        public string Title { get; set; }
        public string AltTitle { get; set; }
        public string Album { get; set; }
        public int TrackNumber { get; set; }
    }
    abstract class Integration
    {
        /*abstract public (string type, string label)[] Options { get; }
        /// <summary>
        /// Fetches metadata from the integration.
        /// </summary>
        /// <param name="query">The query for the integration to use (usually the song title)</param>
        /// <returns></returns>
        abstract public List<IntegrationData> FetchMetadata(IntegrationQuery query);*/
    }
}
