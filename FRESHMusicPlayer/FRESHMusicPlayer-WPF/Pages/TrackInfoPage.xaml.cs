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
    public partial class TrackInfoPage : UserControl
    {
        private readonly string tempPath = Path.Combine(Path.GetTempPath() + "FMPalbumart.png");

        private readonly MainWindow window;
        public TrackInfoPage(MainWindow window)
        {
            this.window = window;
            InitializeComponent();
            window.Player.SongChanged += Player_SongChanged;
            window.Player.SongStopped += Player_SongStopped;
            PopulateFields();
            window.SetCoverArtVisibility(false);
        }

        private void Player_SongStopped(object sender, PlaybackStoppedEventArgs e)
        {
            TrackInfo.Visibility = Visibility.Collapsed;
            CoverArtBox.Source = ForegroundCoverArtBox.Source = null;
        }

        public void PopulateFields()
        {
            var track = window.CurrentTrack;
            if (track is null || !window.Player.FileLoaded) return;
            TrackInfo.Visibility = Visibility.Visible;
            if (track.CoverArt is null)
            {
                ForegroundCoverArtBox.Visibility = Visibility.Collapsed;
                CoverArtBox.Source = ForegroundCoverArtBox.Source = null;
                CoverArtOverlay.Visibility = Visibility.Hidden;
            }
            else
            {
                ForegroundCoverArtBox.Visibility = Visibility.Visible;
                CoverArtBox.Source = ForegroundCoverArtBox.Source = BitmapFrame.Create(new MemoryStream(track.CoverArt), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                CoverArtOverlay.Visibility = Visibility.Visible;
            }

            TrackInfo.Update(track);
        }
        private void Player_SongChanged(object sender, EventArgs e) => PopulateFields();

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            window.Player.SongChanged -= Player_SongChanged;
            window.Player.SongStopped -= Player_SongStopped;
            if (CoverArtBox.Source != null) window.SetCoverArtVisibility(true);
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
