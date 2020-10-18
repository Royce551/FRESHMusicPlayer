using ATL;
using FRESHMusicPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Imaging = System.Drawing.Imaging;
using System.Windows.Shapes;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for TrackInfoPage.xaml
    /// </summary>
    public partial class TrackInfoPage : Page
    {
        public TrackInfoPage()
        {
            InitializeComponent();
            MainWindow.Player.SongChanged += Player_SongChanged;
            PopulateFields();
        }
        public void PopulateFields()
        {
            var track = MainWindow.CurrentTrack;
            if (track is null) return;
            if (track.EmbeddedPictures.Count == 0)
            {
                CoverArtBox.Source = null;
                CoverArtRow.Height = new GridLength(0);
            }
            else CoverArtBox.Source = BitmapFrame.Create(new MemoryStream(track.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            if (!string.IsNullOrEmpty(track.Album))
            {
                AlbumLabel.Visibility = Visibility.Visible;
                AlbumLabel.Text = Properties.Resources.TRACKINFO_ALBUM + track.Album;
            }
            else AlbumLabel.Visibility = Visibility.Collapsed;
            if (!string.IsNullOrEmpty(track.Genre))
            {
                GenreLabel.Visibility = Visibility.Visible;
                GenreLabel.Text = Properties.Resources.TRACKINFO_GENRE + track.Genre;
            }
            else GenreLabel.Visibility = Visibility.Collapsed;
            if (track.Year != 0) 
            {
                YearLabel.Visibility = Visibility.Visible;
                YearLabel.Text = Properties.Resources.TRACKINFO_YEAR + track.Year;
            }
            else YearLabel.Visibility = Visibility.Collapsed;

            TrackNumberLabel.Text = Properties.Resources.TRACKINFO_TRACKNUMBER + track.TrackNumber;
            if (track.TrackTotal > 0) TrackNumberLabel.Text += "/" + track.TrackTotal;
            DiscNumberLabel.Text = Properties.Resources.TRACKINFO_DISCNUMBER + track.DiscNumber;
            if (track.DiscTotal > 0) DiscNumberLabel.Text += "/" + track.DiscTotal;
            BitrateLabel.Text = Properties.Resources.TRACKINFO_BITRATE + track.Bitrate + "kbps";
        }
        private void Player_SongChanged(object sender, EventArgs e) => PopulateFields();

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Player.SongChanged -= Player_SongChanged;
        }

        private void Rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Track track = new Track(MainWindow.Player.FilePath);
            IList<PictureInfo> embeddedPictures = track.EmbeddedPictures;
            foreach (PictureInfo pic in embeddedPictures)
            {
                System.Drawing.Image x = System.Drawing.Image.FromStream(new MemoryStream(pic.PictureData));
                x.Save(System.IO.Path.GetTempPath() + "FMPalbumart.png", Imaging.ImageFormat.Png);
                System.Diagnostics.Process.Start(System.IO.Path.GetTempPath() + "FMPalbumart.png");
                x.Dispose();
            }
        }
    }
}
