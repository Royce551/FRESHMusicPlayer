using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FRESHMusicPlayer.Forms.TagEditor.Integrations
{
    class DiscogsIntegration : IReleaseIntegration
    {
        private readonly HttpClient httpClient;
        private readonly string Key = "rYhrWVjHmbqOhVijxBtk";
        private readonly string Secret = "TaUMdjJnmmcjGttJbegdmRyOHyqQxljK";

        public bool NeedsInternetConnection => true;
        public bool Worked { get; set; } = true;
        public DiscogsIntegration(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public TagEditorRelease Fetch(string id)
        {
            var json = JObject.Parse(httpClient.GetStringAsync($"https://api.discogs.com/releases/{id}").Result);
            var release = new TagEditorRelease // Format for releases: https://www.discogs.com/developers#page:database,header:database-release
            {
                Id = json?.SelectToken("id").ToString(),
                Artist = json?.SelectToken("artists[0].name").ToString(),
                Name = json?.SelectToken("title").ToString(),
                Year = int.Parse(json?.SelectToken("year").ToString()),
                Genre = json?.SelectToken("genres[0]").ToString(),
                URL = json?.SelectToken("uri").ToString(),
                Tracks = new List<TagEditorTrack>()
            };
            foreach (var x in json.SelectToken("tracklist"))
            {
                var track = new TagEditorTrack();

                if (int.TryParse(x.SelectToken("position").ToString(), out int trackNumber)) track.TrackNumber = trackNumber;
                else track.TrackNumber = 0;
                track.Title = x.SelectToken("title").ToString();
                release.Tracks.Add(track);
            }
            return release;
        }

        public List<(string Name, string Id)> Search(string query)
        {
            var releases = new List<(string Name, string Id)>();
            string jsonString;
            try { jsonString = httpClient.GetStringAsync($"https://api.discogs.com/database/search?q={{{query}}}&{{track}}&per_page=1&key={Key}&secret={Secret}").Result; }
            catch (AggregateException) 
            {
                Worked = false;
                return releases;
            }
            var json = JObject.Parse(jsonString);
            var z = json.SelectToken("results"); // Format for searching: https://www.discogs.com/developers#page:database,header:database-search
            foreach (var x in z)
            {
                releases.Add(($"{x.SelectToken("title")}; {x.SelectToken("year") ?? "no year"}; {string.Join(", ", x.SelectToken("format") ?? "no format") }; {x.SelectToken("country") ?? "no country"}", x.SelectToken("id").ToString()));
            }
            return releases;
        }
    }
}
