using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATL;
using Avalonia.Media.Imaging;
using FRESHMusicPlayer.Handlers.Lyrics;
using ReactiveUI;

namespace FRESHMusicPlayer.ViewModels
{
    public class LyricsViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow { get; set; }

        public ITimedLyricsProvider TimedLyrics { get; private set; }

        public void Initialize()
        {
            MainWindow.Player.SongChanged += Player_SongChanged;
            MainWindow.ProgressTimer.Elapsed += ProgressTimer_Elapsed;
            Update();
        }

        private void ProgressTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!MainWindow.Player.FileLoaded || TimedLyrics is null) return;
            if (MainWindow.Player.CurrentBackend.CurrentTime < TimedLyrics.Lines.Keys.First()) return;
            var currentLines = TimedLyrics.Lines.Where(x => x.Key < MainWindow.Player.CurrentBackend.CurrentTime).ToList();
            if (currentLines.Count != 0)
            {
                var closest = currentLines.Last();
                Text = closest.Value;
            }
        }

        private void Player_SongChanged(object sender, EventArgs e) => Update();

        public void Deinitialize()
        {
            MainWindow.Player.SongChanged -= Player_SongChanged;
            MainWindow.ProgressTimer.Elapsed -= ProgressTimer_Elapsed;
        }

        public void Update()
        {
            var track = new Track(MainWindow.Player.FilePath);

            CoverArt = track.EmbeddedPictures.Count <= 0 ? null : new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));

            if (File.Exists(Path.Combine(Path.GetDirectoryName(MainWindow.Player.FilePath), Path.GetFileNameWithoutExtension(MainWindow.Player.FilePath) + ".lrc")))
            {
                TimedLyrics = new LRCTimedLyricsProvider(MainWindow.Player.FilePath);
                Text = string.Empty;
            }
            else if (!string.IsNullOrWhiteSpace(track.Lyrics.UnsynchronizedLyrics)) // Embedded untimed lyrics
            {
                Text = track.Lyrics.UnsynchronizedLyrics;
                TimedLyrics = null;
            }
            else // No lyrics
            {
                Text = "No lyrics available";
                TimedLyrics = null;
            }
        }

        private string text = "No lyrics available";
        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        private Bitmap coverArt;
        public Bitmap CoverArt
        {
            get => coverArt;
            set => this.RaiseAndSetIfChanged(ref coverArt, value);
        }
    }
}
