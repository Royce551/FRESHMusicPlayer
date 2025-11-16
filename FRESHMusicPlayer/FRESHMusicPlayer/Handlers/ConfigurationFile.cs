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
        [NotifyPropertyChangedRecipients]
        private bool showRemainingTime = false;

        [ObservableProperty]
        private double volume = 1;

        [ObservableProperty]
        private Page page = Page.Tracks;

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
