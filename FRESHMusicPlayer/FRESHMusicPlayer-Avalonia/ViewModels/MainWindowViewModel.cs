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
        public Player Player { get; private set; } = new();
        public Timer ProgressTimer { get; private set; } = new(100);
        public MainWindowViewModel()
        {
            Player.SongChanged += Player_SongChanged;
            Player.SongStopped += Player_SongStopped;
            Player.SongException += Player_SongException;
            ProgressTimer.Elapsed += ProgressTimer_Elapsed;
        }

        private void Player_SongException(object? sender, FRESHMusicPlayer.Handlers.PlaybackExceptionEventArgs e)
        {
            // TODO: error handling
        }

        private void Player_SongStopped(object? sender, EventArgs e)
        {
            Artist = "Nothing Playing";
            Title = "Nothing Playing";
            CoverArt = null;
            ProgressTimer.Stop();
        }

        private void ProgressTimer_Elapsed(object sender, ElapsedEventArgs e) => ProgressTick();

        public void ProgressTick()
        {
            settingCurrentTimeFromViewModel = true;
            CurrentTime = Player.CurrentTime;
            settingCurrentTimeFromViewModel = false;
        }

        private void Player_SongChanged(object? sender, EventArgs e)
        {
            var track = new Track(Player.FilePath);
            Artist = track.Artist;
            Title = track.Title;
            CoverArt = new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));
            TotalTime = Player.TotalTime;
            ProgressTimer.Start();
        }

        public void OnClick()
        {
            Player.PlayMusic(FilePathToPlay);
            Player.Volume = 0.6f;
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

        private bool settingCurrentTimeFromViewModel = false; // TODO: figure out how to get rid of this
        private double currentTimeSeconds;
        public double CurrentTimeSeconds
        {
            get => currentTimeSeconds;
            set
            {
                if (!settingCurrentTimeFromViewModel)
                {
                    Player.CurrentTime = TimeSpan.FromSeconds(value);
                    ProgressTick();
                }
                this.RaiseAndSetIfChanged(ref currentTimeSeconds, value);
            }
        }
        private TimeSpan totalTime;
        public TimeSpan TotalTime
        {
            get => totalTime;
            set
            {
                this.RaiseAndSetIfChanged(ref totalTime, value);
                TotalTimeSeconds = TotalTime.TotalSeconds;
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
                Player.Volume = value;
                this.RaiseAndSetIfChanged(ref volume, value);
            }
        }
    }
}
