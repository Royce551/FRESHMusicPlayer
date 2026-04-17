using FRESHMusicPlayer.Backends;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers.PlaybackIntegrations
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
        Task UpdateAsync(IMetadataProvider track, PlaybackStatus status);
        /// <summary>
        /// Prepares the integration to be closed and set to null (basically Dispose)
        /// </summary>
        void Close();
    }
    /// <summary>
    /// Carry-over enum from FMP-WPF since it works. Compatible with PlaybackStatus from the Windows API.
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
