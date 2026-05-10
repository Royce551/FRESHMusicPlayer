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
