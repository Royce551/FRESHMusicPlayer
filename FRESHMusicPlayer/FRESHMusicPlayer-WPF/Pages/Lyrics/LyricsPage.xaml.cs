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
    public partial class LyricsPage : UserControl
    {
        private readonly MainWindow window;
        public LyricsPage(MainWindow window)
        {
            this.window = window;
            window.Player.SongChanged += Player_SongChanged;
            window.Player.SongStopped += Player_SongStopped;
            InitializeComponent();
            Lyrics.Initialize(window);
            ShowCoverArt();
            Lyrics.HandleLyrics();
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
        private void Player_SongChanged(object sender, EventArgs e)
        {
            ShowCoverArt();
            Lyrics.HandleLyrics();
        }
        private void Player_SongStopped(object sender, EventArgs e)
        {
            Lyrics.Timer.Stop();
            Lyrics.TimedLyrics = null;
        }
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Player.SongChanged -= Player_SongChanged;
            window.Player.SongStopped -= Player_SongStopped;
        }
    }
}
