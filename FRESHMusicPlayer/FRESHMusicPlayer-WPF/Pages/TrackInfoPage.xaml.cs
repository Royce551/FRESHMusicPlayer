using ATL;
using FRESHMusicPlayer.Backends;
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

        private readonly MainWindow window;
        public TrackInfoPage(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            window.Player.SongChanged += Player_SongChanged;
            PopulateFields();
        }
        public void PopulateFields()
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
            InterfaceUtils.SetField(AlbumBox, AlbumLabel, track.Album);
            InterfaceUtils.SetField(GenreBox, GenreLabel, string.Join(", ", track.Genres));
            InterfaceUtils.SetField(YearBox, YearLabel, track.Year.ToString() == "0" ? null : track.Year.ToString());

            InterfaceUtils.SetField(TrackBox, TrackNumberLabel, track.TrackNumber.ToString() == "0" ? null : track.TrackNumber.ToString());
            if (track.TrackTotal > 0) TrackBox.Text += "/" + track.TrackTotal;
            InterfaceUtils.SetField(DiscBox, DiscNumberLabel, track.DiscNumber.ToString() == "0" ? null : track.DiscNumber.ToString());
            if (track.DiscTotal > 0) DiscBox.Text += "/" + track.DiscTotal;

            if (window.CurrentTrack is FileMetadataProvider file) BitrateBox.Text = file.ATLTrack.Bitrate + "kbps " + (file.ATLTrack.SampleRate / 1000) + "kHz";
            else BitrateBox.Text = "Not available"; // TODO: translate
        }
        private void Player_SongChanged(object sender, EventArgs e) => PopulateFields();

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Player.SongChanged -= Player_SongChanged;
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }

        private void Rectangle_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var track = new Track(window.Player.FilePath);
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
