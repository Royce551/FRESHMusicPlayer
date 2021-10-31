using System;
using System.Collections.Generic;

namespace FRESHMusicPlayer.Handlers.Configuration
{
    /// <summary>
    /// The way FMP will handle updates
    /// </summary>
    public enum UpdateMode
    {
        /// <summary>
        /// Download, install, and restart automatically - same as default Squirrel behavior
        /// </summary>
        Automatic,
        /// <summary>
        /// Download and install the update automatically, but prompt the user before restarting
        /// </summary>
        Prompt,
        /// <summary>
        /// Do not check for updates automatically; updates are installed through the button in Settings
        /// </summary>
        Manual
    }
    public class ConfigurationFile
    {
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
        /// Whether to integrate with Windows's System Media Transport Controls. Has no effect on Windows 7.
        /// </summary>
        public bool IntegrateSMTC { get; set; } = true;
        /// <summary>
        /// Whether to show remaining time instead of total time at the right side of the progress bar
        /// </summary>
        public bool ShowRemainingProgress { get; set; } = false;
        /// <summary>
        /// Whether to write the currently playing track to a file
        /// </summary>
        public bool PlaybackTracking { get; set; } = false;
        /// <summary>
        /// The way FMP will handle updates
        /// </summary>
        public UpdateMode UpdateMode { get; set; } = UpdateMode.Prompt;
        /// <summary>
        /// The last time FMP checked for updates
        /// </summary>
        public DateTime UpdatesLastChecked { get; set; }
        /// <summary>
        /// The current theme to use
        /// </summary>
        public Skin Theme { get; set; } = Skin.Dark;
        /// <summary>
        /// The volume that FMP was set to before closing
        /// </summary>
        public int Volume { get; set; } = 100;
        /// <summary>
        /// The last menu FMP was on before closing
        /// </summary>
        public Tab CurrentMenu { get; set; } = Tab.Import;
        /// <summary>
        /// File paths to automatically import tracks from on startup
        /// </summary>
        public List<string> AutoImportPaths { get; set; } = new List<string>();

        /// <summary>
        /// The last recorded version of FMP we're on. If this doesn't match the actual version,
        /// FMP was updated.
        /// </summary>
        public string LastRecordedVersion { get; set; }
    }

}
