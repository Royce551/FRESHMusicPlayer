using ATL;

namespace FRESHMusicPlayer.Handlers.Integrations
{
    /// <summary>
    /// Integrates FMP with something that can display current playback info
    /// </summary>
    public interface IPlaybackIntegration
    {
        void Update(Track track, PlaybackStatus status);
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
