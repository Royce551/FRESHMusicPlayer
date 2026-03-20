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
        private Bitmap? coverArt;

        [ObservableProperty]
        private bool isAudioVisible = false;
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
            MainView.Player.SongStopped += Player_SongStopped;
            Update();
        }

        private IMetadataProvider previousMetadata;

        public void Update()
        {
            var track = MainView.Player.Metadata;
            if (track is null || !MainView.Player.FileLoaded)
            {
                IsAudioVisible = false;
                IsAlbumVisible = false;
                IsGenreVisible = false;
                IsYearVisible = false;
                IsTrackVisible = false;
                IsDiscVisible = false;
                CoverArt = null;
                return;
            }

            if (track is FileMetadataProvider file)
            {
                Audio = $"{file.ATLTrack.Bitrate}kbps {file.ATLTrack.SampleRate / 1000}kHz {(file.ATLTrack.CodecFamily == 0 ? "(Lossy) " : "(Lossless)")} " +
                    $"{(file.ATLTrack.AdditionalFields.ContainsKey("replaygain_track_gain") ? "RG" : string.Empty)}";
                IsAudioVisible = true;
            }
            else IsAudioVisible = false;

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

            if (track.CoverArt != null)
            {
                if (previousMetadata == null || (previousMetadata != null && !previousMetadata.CoverArt.SequenceEqual(track.CoverArt)))
                    CoverArt = Bitmap.DecodeToWidth(new MemoryStream(track.CoverArt), 750);
            }
            else CoverArt = null;

            previousMetadata = track;
        }

        private void Player_SongStopped(object? sender, PlaybackStoppedEventArgs e)
        {
            if (e.IsEndOfPlayback) Update();
        }

        private void Player_SongChanged(object? sender, EventArgs e) => Update();

        public override void OnNavigatingAway()
        {
            MainView.Player.SongChanged -= Player_SongChanged;
            MainView.Player.SongStopped -= Player_SongStopped;
            if (CoverArt != null) MainView.SetCoverArtVisibility(true);
        }
    }
}
