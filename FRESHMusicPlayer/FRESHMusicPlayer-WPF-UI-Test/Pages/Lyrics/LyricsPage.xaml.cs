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

        private readonly Player player;
        private readonly Track currentTrack;
        public LyricsPage(Player player, Track currentTrack)
        {
            this.player = player;
            this.currentTrack = currentTrack;
            Timer = new WinForms.Timer { Interval = 100 };
            Timer.Tick += Timer_Tick;
            player.SongChanged += Player_SongChanged;
            player.SongStopped += Player_SongStopped;
            InitializeComponent();
            ShowCoverArt();
            HandleLyrics();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (!player.Playing) return;
            if (player.CurrentBackend.CurrentTime < TimedLyrics.Lines.Keys.First()) return;
            var currentLines = TimedLyrics.Lines.Where(x => x.Key < player.CurrentBackend.CurrentTime).ToList();
            if (currentLines.Count != 0)
            {
                var closest = currentLines.Last();
                LyricsBox.Text = closest.Value;
            }   
        }

        public void ShowCoverArt()
        {
            var track = currentTrack;
            if (track is null) return;
            if (track.EmbeddedPictures.Count == 0)
            {
                CoverArtBox.Source = null;
                CoverArtOverlay.Visibility = Visibility.Hidden;
            }
            else
            {
                CoverArtBox.Source = BitmapFrame.Create(new MemoryStream(track.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                CoverArtOverlay.Visibility = Visibility.Visible;
            }
        }
        public void HandleLyrics()
        {
            LyricsBox.Text = string.Empty;
            var track = currentTrack;
            if (track is null) return;
            // LRC file present
            if (File.Exists(Path.Combine(Path.GetDirectoryName(player.FilePath), Path.GetFileNameWithoutExtension(player.FilePath) + ".lrc")))
            {
                TimedLyrics = new LRCTimedLyricsProvider(player.FilePath);
                Timer.Start();
                ScrollViewer.SetVerticalScrollBarVisibility(LyricsScrollViewer, ScrollBarVisibility.Hidden);
            }
            else if (!string.IsNullOrWhiteSpace(track.Lyrics.UnsynchronizedLyrics)) // Embedded untimed lyrics
            {
                LyricsBox.Text = track.Lyrics.UnsynchronizedLyrics;
                Timer.Stop();
                ScrollViewer.SetVerticalScrollBarVisibility(LyricsScrollViewer, ScrollBarVisibility.Auto);
            }
            else // No lyrics
            {
                LyricsBox.Text = Properties.Resources.LYRICS_NOLYRICS;
                Timer.Stop();
                ScrollViewer.SetVerticalScrollBarVisibility(LyricsScrollViewer, ScrollBarVisibility.Hidden);
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
            player.SongChanged -= Player_SongChanged;
            player.SongStopped -= Player_SongStopped;
        }
    }
}
