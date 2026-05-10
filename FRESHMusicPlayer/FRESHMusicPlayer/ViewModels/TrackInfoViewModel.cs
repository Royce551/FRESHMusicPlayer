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
        public partial Bitmap? CoverArt { get; set; }

        [ObservableProperty]
        public partial bool IsAudioVisible { get; set; } = false;

        [ObservableProperty]
        public partial string Audio { get; set; }

        [ObservableProperty]
        public partial bool IsDiscVisible { get; set; } = false;

        [ObservableProperty]
        public partial string Disc { get; set; }

        [ObservableProperty]
        public partial bool IsTrackVisible { get; set; } = false;

        [ObservableProperty]
        public partial string Track { get; set; }

        [ObservableProperty]
        public partial bool IsYearVisible { get; set; } = false;

        [ObservableProperty]
        public partial string Year { get; set; }

        [ObservableProperty]
        public partial bool IsGenreVisible { get; set; } = false;

        [ObservableProperty]
        public partial string Genre { get; set; }

        [ObservableProperty]
        public partial bool IsAlbumVisible { get; set; } = false;

        [ObservableProperty]
        public partial string Album { get; set; }

        public TrackInfoViewModel()
        {
            
        }

        public override void AfterPageLoaded()
        {
            MainView.SetCoverArtVisibility(false);
            MainView.Player.SongChanged += Player_SongChanged;
            MainView.Player.SongStopped += Player_SongStopped;
            MainView.CoverArtChanged += MainView_CoverArtChanged;

            CoverArt = MainView.CoverArtFullSize;
            Update();
        }

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
        }

        private void MainView_CoverArtChanged(object? sender, EventArgs e) => CoverArt = MainView.CoverArtFullSize;

        private void Player_SongStopped(object? sender, PlaybackStoppedEventArgs e)
        {
            if (e.IsEndOfPlayback) Update();
        }

        private void Player_SongChanged(object? sender, EventArgs e) => Update();

        public override void OnNavigatingAway()
        {
            MainView.Player.SongChanged -= Player_SongChanged;
            MainView.Player.SongStopped -= Player_SongStopped;
            MainView.CoverArtChanged -= MainView_CoverArtChanged;
            if (CoverArt != null) MainView.SetCoverArtVisibility(true);
        }
    }
}
