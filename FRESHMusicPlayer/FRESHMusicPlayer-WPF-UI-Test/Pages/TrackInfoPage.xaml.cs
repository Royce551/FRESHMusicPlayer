using ATL;
using FRESHMusicPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Imaging = System.Drawing.Imaging;

namespace FRESHMusicPlayer.Pages
{
    /// <summary>
    /// Interaction logic for TrackInfoPage.xaml
    /// </summary>
    public partial class TrackInfoPage : Page
    {
        private readonly string tempPath = Path.Combine(Path.GetTempPath() + "FMPalbumart.png");
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
                CoverArtOverlay.Visibility = Visibility.Hidden;
            }
            else
            {
                CoverArtBox.Source = BitmapFrame.Create(new MemoryStream(track.EmbeddedPictures[0].PictureData), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                CoverArtOverlay.Visibility = Visibility.Visible;
            }
            InterfaceUtils.SetField(AlbumBox, AlbumLabel, track.Album);
            InterfaceUtils.SetField(GenreBox, GenreLabel, track.Genre);
            InterfaceUtils.SetField(YearBox, YearLabel, track.Year.ToString() == "0" ? null : track.Year.ToString());

            InterfaceUtils.SetField(TrackBox, TrackNumberLabel, track.TrackNumber.ToString() == "0" ? null : track.TrackNumber.ToString());
            if (track.TrackTotal > 0) TrackBox.Text += "/" + track.TrackTotal;
            InterfaceUtils.SetField(DiscBox, DiscNumberLabel, track.DiscNumber.ToString() == "0" ? null : track.DiscNumber.ToString());
            if (track.DiscTotal > 0) DiscBox.Text += "/" + track.DiscTotal;
            BitrateBox.Text = track.Bitrate + "kbps " + (track.SampleRate / 1000) + "kHz";
        }
        private void Player_SongChanged(object sender, EventArgs e) => PopulateFields();

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            MainWindow.Player.SongChanged -= Player_SongChanged;
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }

        private void Rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var track = new Track(MainWindow.Player.FilePath);
            IList<PictureInfo> embeddedPictures = track.EmbeddedPictures;
            foreach (PictureInfo pic in embeddedPictures)
            {
                var x = System.Drawing.Image.FromStream(new MemoryStream(pic.PictureData));
                x.Save(tempPath, Imaging.ImageFormat.Png);
                System.Diagnostics.Process.Start(tempPath);
                x.Dispose();
            }
        }

    }
}
