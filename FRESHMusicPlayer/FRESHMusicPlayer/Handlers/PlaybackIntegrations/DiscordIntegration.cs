using DiscordRPC;
using FRESHMusicPlayer.Backends;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.PlaybackIntegrations
{
    public class DiscordIntegration : IPlaybackIntegration
    {
        private DiscordRpcClient client;
        private string largeImageKey = "icon";
        private HttpClient httpClient;
        private string? lastAlbum;

        public DiscordIntegration(HttpClient httpClient)
        {
            LoggingHandler.Log("Starting Discord integration");

            this.httpClient = httpClient;

            client = new DiscordRpcClient("656678380283887626");
            client.Initialize();
            client.OnRpcMessage += (sender, e) =>
            {
                LoggingHandler.Log($"Discord: {e.Type}");
            };
        }

        public async Task UpdateAsync(IMetadataProvider track, PlaybackStatus status)
        {
            string activity = string.Empty;
            string state = string.Empty;
            bool showPresence = false;

            switch (status)
            {
                case PlaybackStatus.Playing:
                    showPresence = true;
                    activity = "play";
                    state = $"by {string.Join(", ", track.Artists)}";
                    break;
                case PlaybackStatus.Paused:
                case PlaybackStatus.Stopped:
                    showPresence = false;
                    break;
            }

            var updateTimeStamp = Timestamps.Now;

            if (track.Album != lastAlbum)
            {
                if (track is FileMetadataProvider atlTrack && atlTrack.ATLTrack.AdditionalFields.ContainsKey("MusicBrainz Album Id"))
                {
                    LoggingHandler.Log($@"Discord: MusicBrainz MDID provided in file tags: using https://coverartarchive.org/release/{atlTrack.ATLTrack.AdditionalFields["MusicBrainz Album Id"]}/front-250");

                    largeImageKey = $@"https://coverartarchive.org/release/{atlTrack.ATLTrack.AdditionalFields["MusicBrainz Album Id"]}/front-250";
                    lastAlbum = track.Album;
                }
                else
                {
                    await Task.Run(() =>
                    {
                        LoggingHandler.Log($"Discord: Searching for cover art for {track.Album}");

                        var integration = new MusicBrainzIntegration(httpClient);
                        var results = integration.Search($"album:{track.Album} AND artist:{track.Artists[0]}");
                        if (!integration.Worked)
                        {
                            largeImageKey = "icon";
                            return;
                        }

                        var matchingAlbum = results.FirstOrDefault();
                        if (matchingAlbum == default)
                        {
                            LoggingHandler.Log("Discord: Using alternative search method to find this album");

                            var results2 = integration.Search($"{track.Album} {string.Join(", ", track.Artists)}");
                            matchingAlbum = results2.FirstOrDefault();
                            if (matchingAlbum == default)
                            {
                                largeImageKey = "icon";
                                return;
                            }
                        }

                        LoggingHandler.Log($@"Discord: Cover art found: https://coverartarchive.org/release/{matchingAlbum.Id}/front-250");

                        largeImageKey = $@"https://coverartarchive.org/release/{matchingAlbum.Id}/front-250";
                        lastAlbum = track.Album;
                    });
                }

            }

            if (showPresence)
            {
                client?.SetPresence(new RichPresence
                {
                    Details = TruncateBytes(track.Title, 120),
                    State = TruncateBytes(state, 120),
                    Assets = new Assets
                    {
                        LargeImageKey = largeImageKey,
                        LargeImageText = TruncateBytes(track.Album, 120),
                    },
                    Timestamps = Timestamps.FromTimeSpan(track.Length),
                    Type = ActivityType.Listening,
                    StatusDisplay = StatusDisplayType.Details,
                });
            }
            else client?.ClearPresence();
        }

        public void Close()
        {
            LoggingHandler.Log("Closing Discord integration");

            client.ClearPresence();
            client.Dispose();
        }

        private string TruncateBytes(string str, int bytes)
        {
            if (Encoding.UTF8.GetByteCount(str) <= bytes) return str;
            int i = 0;
            while (true)
            {
                if (Encoding.UTF8.GetByteCount(str.Substring(0, i)) > bytes) return str.Substring(0, i);
                i++;
            }
        }
    }

    // TODO: temporary! here just so i can use FMP-WPF code. this should all be remade.
#nullable disable
    internal class MusicBrainzIntegration
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

    internal class TagEditorRelease
    {
        public string Id { get; set; }
        public string Artist { get; set; }
        public string Name { get; set; }
        public int Year { get; set; }
        public string Genre { get; set; }
        public string URL { get; set; }
        public List<TagEditorTrack> Tracks { get; set; }
    }
    internal class TagEditorTrack
    {
        public int TrackNumber { get; set; }
        public string Title { get; set; }
    }
}
