using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace FRESHMusicPlayer.Handlers.DatabaseIntegrations
{
    public class MusicBrainzIntegration : IDatabaseIntegration
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
                Artist = json.SelectToken("artist-credit[0].name")?.ToString(),
                Name = json.SelectToken("title")?.ToString(),
                URL = $"https://musicbrainz.org/release/{id}",
                Tracks = new List<TagEditorTrack>()
            };
            var tracks = json.SelectToken("media[0].tracks");
            if (tracks != null)
			{
                foreach (var x in tracks)
                {
                    release.Tracks.Add(new TagEditorTrack
                    {
                        TrackNumber = int.TryParse(x.SelectToken("number")?.ToString(), out int result) ? result : 0,
                        Title = x.SelectToken("title")?.ToString()
                    });
                }
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
            if (z != null)
			{
                foreach (var x in z)
                {
                    releases.Add((x.SelectToken("title")?.ToString() ?? "", x.SelectToken("id")?.ToString() ?? ""));
                }
            }
            return releases;
        }
    }
}
