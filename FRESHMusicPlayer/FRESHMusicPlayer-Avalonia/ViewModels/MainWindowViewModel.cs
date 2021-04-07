using ATL;
using FRESHMusicPlayer;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using Avalonia.Media.Imaging;
using System.IO;
using System.Timers;

namespace FRESHMusicPlayer_Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Player player = new();
        private Timer progressTimer = new Timer(100);
        public MainWindowViewModel()
        {
            player.SongChanged += Player_SongChanged;
            progressTimer.Elapsed += ProgressTimer_Elapsed;
        }

        private void ProgressTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CurrentTime = player.CurrentTime;
        }

        private void Player_SongChanged(object? sender, EventArgs e)
        {
            var track = new Track(player.FilePath);
            Artist = track.Artist;
            Title = track.Title;
            CoverArt = new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));
            TotalTime = player.TotalTime;
            progressTimer.Start();
        }

        public void OnClick()
        {
            player.PlayMusic(FilePathToPlay);
            player.Volume = 0.6f;
        }

        private string? filePathToPlay;
        public string? FilePathToPlay
        {
            get => filePathToPlay;
            set => this.RaiseAndSetIfChanged(ref filePathToPlay, value);
        }

        private TimeSpan currentTime;
        public TimeSpan CurrentTime
        {
            get => currentTime;
            set
            {
                this.RaiseAndSetIfChanged(ref currentTime, value);
                CurrentTimeSeconds = CurrentTime.TotalSeconds;
            }
        }
        private double currentTimeSeconds;
        public double CurrentTimeSeconds
        {
            get => currentTimeSeconds;
            set => this.RaiseAndSetIfChanged(ref currentTimeSeconds, value);
        }
        private TimeSpan totalTime;
        public TimeSpan TotalTime
        {
            get => totalTime;
            set
            {
                this.RaiseAndSetIfChanged(ref currentTime, value);
                CurrentTimeSeconds = CurrentTime.TotalSeconds;
            }
        }
        private double totalTimeSeconds;
        public double TotalTimeSeconds
        {
            get => totalTimeSeconds;
            set => this.RaiseAndSetIfChanged(ref totalTimeSeconds, value);
        }

        private Bitmap? coverArt;
        public Bitmap? CoverArt
        {
            get => coverArt;
            set => this.RaiseAndSetIfChanged(ref coverArt, value);
        }

        private string artist = "Nothing Playing";
        public string Artist
        {
            get => artist;
            set => this.RaiseAndSetIfChanged(ref artist, value);
        }
        private string title = "Nothing Playing";
        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        private float volume;
        public float Volume
        {
            get => volume;
            set
            {
                player.Volume = value;
                this.RaiseAndSetIfChanged(ref volume, value);
            }
        }
    }
}
