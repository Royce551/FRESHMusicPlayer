using Avalonia.Controls;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.ViewModels;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.PlaybackIntegrations
{
    public class LastFMIntegration : IPlaybackIntegration
    {
        public bool IsInitialized { get; private set; } = false;

        public const string ApiKey = "8491d888bf27f181e9ed45d370067d9a"; // TODO: i still need to make the secret service
        public const string Secret = "77caff321db877b5167167d5dfa24664";
        private string sessionKey;

        private readonly MainViewModel viewModel;
        private readonly HttpClient httpClient;
        public LastFMIntegration(MainViewModel viewModel)
        {
            this.viewModel = viewModel;
            httpClient = viewModel.HttpClient;
            Initialize();
        }

        public void Initialize()
        {
            if (viewModel.Config.LastFMToken == null)
            {
                IsInitialized = false;
                return;
            }

            sessionKey = viewModel.Config.LastFMToken;
            IsInitialized = true;
        }

        public async Task<string> GetAuthorizationToken()
        {
            var authenticationTokenRequest = $"https://ws.audioscrobbler.com/2.0/?method=auth.gettoken&api_key={ApiKey}&format=json";
            var presig = $"api_key{ApiKey}methodauth.gettoken";
            var getTokenResponse = await httpClient.GetAsync($"{authenticationTokenRequest}&api_sig={EncodeSignature(presig)}");

            var tokenJson = JObject.Parse(await getTokenResponse.Content.ReadAsStringAsync());
            return tokenJson?.SelectToken("token").ToString();
        }

        public async Task<HttpResponseMessage> ConfirmSignIn(string token)
        {
            var request = $"https://ws.audioscrobbler.com/2.0/?method=auth.getSession&api_key={ApiKey}";
            var authPresig = $"api_key{ApiKey}methodauth.getSessiontoken{token}{Secret}";
            return await httpClient.PostAsync($"{request}&api_sig={EncodeSignature(authPresig)}&format=json", new FormUrlEncodedContent(new Dictionary<string, string>()
                    {
                        { "token", token }
                    }));
        }

        private string EncodeSignature(string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }
                return sb.ToString();
            }
        }

        private IMetadataProvider lastTrackListenedTo;
        private DateTime timeListeningStarted;

        public async Task UpdateAsync(IMetadataProvider track, PlaybackStatus status)
        {
            if (!IsInitialized) return;

            if (string.Join(", ", track.Artists) == string.Empty || track.Album == string.Empty) return;

            switch (status)
            {
                case PlaybackStatus.Playing:
                    lastTrackListenedTo = track;
                    timeListeningStarted = DateTime.UtcNow;

                    var updateNowPlayingRequest = $"https://ws.audioscrobbler.com/2.0/?method=track.updateNowPlaying&api_key={ApiKey}&sk={sessionKey}";
                    var updateNowPlayingSignature = $"album{track.Album}api_key{ApiKey}artist{track.Artists[0]}methodtrack.updateNowPlayingsk{sessionKey}track{track.Title}{Secret}";
                    try
                    {
                        await httpClient.PostAsync($"{updateNowPlayingRequest}&api_sig={EncodeSignature(updateNowPlayingSignature)}&format=json", new FormUrlEncodedContent(new Dictionary<string, string>()
                        {
                            { "artist", track.Artists[0] },
                            { "track", track.Title },
                            { "album", track.Album }
                        }));

                        LoggingHandler.Log($"last.fm: updateNowPlaying, request: {updateNowPlayingRequest}");
                    }
                    catch (HttpRequestException)
                    {
                        // ignored
                    }
                    break;
                case PlaybackStatus.Changing:
                case PlaybackStatus.Stopped:
                    _ = ScrobbleAsync();
                    break;
            }
        }

        private async Task ScrobbleAsync()
        {
            if (lastTrackListenedTo == null || lastTrackListenedTo.Length < 30 || (DateTime.UtcNow - timeListeningStarted) < TimeSpan.FromSeconds(lastTrackListenedTo.Length / 2))
                return;

            //if (App.Config.LastFMPaused) return;

            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var scrobbleRequest = $"https://ws.audioscrobbler.com/2.0/?method=track.scrobble&api_key={ApiKey}&sk={sessionKey}";
            var scrobbleSignature = $"album{lastTrackListenedTo.Album}api_key{ApiKey}artist{lastTrackListenedTo.Artists[0]}methodtrack.scrobblesk{sessionKey}timestamp{timeStamp}track{lastTrackListenedTo.Title}{Secret}";
            try
            {
                var scrobbleResponse = await httpClient.PostAsync($"{scrobbleRequest}&api_sig={EncodeSignature(scrobbleSignature)}&format=json", new FormUrlEncodedContent(new Dictionary<string, string>()
                        {
                            { "artist", lastTrackListenedTo.Artists[0] },
                            { "track", lastTrackListenedTo.Title },
                            { "timestamp", timeStamp.ToString() },
                            { "album", lastTrackListenedTo.Album },
                        }));

                LoggingHandler.Log($"last.fm: scrobbling recently played track, request: {scrobbleRequest}");

                if (!scrobbleResponse.IsSuccessStatusCode)
                    viewModel.Notifications.Add(new Notification(viewModel)
                    {
                        ContentText = $"An error occured trying to scrobble {{string.Join(\", \", lastTrackListenedTo.Artists)}} - {{lastTrackListenedTo.Title}}\"),",
                        Type = NotificationType.Failure,
                        DisplayAsToast = true
                    });
            }
            catch (HttpRequestException)
            {
                // ignored
            }

        }

        public void Close() => ScrobbleAsync().Wait(10000);
    }
}
