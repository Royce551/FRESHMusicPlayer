using ATL;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.IO;

namespace FRESHMusicPlayer.ViewModels
{
    public class TrackInfoViewModel : ViewModelBase
    {
        public Player Player { get; set; }

        private Bitmap coverArt;
        public Bitmap CoverArt
        {
            get => coverArt;
            set => this.RaiseAndSetIfChanged(ref coverArt, value);
        }
        private string bitrateText;
        public string BitrateText
        {
            get => bitrateText;
            set => this.RaiseAndSetIfChanged(ref bitrateText, value);
        }

        private string discNumberText;
        public string DiscNumberText
        {
            get => discNumberText;
            set => this.RaiseAndSetIfChanged(ref discNumberText, value);
        }
        private bool discNumberShouldBeVisible = true;
        public bool DiscNumberShouldBeVisible
        {
            get => discNumberShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref discNumberShouldBeVisible, value);
        }

        private string trackNumberText;
        public string TrackNumberText
        {
            get => trackNumberText;
            set => this.RaiseAndSetIfChanged(ref trackNumberText, value);
        }
        private bool trackNumberShouldBeVisible = true;
        public bool TrackNumberShouldBeVisible
        {
            get => trackNumberShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref trackNumberShouldBeVisible, value);
        }

        private string yearText;
        public string YearText
        {
            get => yearText;
            set => this.RaiseAndSetIfChanged(ref yearText, value);
        }
        private bool yearShouldBeVisible = true;
        public bool YearShouldBeVisible
        {
            get => yearShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref yearShouldBeVisible, value);
        }

        private string albumText;
        public string AlbumText
        {
            get => albumText;
            set => this.RaiseAndSetIfChanged(ref albumText, value);
        }
        private bool albumShouldBeVisible = true;
        public bool AlbumShouldBeVisible
        {
            get => albumShouldBeVisible;
            set => this.RaiseAndSetIfChanged(ref albumShouldBeVisible, value);
        }

        public TrackInfoViewModel()
        {

        }

        public void StartThings()
        {
            Player.SongChanged += Player_SongChanged;
            Update();
        }
        public void CloseThings()
        {
            Player.SongChanged -= Player_SongChanged;
        }

        public void Update()
        {
            var track = new Track(Player.FilePath);

            CoverArt = track.EmbeddedPictures.Count <= 0 ? null : new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));

            DiscNumberShouldBeVisible = true;
            TrackNumberShouldBeVisible = true;
            YearShouldBeVisible = true;
            AlbumShouldBeVisible = true;

            BitrateText = $"{track.Bitrate}kbps {track.SampleRate / 1000}kHz";

            if (track.DiscNumber == 0) DiscNumberShouldBeVisible = false;
            else DiscNumberText = track.DiscTotal <= 0 ? track.DiscNumber.ToString() : $"{track.DiscNumber}/{track.DiscTotal}";

            if (track.TrackNumber == 0) TrackNumberShouldBeVisible = false;
            else TrackNumberText = track.TrackTotal <= 0 ? track.TrackNumber.ToString() : $"{track.TrackNumber}/{track.TrackTotal}";

            if (track.Year == 0) YearShouldBeVisible = false;
            else YearText = track.Year.ToString();

            if (track.Album is null) AlbumShouldBeVisible = false;
            else AlbumText = track.Album;
        }

        private void Player_SongChanged(object sender, EventArgs e) => Update();
    }
}
