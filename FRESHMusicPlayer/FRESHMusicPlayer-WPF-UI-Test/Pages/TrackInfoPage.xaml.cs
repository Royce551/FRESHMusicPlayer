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

namespace FRESHMusicPlayer_WPF_UI_Test.Pages
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
            Track track = new Track(MainWindow.Player.FilePath);
            if (track.EmbeddedPictures.Count == 0)
            {
                CoverArtBox.Source = null;
                CoverArtRow.Height = new GridLength(0);
            }
            else CoverArtBox.Source = BitmapFrame.Create(new System.IO.MemoryStream(track.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            AlbumLabel.Text = Properties.Resources.TRACKINFO_ALBUM + track.Album;
            GenreLabel.Text = Properties.Resources.TRACKINFO_GENRE + track.Genre;
            YearLabel.Text = Properties.Resources.TRACKINFO_YEAR + track.Year;

            TrackNumberLabel.Text = Properties.Resources.TRACKINFO_TRACKNUMBER + track.TrackNumber + "/" + track.TrackTotal;
            DiscNumberLabel.Text = Properties.Resources.TRACKINFO_DISCNUMBER + track.DiscNumber + "/" + track.DiscTotal;

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
            }
        }
    }
}
