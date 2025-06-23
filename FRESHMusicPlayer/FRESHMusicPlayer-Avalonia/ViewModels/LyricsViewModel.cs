using ATL;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using FRESHMusicPlayer.Handlers.Lyrics;
using ReactiveUI;
using System;
using System.IO;
using System.Linq;

namespace FRESHMusicPlayer.ViewModels
{
    public class LyricsViewModel : ViewModelBase
    {
        public MainWindowViewModel MainWindow { get; set; }

        public ITimedLyricsProvider TimedLyrics { get; private set; }

        public void Initialize()
        {
            Update();
            MainWindow.Player.SongChanged += Player_SongChanged;
            MainWindow.ProgressTimer.Elapsed += ProgressTimer_Elapsed;
        }

        private void ProgressTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (!MainWindow.Player.FileLoaded || TimedLyrics is null) return;
            if (MainWindow.Player.CurrentBackend.CurrentTime < TimedLyrics.Lines.Keys.First()) return;
            var currentLines = TimedLyrics.Lines.Where(x => x.Key < MainWindow.Player.CurrentBackend.CurrentTime).ToList();
            var previousLines = TimedLyrics.Lines.Where(x => x.Key > MainWindow.Player.CurrentBackend.CurrentTime).Reverse().ToList();
            if (currentLines.Count != 0)
            {
                var closest = currentLines.Last();
                Text = closest.Value;
                TextPlus1 = previousLines.Count - 1 >= 0 && previousLines.Count - 1 < previousLines.Count ? previousLines[previousLines.Count - 1].Value : string.Empty;
                TextPlus2 = previousLines.Count - 2 >= 0 && previousLines.Count - 2 < previousLines.Count ? previousLines[previousLines.Count - 2].Value : string.Empty;
                TextMinus1 = currentLines.Count - 2 >= 0 && currentLines.Count - 3 < currentLines.Count ? currentLines[currentLines.Count - 2].Value : string.Empty;
                TextMinus2 = currentLines.Count - 3 >= 0 && currentLines.Count - 3 < currentLines.Count ? currentLines[currentLines.Count - 3].Value : string.Empty;
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
            if (!MainWindow.Player.FileLoaded) return;

            var track = new Track(MainWindow.Player.FilePath);

            CoverArt = track.EmbeddedPictures.Count <= 0 ? null : new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));

            if (File.Exists(Path.Combine(Path.GetDirectoryName(MainWindow.Player.FilePath), Path.GetFileNameWithoutExtension(MainWindow.Player.FilePath) + ".lrc")))
            {
                TimedLyrics = new LRCTimedLyricsProvider(MainWindow.Player.FilePath);
                Text = string.Empty;
                TextMinus2 = TextMinus1 = TextPlus1 = TextPlus2 = string.Empty;
                FontWeight = FontWeight.Bold;
            }
            else if (!string.IsNullOrWhiteSpace(track.Lyrics.UnsynchronizedLyrics)) // Embedded untimed lyrics
            {
                Text = track.Lyrics.UnsynchronizedLyrics;
                TimedLyrics = null;
                TextMinus2 = TextMinus1 = TextPlus1 = TextPlus2 = string.Empty;
                FontWeight = FontWeight.Regular;
            }
            else // No lyrics
            {
                Text = Properties.Resources.Lyrics_NoLyrics;
                TimedLyrics = null;
                TextMinus2 = TextMinus1 = TextPlus1 = TextPlus2 = string.Empty;
                FontWeight = FontWeight.Regular;
            }
        }

        private string textMinus1 = string.Empty;
        public string TextMinus1
        {
            get => textMinus1;
            set => this.RaiseAndSetIfChanged(ref textMinus1, value);
        }
        private string textMinus2 = string.Empty;
        public string TextMinus2
        {
            get => textMinus2;
            set => this.RaiseAndSetIfChanged(ref textMinus2, value);
        }
        private string text = Properties.Resources.Lyrics_NoLyrics;
        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }
        private string textPlus1 = string.Empty;
        public string TextPlus1
        {
            get => textPlus1;
            set => this.RaiseAndSetIfChanged(ref textPlus1, value);
        }
        private string textPlus2 = string.Empty;
        public string TextPlus2
        {
            get => textPlus2;
            set => this.RaiseAndSetIfChanged(ref textPlus2, value);
        }

        private FontWeight fontWeight = FontWeight.Regular;
        public FontWeight FontWeight
        {
            get => fontWeight;
            set => this.RaiseAndSetIfChanged(ref fontWeight, value);
        }

        private Bitmap coverArt;
        public Bitmap CoverArt
        {
            get => coverArt;
            set => this.RaiseAndSetIfChanged(ref coverArt, value);
        }
    }
}
