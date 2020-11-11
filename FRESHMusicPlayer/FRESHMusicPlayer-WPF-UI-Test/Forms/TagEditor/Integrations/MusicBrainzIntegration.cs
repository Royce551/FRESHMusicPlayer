using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Navigation;

namespace FRESHMusicPlayer.Forms.TagEditor.Integrations
{
    class MusicBrainzIntegration : IReleaseIntegration
    {
        private readonly HttpClient httpClient;
        public bool NeedsInternetConnection => true;

        public MusicBrainzIntegration()
        {
            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FRESHMusicPlayer/8.2.0 (https://github.com/Royce551/FRESHMusicPlayer)");
        }

        public TagEditorRelease Fetch(string id)
        {
            var json = JObject.Parse(httpClient.GetStringAsync($"http://musicbrainz.org/ws/2/release/{id}?inc=artist-credits+labels+discids+recordings&fmt=json").Result);
            var release = new TagEditorRelease
            {
                Id = id,
                Artist = json.SelectToken("artist-credit[0].name").ToString(),
                Name = json.SelectToken("title").ToString(),
                URL = $"https://musicbrainz.org/release/{id}",
                Tracks = new List<TagEditorTrack>()
            };
            foreach (var x in json.SelectToken("media[0].tracks"))
            {
                release.Tracks.Add(new TagEditorTrack
                {
                    TrackNumber = int.Parse(x.SelectToken("number").ToString()),
                    Title = x.SelectToken("title").ToString()
                });
            }
            return release;
        }

        public List<(string Name, string Id)> Search(string query)
        {
            var releases = new List<(string Name, string Id)>();
            var json = JObject.Parse(httpClient.GetStringAsync($"https://musicbrainz.org/ws/2/release/?query={query}&fmt=json").Result);
            var z = json.SelectToken("releases");
            foreach (var x in z)
            {
                releases.Add((x.SelectToken("title").ToString(), x.SelectToken("id").ToString()));
            }
            return releases;
        }
    }
}
