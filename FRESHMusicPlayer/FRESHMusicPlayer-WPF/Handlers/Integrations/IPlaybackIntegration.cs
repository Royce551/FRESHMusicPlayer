using ATL;
using FRESHMusicPlayer.Backends;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    /// <summary>
    /// Integrates FMP with something that can display current playback info
    /// </summary>
    public interface IPlaybackIntegration
    {
        /// <summary>
        /// Updates the integration with new information
        /// </summary>
        /// <param name="track">The current track</param>
        /// <param name="status">The playback status of the current track</param>
        void Update(IMetadataProvider track, PlaybackStatus status);
        /// <summary>
        /// Prepares the integration to be closed and set to null (basically Dispose)
        /// </summary>
        void Close();
    }
    /// <summary>
    /// Our own version of WinRT's MediaPlaybackStatus in order to avoid type loading issues on Windows 7.
    /// </summary>
    public enum PlaybackStatus
    {
        Changing = 1,
        Closed = 0,
        Paused = 4,
        Playing = 3,
        Stopped = 2
    }
}
