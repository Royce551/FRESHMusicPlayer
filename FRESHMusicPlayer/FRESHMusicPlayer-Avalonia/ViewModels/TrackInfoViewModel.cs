using ATL;
using Avalonia.Media.Imaging;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        private string trackNumberText;
        public string TrackNumberText
        {
            get => trackNumberText; 
            set => this.RaiseAndSetIfChanged(ref trackNumberText, value);
        }
        private string yearText;
        public string YearText
        {
            get => yearText;
            set => this.RaiseAndSetIfChanged(ref yearText, value);
        }
        private string albumText;
        public string AlbumText
        {
            get => albumText;
            set => this.RaiseAndSetIfChanged(ref albumText, value);
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

            if (track.EmbeddedPictures.Count > 0)
                CoverArt = new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));
            else
                CoverArt = null;

            BitrateText = track.Bitrate == 0 ? null : $"Bitrate - {track.Bitrate}kbps {track.SampleRate / 1000}kHz";

            DiscNumberText = track.DiscNumber == 0 ? null : $"Disc {track.DiscNumber}";
            if (track.DiscTotal > 0) DiscNumberText = $"Disc {track.DiscNumber}/{track.DiscTotal}";
            TrackNumberText = track.TrackNumber == 0 ? null : $"Disc {track.TrackNumber}";
            if (track.TrackTotal > 0) TrackNumberText = $"Disc {track.TrackNumber}/{track.TrackTotal}";

            YearText = track.Year == 0 ? null : $"Year {track.Year}";
            AlbumText = track.Album is null ? null : $"Album - {track.Album}";
        }

        private void Player_SongChanged(object sender, EventArgs e) => Update();
    }
}
