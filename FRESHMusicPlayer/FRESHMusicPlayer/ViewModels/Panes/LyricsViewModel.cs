using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class LyricsViewModel : LyricsHandlingViewModel
    {
        public LyricsView View { get; set; } = null!;

        private readonly DispatcherTimer timer;

        public LyricsViewModel(MainViewModel mainView)
        {
            MainView = mainView;

            timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            timer.Tick += Timer_Tick;
            Update();
            CoverArt = MainView.CoverArt;
        }

        [ObservableProperty]
        public partial bool AutoScrollEnabled { get; set; } = true;

        public void ResumeAutoScroll()
        {
            AutoScrollEnabled = true;
            if (CurrentLines != null) View.ScrollToCenter(CurrentLines);
        }

        private void Timer_Tick(object? sender, EventArgs e) => TickLyrics();

        public override void OnCurrentLineChanged()
        {
            if (CurrentLines != null && AutoScrollEnabled) View.ScrollToCenter(CurrentLines);
        }

        public override void AfterPageLoaded()
        {
            MainView.Player.SongChanged += Player_SongChanged;
            MainView.Player.SongStopped += Player_SongStopped;
            MainView.CoverArtChanged += MainView_CoverArtChanged;
        }

        private void MainView_CoverArtChanged(object? sender, EventArgs e)
        {
            CoverArt = MainView.CoverArt;
        }

        private void Player_SongStopped(object? sender, PlaybackStoppedEventArgs e)
        {
            timer.Stop();

            if (e.IsEndOfPlayback) CoverArt = null;

            Update();
        }

        public override void OnNavigatingAway()
        {
            MainView.Player.SongChanged -= Player_SongChanged;
            MainView.Player.SongStopped -= Player_SongStopped;
            MainView.CoverArtChanged -= MainView_CoverArtChanged;
        }

        private void Player_SongChanged(object? sender, EventArgs e) => Update();

        [ObservableProperty]
        public partial Bitmap? CoverArt { get; set; }

        

        public void Update()
        {
            if (!MainView.Player.FileLoaded)
            {
                Lyrics = null;
                return;
            }

            AutoScrollEnabled = true;

            UpdateLyrics();
            timer.Start();
        }
    }

    public partial class LyricLineViewModel : ObservableObject
    {
        [ObservableProperty]
        public partial TimeSpan Timestamp { get; set; }

        [ObservableProperty]
        public partial string? Lyric { get; set; }

        private LyricState state = LyricState.Next;
        public LyricState State
        {
            get => state;
            set
            {
                if (!SetProperty(ref state, value))
                {
                    return;
                }

                OnPropertyChanged(nameof(Weight));
                OnPropertyChanged(nameof(Opacity));
                OnPropertyChanged(nameof(Transform));

                if (state == LyricState.Current)
                {
                    view.OnCurrentLineChanged();
                }
            }
        }

        public FontWeight Weight => State == LyricState.Current ? FontWeight.Bold : FontWeight.Normal;

        public double Opacity
        {
            get
            {
                switch (State)
                {
                    case LyricState.Next:
                        return 0.5;
                    default:
                        return 1;
                }
            }
        }

        public string? Transform
        {
            get
            {
                switch (State)
                {
                    case LyricState.Current:
                        return "scale(1.15)";
                    default:
                        return null;
                }
            }
        }

        private readonly LyricsHandlingViewModel view;
        public LyricLineViewModel(LyricsHandlingViewModel view)
        {
            this.view = view;
        }

        public void JumpTo() => view.MainView.Player.CurrentTime = Timestamp;
    }

    public enum LyricState
    {
        Past,
        Current,
        Next,
        Untimed
    }

    public interface ITimedLyricsProvider
    {
        Dictionary<TimeSpan, string> Lines { get; set; }
    }

    public class LRCTimedLyricsProvider : ITimedLyricsProvider
    {
        public Dictionary<TimeSpan, string> Lines { get; set; } = new Dictionary<TimeSpan, string>();
        public LRCTimedLyricsProvider(string filePath) => Parse(filePath);
        private void Parse(string path)
        {
            var filetoRead = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path) + ".lrc");
            var lines = File.ReadAllLines(filetoRead);

            var lineExpression = new Regex(@"\[(\d+):(\d+).(\d+)\]+\s*(.*)");

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!lineExpression.IsMatch(line)) continue; // not a timestamp; not interested

                var match = lineExpression.Match(line);

                var minutes = int.Parse(match.Groups[1].Value);
                var seconds = int.Parse(match.Groups[2].Value);
                var hundredths = double.Parse(match.Groups[3].Value);
                var lyric = match.Groups[4].Value;

                var timeStamp = new TimeSpan(0, 0, minutes, seconds, (int)Math.Round(hundredths / 10));

                if (!Lines.ContainsKey(timeStamp)) Lines.Add(timeStamp, lyric);
            }
        }
    }
}
