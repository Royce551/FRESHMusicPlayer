using System.Collections.Generic;

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
        /// Whether to integrate with the Linux desktop
        /// </summary>
        public bool IntegrateMPRIS { get; set; } = true;
        /// <summary>
        /// Whether to provide cover art images to MPRIS
        /// </summary>
        public bool MPRISShowCoverArt { get; set; } = false;
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
        /// <summary>
        /// The file path that FMP was playing before closing
        /// </summary>
        public string FilePath { get; set; }
        /// <summary>
        /// The position into the track that FMP was playing before closing
        /// </summary>
        public double FilePosition { get; set; } = 0;
        /// <summary>
        /// The last tab FMP was on before closing
        /// </summary>
        public int CurrentTab { get; set; } = 0;
        /// <summary>
        /// Directories to scan for tracks to import from on startup
        /// </summary>
        public List<string> AutoImportPaths { get; set; } = new List<string>();
    }
}
