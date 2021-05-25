using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.Configuration
{
    public class ConfigurationFile /*: ViewModelBase*/
    {                       // this is technically a Model but who cares!
        /// <summary>
        /// The ISO language code for the current language, or "automatic" to use the system default.
        /// </summary>
        public string Language { get; set; } = "automatic";
        /// <summary>
        /// Whether to show the current time in the window's title
        /// </summary>
        public bool ShowTimeInWindow { get; set; } = false;
        /// <summary>
        /// Whether to integrate with Discord Rich Presence
        /// </summary>
        public bool IntegrateDiscordRPC { get; set; } = false;
        /// <summary>
        /// Whether to show remaining time instead of total time at the right side of the progress bar
        /// </summary>
        public bool ShowRemainingProgress { get; set; } = false;
        /// <summary>
        /// Whether to write the currently playing track to a file
        /// </summary>
        public bool PlaybackTracking { get; set; } = false;
        /// <summary>
        /// Whether FMP will check for updates
        /// </summary>
        public bool CheckForUpdates { get; set; } = true;
        /// <summary>
        /// The volume that FMP was set to before closing
        /// </summary>
        public float Volume { get; set; } = 1f;
        public string FilePath { get; set; }
        public double FilePosition { get; set; } = 0;
        /// <summary>
        /// The last tab FMP was on before closing
        /// </summary>
        public int CurrentTab { get; set; } = 0;
        public List<string> AutoImportPaths { get; set; } = new List<string>();
        /// <summary>
        /// Whether the "downloaded wrong thing" warning was already shown
        /// </summary>
        public bool WindowsWarningWasShown { get; set; } = false;
    }
}
