using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FRESHMusicPlayer.Forms.TagEditor.Integrations
{
    class MusicBrainzIntegration : IReleaseIntegration
    {
        private readonly HttpClient httpClient;
        public bool NeedsInternetConnection => true;
        public bool Worked { get; set; } = true;

        public MusicBrainzIntegration(HttpClient httpClient)
        {
            this.httpClient = httpClient;
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
            string jsonString;
            try { jsonString = httpClient.GetStringAsync($"https://musicbrainz.org/ws/2/release/?query={query}&fmt=json").Result; }
            catch (AggregateException)
            {
                Worked = false;
                return releases;
            }
            var json = JObject.Parse(jsonString);
            var z = json.SelectToken("releases");
            foreach (var x in z)
            {
                releases.Add((x.SelectToken("title").ToString(), x.SelectToken("id").ToString()));
            }
            return releases;
        }
    }
}
