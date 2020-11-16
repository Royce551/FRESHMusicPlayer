using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Forms.TagEditor.Integrations
{
    interface IReleaseIntegration
    {
        bool NeedsInternetConnection { get; }
        bool Worked { get; set; }
        List<(string Name, string Id)> Search(string query);
        TagEditorRelease Fetch(string id);
    }
    public class TagEditorRelease
    {
        public string Id { get; set; }
        public string Artist { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public string URL { get; set; }
        public List<TagEditorTrack> Tracks { get; set; }
    }
    public class TagEditorTrack
    {
        public int TrackNumber { get; set; }
        public string Title { get; set; }
    }
}
