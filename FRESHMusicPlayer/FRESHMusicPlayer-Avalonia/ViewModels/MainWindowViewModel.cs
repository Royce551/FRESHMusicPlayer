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
        #region Core
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
            currentTime = Player.CurrentTime;
            currentTimeSeconds = Player.CurrentTime.TotalSeconds;
            this.RaisePropertyChanged(nameof(CurrentTime));
            this.RaisePropertyChanged(nameof(CurrentTimeSeconds));
            Player.AvoidNextQueue = false;
        }

        private void Player_SongChanged(object? sender, EventArgs e)
        {
            var track = new Track(Player.FilePath);
            Artist = track.Artist;
            Title = track.Title;
            if (track.EmbeddedPictures.Count != 0)
                CoverArt = new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));
            TotalTime = Player.TotalTime;
            ProgressTimer.Start();
        }

        public bool RepeatModeNone { get => Player.Queue.RepeatMode == RepeatMode.None; }
        public bool RepeatModeAll { get => Player.Queue.RepeatMode == RepeatMode.RepeatAll; }
        public bool RepeatModeOne { get => Player.Queue.RepeatMode == RepeatMode.RepeatOne; }
        private RepeatMode repeatMode = RepeatMode.RepeatAll;
        public RepeatMode RepeatMode
        {
            get => repeatMode;
            set
            {
                this.RaiseAndSetIfChanged(ref repeatMode, value);
                this.RaisePropertyChanged(nameof(RepeatModeNone));
                this.RaisePropertyChanged(nameof(RepeatModeAll));
                this.RaisePropertyChanged(nameof(RepeatModeOne));
            }
        }
        private bool paused = false;
        public bool Paused
        {
            get => paused;
            set => this.RaiseAndSetIfChanged(ref paused, value);
        }
        private bool shuffle = false;
        public bool Shuffle
        {
            get => shuffle;
            set => this.RaiseAndSetIfChanged(ref shuffle, value);
        }

        public void SkipPreviousCommand()
        {
            // TODO: implement skip to beginning logic
            Player.PreviousSong();
        }
        public void RepeatCommand()
        {
            if (Player.Queue.RepeatMode == RepeatMode.None)
            {
                Player.Queue.RepeatMode = RepeatMode.RepeatAll;
                RepeatMode = RepeatMode.RepeatAll;
            }
            else if (Player.Queue.RepeatMode == RepeatMode.RepeatAll)
            {
                Player.Queue.RepeatMode = RepeatMode.RepeatOne;
                RepeatMode = RepeatMode.RepeatOne;
            }
            else
            {
                Player.Queue.RepeatMode = RepeatMode.None;
                RepeatMode = RepeatMode.None;
            }
        }
        public void PlayPauseCommand()
        {
            if (Player.Paused)
            {
                Player.ResumeMusic();
                Paused = false;
            }
            else
            {
                Player.PauseMusic();
                Paused = true;
            }
        }
        public void ShuffleCommand()
        {
            if (Player.Queue.Shuffle)
            {
                Player.Queue.Shuffle = false;
                Shuffle = false;
            }
            else
            {
                Player.Queue.Shuffle = true;
                Shuffle = true;
            }
        }
        public void SkipNextCommand()
        {
            Player.NextSong();
        }

        public void OnClick()
        {
            Player.PlayMusic(FilePathToPlay);
        }
        public void EnqueueCommand()
        {
            Player.Queue.Add(FilePathToPlay);
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
            set
            {
                Player.CurrentTime = TimeSpan.FromSeconds(value);
                ProgressTick();
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
        #endregion

        #region Library
        #endregion

    }
}
