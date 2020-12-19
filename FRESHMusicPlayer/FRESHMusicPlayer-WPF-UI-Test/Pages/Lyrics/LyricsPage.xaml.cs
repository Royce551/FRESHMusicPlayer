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
using System.Windows.Shapes;

namespace FRESHMusicPlayer.Pages.Lyrics
{
    /// <summary>
    /// Interaction logic for LyricsPage.xaml
    /// </summary>
    public partial class LyricsPage : Page
    {
        public LyricsPage()
        {
            MainWindow.Player.SongChanged += Player_SongChanged;
            InitializeComponent();
            PopulateFields();
        }

        public void PopulateFields()
        {
            var track = MainWindow.CurrentTrack;
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
            LyricsBox.Text = track.Lyrics.UnsynchronizedLyrics;
        }
        private void Player_SongChanged(object sender, EventArgs e) => PopulateFields();
        private void Page_Unloaded(object sender, RoutedEventArgs e) => MainWindow.Player.SongChanged -= Player_SongChanged;
    }
}
