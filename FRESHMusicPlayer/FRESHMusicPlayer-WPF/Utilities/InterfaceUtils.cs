using FRESHMusicPlayer.Handlers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace FRESHMusicPlayer.Utilities
{
    public static class InterfaceUtils
    {
        /// <summary>
        /// Collapses the box and lebel if the value is null or empty.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="label"></param>
        /// <param name="value"></param>
        public static void SetField(TextBlock box, TextBlock label, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                box.Visibility = Visibility.Collapsed;
                label.Visibility = Visibility.Collapsed;
            }
            else
            {
                box.Text = value;
                box.Visibility = Visibility.Visible;
                label.Visibility = Visibility.Visible;
            }
        }
        //public static IEnumerable<CultureInfo> GetAvailableCultures()
        //{
        //    CultureInfo[] culture = CultureInfo.GetCultures(CultureTypes.AllCultures);
        //    string exeLocation = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

        //    return culture.Where(cultureInfo => Directory.Exists(Path.Combine(exeLocation, cultureInfo.Name)));
        //}
        /// <summary>
        /// Handles everything related to drag drop
        /// </summary>
        /// <param name="tracks">The file paths that were dropped</param>
        /// <param name="player">An instance of the Player</param>
        /// <param name="library">An instance of the Library</param>
        /// <param name="enqueue">Whether to enqueue the tracks that were dropped</param>
        /// <param name="import">Whether to import the tracks that were dropped</param>
        /// <param name="clearqueue">Whether to clear the queue before handling everything else</param>
        public static async void DoDragDrop(string[] tracks, Player player, GUILibrary library, bool enqueue = true, bool import = true, bool clearqueue = true)
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
                        if (import) await Task.Run(() => library.Import(paths));
                        if (enqueue) player.Queue.Add(paths);
                    }
                    else
                    {
                        if (import) await Task.Run(() => library.Import(track));
                        if (enqueue) player.Queue.Add(track);
                    }
                }

            }
            else
            {
                if (import) await Task.Run(() => library.Import(tracks));
                if (enqueue) player.Queue.Add(tracks);
            }
        }
        /// <summary>
        /// Provides a storyboard with a double animation for convenience
        /// </summary>
        /// <param name="from">The start value</param>
        /// <param name="to">The end value</param>
        /// <param name="duration">How long the animation will run</param>
        /// <param name="path">The property to animate</param>
        /// <returns>A storyboard ready to begin</returns>
        public static Storyboard GetDoubleAnimation(double from, double to, TimeSpan duration, PropertyPath path)
        {
            var sb = new Storyboard();
            var doubleAnimation = new DoubleAnimation(from, to, duration);
            Storyboard.SetTargetProperty(doubleAnimation, path);
            sb.Children.Add(doubleAnimation);
            return sb;
        }
        public static Storyboard GetThicknessAnimation(Thickness from, Thickness to, TimeSpan duration, PropertyPath path, IEasingFunction easingFunction = null)
        {
            var sb = new Storyboard();
            var thicknessAnimation = new ThicknessAnimation(from, to, duration);
            if (easingFunction != null) thicknessAnimation.EasingFunction = easingFunction;
            Storyboard.SetTargetProperty(thicknessAnimation, path);
            sb.Children.Add(thicknessAnimation);
            return sb;
        }
        public static Task BeginStoryboardAsync(this Storyboard storyboard, FrameworkElement containingObject)
        {
            var tcs = new TaskCompletionSource<bool>();
            if (storyboard == null)
                tcs.SetException(new ArgumentNullException());
            else
            {
                EventHandler onComplete = null;
                onComplete = (s, e) => {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                };
                storyboard.Completed += onComplete;
                storyboard.Begin(containingObject);
            }
            return tcs.Task;
        }
    }
}
