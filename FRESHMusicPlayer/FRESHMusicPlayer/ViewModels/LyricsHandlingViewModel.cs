using Avalonia.Media.TextFormatting;
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
    public abstract partial class LyricsHandlingViewModel : ViewModelBase
    {
        [ObservableProperty]
        public partial ObservableCollection<LyricLineViewModel>? Lyrics { get; set; } = new ObservableCollection<LyricLineViewModel>();

        public abstract void OnCurrentLineChanged();

        public void UpdateLyrics()
        {
            if (File.Exists(Path.Combine(Path.GetDirectoryName(MainView.Player.FilePath)!, Path.GetFileNameWithoutExtension(MainView.Player.FilePath) + ".lrc")))
            {
                Lyrics = new ObservableCollection<LyricLineViewModel>(new LRCTimedLyricsProvider(MainView.Player.FilePath).Lines.Select(x => new LyricLineViewModel(this) { Timestamp = x.Key, Lyric = x.Value }));
            }
            else if (MainView.Player.Metadata is FileMetadataProvider provider && !string.IsNullOrWhiteSpace(provider.ATLTrack.Lyrics.UnsynchronizedLyrics))
            {
                Lyrics = new ObservableCollection<LyricLineViewModel>(provider.ATLTrack.Lyrics.UnsynchronizedLyrics.Split(["\r\n", "\r", "\n"], StringSplitOptions.None).Select(x => new LyricLineViewModel(this) { Timestamp = TimeSpan.Zero, Lyric = x, State = LyricState.Untimed }));
            }
            else Lyrics = null;
        }

        public List<LyricLineViewModel>? CurrentLines { get; private set; }
        public List<LyricLineViewModel>? NextLines { get; private set; }

        public void TickLyrics()
        {
            if (Lyrics is null || !MainView.Player.FileLoaded) return;

            var currentTime = MainView.Player.CurrentBackend.CurrentTime;

            CurrentLines = Lyrics.Where(x => x.Timestamp < currentTime).ToList();
            NextLines = Lyrics.Where(x => x.Timestamp > currentTime).Reverse().ToList();

            foreach (var line in NextLines)
            {
                if (line.State != LyricState.Next)
                    line.State = LyricState.Next;
            }

            if (CurrentLines.Count != 0)
            {
                for (int i = 0; i < CurrentLines.Count - 1; i++)
                {
                    var line = CurrentLines[i];
                    if (line.State != LyricState.Past)
                        line.State = LyricState.Past;
                }

                var last = CurrentLines.Last();
                if (last.State != LyricState.Current)
                    last.State = LyricState.Current;
            }
        }
    }
}
