using System;
namespace FRESHMusicPlayer.Handlers.Configuration
{
    public enum UpdateMode
    {
        Automatic,
        Prompt,
        Manual
    }
    public class ConfigurationFile
    {
        public string Language { get; set; } = "en";
        public bool ShowTimeInWindow { get; set; } = false;
        public bool ShowPanesAsWindow { get; set; } = false;
        public bool IntegrateDiscordRPC { get; set; } = false;
        public bool IntegrateSMTC { get; set; } = true;
        public bool ShowRemainingProgress { get; set; } = false;
        public bool PlaybackTracking { get; set; } = false;
        public UpdateMode UpdateMode { get; set; } = UpdateMode.Prompt;
        public DateTime UpdatesLastChecked { get; set; }
        public Skin Theme { get; set; } = Skin.Dark;
        public int Volume { get; set; } = 100;
        public SelectedMenu CurrentMenu { get; set; } = SelectedMenu.Import;
    }
}
