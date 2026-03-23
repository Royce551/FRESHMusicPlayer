using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Backends;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels
{
    public partial class LyricsViewModel : ViewModelBase
    {
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
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (Lyrics is null || !MainView.Player.FileLoaded) return;

            var currentLines = Lyrics.Where(x => x.Timestamp < MainView.Player.CurrentBackend.CurrentTime).ToList();
            var nextLines = Lyrics.Where(x => x.Timestamp > MainView.Player.CurrentBackend.CurrentTime).Reverse().ToList();

            foreach (var line in nextLines) line.State = LyricState.Next;

            if (currentLines.Count != 0)
            {
                foreach (var line in currentLines) line.State = LyricState.Past;

                currentLines.Last().State = LyricState.Current;
            }
        }

        public override void AfterPageLoaded()
        {
            MainView.Player.SongChanged += Player_SongChanged;
            MainView.Player.SongStopped += Player_SongStopped;
        }

        private void Player_SongStopped(object? sender, PlaybackStoppedEventArgs e)
        {
            timer.Stop();
        }

        public override void OnNavigatingAway()
        {
            MainView.Player.SongChanged -= Player_SongChanged;
            MainView.Player.SongStopped -= Player_SongStopped;
        }

        private void Player_SongChanged(object? sender, EventArgs e) => Update();

        private IMetadataProvider? previousMetadata;

        [ObservableProperty]
        private Bitmap? coverArt;

        [ObservableProperty]
        private ObservableCollection<LyricLineViewModel>? lyrics = new ObservableCollection<LyricLineViewModel>();

        public void Update()
        {
            if (!MainView.Player.FileLoaded) return;

            if (MainView.Player.Metadata.CoverArt != null)
            {
                if (previousMetadata == null || (previousMetadata != null && !previousMetadata.CoverArt.SequenceEqual(MainView.Player.Metadata.CoverArt)))
                    CoverArt = Bitmap.DecodeToWidth(new MemoryStream(MainView.Player.Metadata.CoverArt), 750);
            }

            if (File.Exists(Path.Combine(Path.GetDirectoryName(MainView.Player.FilePath), Path.GetFileNameWithoutExtension(MainView.Player.FilePath) + ".lrc")))
            {
                Lyrics = new ObservableCollection<LyricLineViewModel>(new LRCTimedLyricsProvider(MainView.Player.FilePath).Lines.Select(x => new LyricLineViewModel(MainView) { Timestamp = x.Key, Lyric = x.Value }));
            }
            else if (MainView.Player.Metadata is FileMetadataProvider provider && !string.IsNullOrWhiteSpace(provider.ATLTrack.Lyrics.UnsynchronizedLyrics))
            {

            }
            else Lyrics = null;

            timer.Start();

            previousMetadata = MainView.Player.Metadata;
        }
    }

    public partial class LyricLineViewModel : ObservableObject
    {
        [ObservableProperty]
        private TimeSpan timestamp;

        [ObservableProperty]
        private string? lyric;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Weight))]
        [NotifyPropertyChangedFor(nameof(Opacity))]
        [NotifyPropertyChangedFor(nameof(Transform))]
        private LyricState state = LyricState.Next;

        public FontWeight Weight => State == LyricState.Current ? FontWeight.Bold : FontWeight.Normal;

        //public Brush? Foreground
        //{
        //    get
        //    {
        //        switch (State)
        //        {
        //            case LyricState.Past:
        //            case LyricState.Current:
        //            case LyricState.Untimed:
        //                if (Application.Current.TryFindResource("PrimaryTextColor", mainView.MainWindow.ActualThemeVariant, out object? brush))
        //                    return (Brush)brush;
        //                break;
                    
        //            case LyricState.Next:    
        //                if (Application.Current.TryFindResource("SecondaryColor", mainView.MainWindow.ActualThemeVariant, out object? brush2))
        //                    return (Brush)brush2;
        //                break;
        //        }

        //        return null;
        //    }
        //}

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

        private readonly MainViewModel mainView;
        public LyricLineViewModel(MainViewModel mainView)
        {
            this.mainView = mainView;
        }
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
            var filetoRead = Path.Combine(Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path) + ".lrc");
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
