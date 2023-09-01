using ATL;
using DiscordRPC;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Forms.TagEditor.Integrations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class DiscordIntegration : IPlaybackIntegration
    {
        private DiscordRpcClient client;
        private string largeImageKey = "icon";
        private HttpClient httpClient;
        private string lastAlbum;

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

        public async void Update(IMetadataProvider track, PlaybackStatus status)
        {
            string activity = string.Empty;
            string state = string.Empty;
            switch (status)
            {
                case PlaybackStatus.Playing:
                    activity = "play";
                    state = $"by {string.Join(", ", track.Artists)}";
                    break;
                case PlaybackStatus.Paused:
                    activity = "pause";
                    state = "Paused";
                    break;
                case PlaybackStatus.Stopped:
                    client.ClearPresence();
                    return;
            }
        
            var updateTimeStamp = Timestamps.Now; 

            if (track.Album != lastAlbum)
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

            client?.SetPresence(new RichPresence
            {
                Details = TruncateBytes(track.Title, 120),
                State = TruncateBytes(state, 120),
                Assets = new Assets
                {
                    LargeImageKey = largeImageKey,
                    LargeImageText = TruncateBytes(track.Album, 120),
                    SmallImageKey = activity
                },
                Timestamps = updateTimeStamp,
            });
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
}
