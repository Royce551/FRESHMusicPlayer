using ATL;
using DiscordRPC;
using FRESHMusicPlayer.Backends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    public class DiscordIntegration : IPlaybackIntegration
    {
        private DiscordRpcClient client;

        public DiscordIntegration()
        {
            LoggingHandler.Log("Starting Discord integration");

            client = new DiscordRpcClient("656678380283887626");
            client.Initialize();
            client.OnRpcMessage += (sender, e) =>
            {
                LoggingHandler.Log($"Discord: {e.Type}");
            };
        }

        public void Update(IMetadataProvider track, PlaybackStatus status)
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
                    activity = "idle";
                    state = "Idle";
                    break;
            }
            client?.SetPresence(new RichPresence
            {
                Details = TruncateBytes(track.Title, 120),
                State = TruncateBytes(state, 120),
                Assets = new Assets
                {
                    LargeImageKey = "icon",
                    SmallImageKey = activity
                },
                Timestamps = Timestamps.Now
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
