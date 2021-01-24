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
        public static IEnumerable<CultureInfo> GetAvailableCultures()
        {
            CultureInfo[] culture = CultureInfo.GetCultures(CultureTypes.AllCultures);
            string exeLocation = Path.GetDirectoryName(Uri.UnescapeDataString(new UriBuilder(Assembly.GetExecutingAssembly().CodeBase).Path));

            return culture.Where(cultureInfo => Directory.Exists(Path.Combine(exeLocation, cultureInfo.Name)));
        }
        public static async void DoDragDrop(string[] tracks, bool enqueue = true, bool import = true, bool clearqueue = true)
        {
            if (clearqueue) MainWindow.Player.ClearQueue();
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
                        if (enqueue) MainWindow.Player.AddQueue(paths);
                        if (import) await Task.Run(() => DatabaseUtils.Import(paths));
                    }
                    else
                    {
                        if (enqueue) MainWindow.Player.AddQueue(track);
                        if (import) await Task.Run(() => DatabaseUtils.Import(track));
                    }
                }

            }
            else
            {
                if (enqueue) MainWindow.Player.AddQueue(tracks);
                if (import) await Task.Run(() => DatabaseUtils.Import(tracks));
            }
        }
        public static Storyboard GetDoubleAnimation(double from, double to, TimeSpan duration, PropertyPath path)
        {
            var sb = new Storyboard();
            var doubleAnimation = new DoubleAnimation(from, to, duration);
            Storyboard.SetTargetProperty(doubleAnimation, path);
            sb.Children.Add(doubleAnimation);
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
