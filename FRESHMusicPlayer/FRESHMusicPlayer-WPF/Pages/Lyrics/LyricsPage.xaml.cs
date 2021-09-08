using ATL;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Pages.Lyrics
{
    /// <summary>
    /// Interaction logic for LyricsPage.xaml
    /// </summary>
    public partial class LyricsPage : Page
    {
        public ITimedLyricsProvider TimedLyrics;
        public WinForms.Timer Timer;

        private readonly MainWindow window;
        public LyricsPage(MainWindow window)
        {
            this.window = window;
            Timer = new WinForms.Timer { Interval = 100 };
            Timer.Tick += Timer_Tick;
            window.Player.SongChanged += Player_SongChanged;
            window.Player.SongStopped += Player_SongStopped;
            InitializeComponent();
            ShowCoverArt();
            HandleLyrics();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!window.Player.FileLoaded) return;
            if (window.Player.CurrentBackend.CurrentTime < TimedLyrics.Lines.Keys.First()) return;
            var currentLines = TimedLyrics.Lines.Where(x => x.Key < window.Player.CurrentBackend.CurrentTime).ToList();
            var previousLines = TimedLyrics.Lines.Where(x => x.Key > window.Player.CurrentBackend.CurrentTime).Reverse().ToList();
            if (currentLines.Count != 0)
            {
                var closest = currentLines.Last();
                LyricsBox.Text = closest.Value;
                LyricsBoxPlus1.Text = previousLines.Count - 1 >= 0 && previousLines.Count - 1 < previousLines.Count ? previousLines[previousLines.Count - 1].Value : string.Empty;
                LyricsBoxPlus2.Text = previousLines.Count - 2 >= 0 && previousLines.Count - 2 < previousLines.Count ? previousLines[previousLines.Count - 2].Value : string.Empty;
                LyricsBoxMinus1.Text = currentLines.Count - 2 >= 0 && currentLines.Count - 3 < currentLines.Count ? currentLines[currentLines.Count - 2].Value : string.Empty;
                LyricsBoxMinus2.Text = currentLines.Count - 3 >= 0 && currentLines.Count - 3 < currentLines.Count ? currentLines[currentLines.Count - 3].Value : string.Empty;
            }   
        }

        public void ShowCoverArt()
        {
            var track = window.CurrentTrack;
            if (track is null) return;
            if (track.CoverArt is null)
            {
                CoverArtBox.Source = null;
                CoverArtOverlay.Visibility = Visibility.Hidden;
            }
            else
            {
                CoverArtBox.Source = BitmapFrame.Create(new MemoryStream(track.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                CoverArtOverlay.Visibility = Visibility.Visible;
            }
        }
        public void HandleLyrics()
        {
            LyricsBox.Text = LyricsBoxMinus2.Text = LyricsBoxMinus1.Text = LyricsBoxPlus1.Text = LyricsBoxPlus2.Text = string.Empty;
            var track = new Track(window.Player.FilePath);
            if (track is null) return;
            // LRC file present
            if (File.Exists(Path.Combine(Path.GetDirectoryName(window.Player.FilePath), Path.GetFileNameWithoutExtension(window.Player.FilePath) + ".lrc")))
            {
                TimedLyrics = new LRCTimedLyricsProvider(window.Player.FilePath);
                Timer.Start();
                ScrollViewer.SetVerticalScrollBarVisibility(LyricsScrollViewer, ScrollBarVisibility.Hidden);
                LyricsBoxMinus2.Visibility = LyricsBoxMinus1.Visibility = LyricsBoxPlus1.Visibility = LyricsBoxPlus2.Visibility = Visibility.Visible;
                LyricsBox.FontWeight = FontWeights.Bold;
            }
            else if (!string.IsNullOrWhiteSpace(track.Lyrics.UnsynchronizedLyrics)) // Embedded untimed lyrics
            {
                LyricsBox.Text = track.Lyrics.UnsynchronizedLyrics;
                Timer.Stop();
                ScrollViewer.SetVerticalScrollBarVisibility(LyricsScrollViewer, ScrollBarVisibility.Auto);
                LyricsBoxMinus2.Visibility = LyricsBoxMinus1.Visibility = LyricsBoxPlus1.Visibility = LyricsBoxPlus2.Visibility = Visibility.Collapsed;
                LyricsBox.FontWeight = FontWeights.Regular;
            }
            else // No lyrics
            {
                LyricsBox.Text = Properties.Resources.LYRICS_NOLYRICS;
                Timer.Stop();
                ScrollViewer.SetVerticalScrollBarVisibility(LyricsScrollViewer, ScrollBarVisibility.Hidden);
                LyricsBoxMinus2.Visibility = LyricsBoxMinus1.Visibility = LyricsBoxPlus1.Visibility = LyricsBoxPlus2.Visibility = Visibility.Collapsed;
                LyricsBox.FontWeight = FontWeights.Regular;
            }
        }
        private void Player_SongChanged(object sender, EventArgs e)
        {
            ShowCoverArt();
            HandleLyrics();
        }
        private void Player_SongStopped(object sender, EventArgs e)
        {
            Timer.Stop();
            TimedLyrics = null;
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Player.SongChanged -= Player_SongChanged;
            window.Player.SongStopped -= Player_SongStopped;
            Timer.Dispose();
        }
    }
}
