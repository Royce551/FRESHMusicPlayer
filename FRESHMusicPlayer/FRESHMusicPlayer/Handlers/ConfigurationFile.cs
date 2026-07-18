using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers
{
    public partial class ConfigurationFile : ObservableRecipient
    {
        [ObservableProperty]
        public partial bool ShowTimeInWindow { get; set; } = false;

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial bool ShowRemainingTime { get; set; } = false;

        [ObservableProperty]
        public partial double Volume { get; set; } = 1;

        [ObservableProperty]
        public partial Page Page { get; set; } = Page.Tracks;

        [ObservableProperty]
        public partial double WindowWidth { get; set; } = 1000;

        [ObservableProperty]
        public partial double WindowHeight { get; set; } = 800;

        [ObservableProperty]
        public partial bool AutoQueue { get; set; } = true;

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial bool IntegrateDiscordRichPresence { get; set; } = false;

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial bool IntegrateLastFM { get; set; } = false;

        [ObservableProperty]
        public partial string? LastFMToken { get; set; } = null;

        [ObservableProperty]
        public partial bool UseReplayGain { get; set; } = true;

        [ObservableProperty]
        public partial bool PerformReplayGainByTrack { get; set; } = false;

        [ObservableProperty]
        public partial bool PerformReplayGainByAlbum { get; set; } = true;

        [ObservableProperty]
        public partial double ReplayGainPreAmp { get; set; } = 0;

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial List<string> AutoImportPaths { get; set; } = new List<string>();

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial bool PreferDarkTheme { get; set; } = false; // the reason for this setup is to make binding to radio buttons easier

        [ObservableProperty]
        [NotifyPropertyChangedRecipients]
        public partial bool PreferLightTheme { get; set; } = false;

        public static ConfigurationFile Read(string filePath)
        {
            if (!File.Exists(Path.Combine(filePath, "config.json"))) new ConfigurationFile().Save(filePath);

            return JsonSerializer.Deserialize<ConfigurationFile>(File.ReadAllText(Path.Combine(filePath, "config.json"))) ?? throw new Exception();
        }

        public void Save(string filePath)
        {
            if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);

            File.WriteAllText(Path.Combine(filePath, "config.json"), JsonSerializer.Serialize(this));
        }
    }
}
