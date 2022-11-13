using FRESHMusicPlayer.Backends;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace FRESHMusicPlayer.Handlers.Integrations.LastFM
{
    class LastFMIntegration : IPlaybackIntegration
    {
        private HttpClient httpClient;
        private const string apiKey = "8491d888bf27f181e9ed45d370067d9a";
        private const string secret = "77caff321db877b5167167d5dfa24664";

        private string sessionKey;

        private readonly MainWindow window;
        public LastFMIntegration(MainWindow window)
        {
            this.window = window;

            httpClient = new HttpClient();
            System.Net.ServicePointManager.SecurityProtocol |= System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls11 | System.Net.SecurityProtocolType.Tls;
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("FRESHMusicPlayer/11.2.0 (https://github.com/Royce551/FRESHMusicPlayer)");

            Initialize();
        }

        private async void Initialize()
        {
            var sessionKeyPath = Path.Combine(App.DataFolderLocation, "Configuration", "FMP-WPF", "lastfmsession.txt");
            if (File.Exists(sessionKeyPath)) sessionKey = File.ReadAllText(sessionKeyPath);
            else
            {
                var userDialog = new Forms.FMPTextEntryBox("LastFM username or password");
                userDialog.ShowDialog();

                string username;
                if (userDialog.OK)
                {
                    username = userDialog.Response;
                }
                else return;

                var passwordDialog = new Forms.FMPTextEntryBox("LastFM password");
                passwordDialog.ShowDialog();
                string password;
                if (passwordDialog.OK)
                {
                    password = passwordDialog.Response;
                }
                else return;

                var request = $"https://ws.audioscrobbler.com/2.0/?method=auth.getMobileSession&api_key={apiKey}&password={password}&username={username}";
                var presig = $"api_key{apiKey}methodauth.getMobileSessionpassword{password}username{username}{secret}";
                var response = await httpClient.PostAsync($"{request}&api_sig={EncodeSignature(presig)}&format=json", null);
                response.EnsureSuccessStatusCode();
                MessageBox.Show(await response.Content.ReadAsStringAsync());
                var json = JObject.Parse(await response.Content.ReadAsStringAsync());
                sessionKey = json?.SelectToken("session.key").ToString();
                File.WriteAllText(Path.Combine(App.DataFolderLocation, "Configuration", "FMP-WPF", "lastfmsession.txt"), sessionKey);
            }
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

        public async void Update(IMetadataProvider track, PlaybackStatus status)
        {
            switch (status)
            {
                case PlaybackStatus.Playing:
                    lastTrackListenedTo = track;
                    timeListeningStarted = DateTime.UtcNow;

                    var updateNowPlayingRequest = $"https://www.audioscrobbler.com/2.0/?method=track.updateNowPlaying&artist={track.Artists[0]}&track={track.Title}&album={track.Album}&api_key={apiKey}&sk={sessionKey}";
                    var updateNowPlayingSignature = $"album{track.Album}api_key{apiKey}artist{track.Artists[0]}methodtrack.updateNowPlayingsk{sessionKey}track{track.Title}{secret}";
                    var updateNowPlayingResponse = await httpClient.PostAsync($"{updateNowPlayingRequest}&api_sig={EncodeSignature(updateNowPlayingSignature)}&format=json", null);
                    break;
                case PlaybackStatus.Changing:
                case PlaybackStatus.Stopped:
                    if (lastTrackListenedTo == null || lastTrackListenedTo.Length < 30 || (DateTime.UtcNow - timeListeningStarted) < TimeSpan.FromSeconds(lastTrackListenedTo.Length / 2))
                        break;

                    var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    var scrobbleRequest = $"https://www.audioscrobbler.com/2.0/?method=track.scrobble&artist={lastTrackListenedTo.Artists[0]}&track={lastTrackListenedTo.Title}&timestamp={timeStamp}&album={lastTrackListenedTo.Album}&api_key={apiKey}&sk={sessionKey}";
                    var scrobbleSignature = $"album{lastTrackListenedTo.Album}api_key{apiKey}artist{lastTrackListenedTo.Artists[0]}methodtrack.scrobblesk{sessionKey}timestamp{timeStamp}track{lastTrackListenedTo.Title}{secret}";
                    var scrobbleResponse = await httpClient.PostAsync($"{scrobbleRequest}&api_sig={EncodeSignature(scrobbleSignature)}&format=json", null);
                    window.NotificationHandler.Add(new Notifications.Notification { ContentText = "Scrobbled!" });
                    break;
            }
        }

        public void Close()
        {
            
        }

       
    }
}
