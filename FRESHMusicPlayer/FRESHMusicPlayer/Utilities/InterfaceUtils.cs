using FRESHMusicPlayer.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Utilities
{
    public static class InterfaceUtils
    {
        /// <summary>
        /// Handles everything related to drag drop
        /// </summary>
        /// <param name="tracks">The file paths that were dropped</param>
        /// <param name="player">An instance of the Player</param>
        /// <param name="library">An instance of the Library</param>
        /// <param name="enqueue">Whether to enqueue the tracks that were dropped</param>
        /// <param name="import">Whether to import the tracks that were dropped</param>
        /// <param name="clearqueue">Whether to clear the queue before handling everything else</param>
        public static async Task DoDragDropAsync(string[] tracks, Player player, GUILibrary library, bool enqueue = true, bool import = true, bool clearqueue = true)
        {
            if (tracks is null) return;

            if (clearqueue) player.Queue.Clear();
            if (tracks.Any(x => Directory.Exists(x)))
            {
                foreach (var track in tracks)
                {
                    if (Directory.Exists(track))
                    {
                        string[] paths = Directory.EnumerateFiles(tracks[0], "*", SearchOption.AllDirectories)
                        .Where(name => name.EndsWith(".mp3")
                        || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
                        || name.EndsWith(".flac") || name.EndsWith(".aiff")
                        || name.EndsWith(".wma")
                        || name.EndsWith(".aac")).ToArray();
                        if (import) await library.ImportAsync(paths);
                        if (enqueue) player.Queue.Add(paths);
                    }
                    else
                    {
                        if (import) await library.ImportAsync(track);
                        if (enqueue) player.Queue.Add(track);
                    }
                }

            }
            else
            {
                if (import) await library.ImportAsync(tracks);
                if (enqueue) player.Queue.Add(tracks);
            }
        }
    }
}
