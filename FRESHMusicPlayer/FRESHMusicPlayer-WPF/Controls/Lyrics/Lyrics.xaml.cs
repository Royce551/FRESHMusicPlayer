using ATL;
using FRESHMusicPlayer.Pages.Lyrics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using WinForms = System.Windows.Forms;

namespace FRESHMusicPlayer.Controls.Lyrics
{
    /// <summary>
    /// Interaction logic for Lyrics.xaml
    /// </summary>
    public partial class Lyrics : UserControl
    {
        public ITimedLyricsProvider TimedLyrics;
        public WinForms.Timer Timer;

        private MainWindow window;

        public Lyrics()
        {
            Timer = new WinForms.Timer { Interval = 100 };
            Timer.Tick += Timer_Tick;
            InitializeComponent();
        }

        public void Initialize(MainWindow window) => this.window = window;

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

        private void UserControl_Unloaded(object sender, RoutedEventArgs e) => Timer.Dispose();
    }
}
