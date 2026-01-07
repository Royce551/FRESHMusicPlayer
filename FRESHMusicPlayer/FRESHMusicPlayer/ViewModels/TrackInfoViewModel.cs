using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Backends;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class TrackInfoViewModel : ViewModelBase
    {
        [ObservableProperty]
        private Bitmap coverArt;

        [ObservableProperty]
        private string audio;

        [ObservableProperty]
        private bool isDiscVisible = false;
        [ObservableProperty]
        private string disc;

        [ObservableProperty]
        private bool isTrackVisible = false;
        [ObservableProperty]
        private string track;

        [ObservableProperty]
        private bool isYearVisible = false;
        [ObservableProperty]
        private string year;

        [ObservableProperty]
        private bool isGenreVisible = false;
        [ObservableProperty]
        private string genre;

        [ObservableProperty]
        private bool isAlbumVisible = false;
        [ObservableProperty]
        private string album;

        public TrackInfoViewModel()
        {
            
        }

        public override void AfterPageLoaded()
        {
            MainView.SetCoverArtVisibility(false);
            MainView.Player.SongChanged += Player_SongChanged;
            Player_SongChanged(null, EventArgs.Empty);
        }

        private void Player_SongChanged(object? sender, EventArgs e)
        {
            var track = MainView.Player.Metadata;
            if (track is null || !MainView.Player.FileLoaded) return;

            if (track is FileMetadataProvider file)
                Audio = $"{file.ATLTrack.Bitrate}kbps {file.ATLTrack.SampleRate / 1000}kHz {(file.ATLTrack.CodecFamily == 0 ? "(Lossy) " : "(Lossless)")} " +
                    $"{(file.ATLTrack.AdditionalFields.ContainsKey("replaygain_track_gain") ? "RG" : string.Empty)}";
            else Audio = "Unavailable";

            if (!string.IsNullOrWhiteSpace(track.Album))
            {
                Album = track.Album;
                IsAlbumVisible = true;
            }
            else IsAlbumVisible = false;

            var genres = string.Join(", ", track.Genres);
            if (!string.IsNullOrWhiteSpace(genres))
            {
                Genre = genres;
                IsGenreVisible = true;
            }
            else IsGenreVisible = false;

            if (track.Year != 0)
            {
                Year = track.Year.ToString();
                IsYearVisible = true;
            }
            else IsYearVisible = false;

            if (track.TrackNumber != 0)
            {
                Track = track.TrackTotal != 0 ? $"{track.TrackNumber}/{track.TrackTotal}" : track.TrackNumber.ToString();
                IsTrackVisible = true;
            }
            else IsTrackVisible = false;

            if (track.DiscNumber != 0)
            {
                Disc = track.DiscTotal != 0 ? $"{track.DiscNumber}/{track.DiscTotal}" : track.DiscNumber.ToString();
                IsDiscVisible = true;
            }
            else IsDiscVisible = false;

            if (track.CoverArt != null) CoverArt = new Bitmap(new MemoryStream(track.CoverArt));
            else CoverArt = null;
        }

        public override void OnNavigatingAway()
        {
            MainView.Player.SongChanged -= Player_SongChanged;
            if (CoverArt != null) MainView.SetCoverArtVisibility(true);
        }
    }
}
