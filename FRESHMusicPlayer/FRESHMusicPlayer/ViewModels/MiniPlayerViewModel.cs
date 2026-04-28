using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Backends;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class MiniPlayerViewModel : ObservableRecipient
    {
        public MainViewModel MainViewModel { get; }

        [ObservableProperty]
        public partial bool AreLyricsAvailable { get; set; }

        [ObservableProperty]
        public partial string CurrentLyricLine { get; set; }

        private readonly DispatcherTimer timer;
        public MiniPlayerViewModel(MainViewModel mainViewModel)
        {
            MainViewModel = mainViewModel;
            MainViewModel.Player.SongChanged += Player_SongChanged;

            timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(100) };
            timer.Tick += Timer_Tick;
            Update();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (Lyrics is null || !MainViewModel.Player.FileLoaded)
            {
                AreLyricsAvailable = false;
                CurrentLyricLine = string.Empty;
                return;
            }
            AreLyricsAvailable = true;

            var currentLines = Lyrics.Lines.Where(x => x.Key < MainViewModel.Player.CurrentBackend.CurrentTime);

            CurrentLyricLine = currentLines.Any() ? currentLines.Last().Value : string.Empty;
        }

        [ObservableProperty]
        public partial ITimedLyricsProvider Lyrics { get; set; }

        public void Update()
        {
            if (!MainViewModel.Player.FileLoaded)
            {
                Lyrics = null;
                return;
            }

            if (File.Exists(Path.Combine(Path.GetDirectoryName(MainViewModel.Player.FilePath)!, Path.GetFileNameWithoutExtension(MainViewModel.Player.FilePath) + ".lrc")))
            {
                Lyrics = new LRCTimedLyricsProvider(MainViewModel.Player.FilePath);
            }
            else Lyrics = null;

            timer.Start();
        }

        public void Close()
        {
            MainViewModel.Player.SongChanged -= Player_SongChanged;
        }

        private void Player_SongChanged(object? sender, EventArgs e) => Update();
    }
}
