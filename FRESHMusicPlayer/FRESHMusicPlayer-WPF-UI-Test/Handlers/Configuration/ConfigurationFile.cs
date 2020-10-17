using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using FRESHMusicPlayer;
namespace FRESHMusicPlayer.Handlers.Configuration
{
    public enum UpdateMode
    {
        Automatic,
        Prompt,
        Manual
    }
    public enum ReleaseStream
    {
        Stable,
        PreRelease
    }
    public class ConfigurationFile
    {
        public string Language { get; set; } = "en";
        public int OutputDevice { get; set; }
        public bool ShowTimeInWindow { get; set; } = false;
        public bool IntegrateDiscordRPC { get; set; } = false;
        public bool IntegrateSMTC { get; set; } = true;
        public UpdateMode UpdateMode { get; set; } = UpdateMode.Prompt;
        public ReleaseStream ReleaseStream { get; set; } = ReleaseStream.Stable;
        public DateTime UpdatesLastChecked { get; set; }
        public Skin Theme { get; set; } = Skin.Dark;
        public string AccentColorHex { get; set; }
        public int Volume { get; set; } = 100;
        public SelectedMenus CurrentMenu { get; set; } = SelectedMenus.Import;
    }
}
