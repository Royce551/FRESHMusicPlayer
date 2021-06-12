using FRESHMusicPlayer.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using System.Timers;
using Avalonia.Threading;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ATL.Playlist;
using System.IO;

namespace FRESHMusicPlayer.ViewModels
{
    public class QueueManagementViewModel : ViewModelBase
    {
        public Player Player { get; set; }
        public Library Library { get; set; }
        public Timer ProgressTimer { get; set; }

        public ObservableCollection<QueueManagementEntry> Queue { get; set; } = new();

        private Window Window
        {
            get
            {
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    return desktop.MainWindow;
                else return null;
            }
        }

        public void Update()
        {
            Queue.Clear();
            int number = 1;
            foreach (var track in Player.Queue.Queue)
            {
                var info = Library.GetFallbackTrack(track);
                var entry = new QueueManagementEntry
                {
                    Title = info.Title,
                    Artist = info.Artist,
                    Position = number,
                    Length = info.Length
                };
                entry.IsCurrentTrack = Player.Queue.Position == number;
                Queue.Add(entry);
                number++;
            }
        }

        public void JumpToCommand(int position)
        {
            Player.Queue.Position = position - 1;
            Player.PlayMusic();
        }
        public void RemoveCommand(int position)
        {
            Player.Queue.Remove(position - 1);
        }

        private List<string> acceptableFilePaths = "wav;aiff;mp3;wma;3g2;3gp;3gp2;3gpp;asf;wmv;aac;adts;avi;m4a;m4a;m4v;mov;mp4;sami;smi;flac".Split(';').ToList();
        public async void AddTrackCommand()
        {
            var dialog = new OpenFileDialog()
            {
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter()
                    {
                        Name = "Audio Files",
                        Extensions = acceptableFilePaths
                    },
                    new FileDialogFilter()
                    {
                        Name = "Other",
                        Extensions = new List<string>() { "*" }
                    }
                },
                AllowMultiple = true
            };
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var files = await dialog.ShowAsync(desktop.MainWindow);
                if (files.Length > 0) Player.Queue.Add(files);
            }
        }
        public async void AddPlaylistCommand()
        {
            var dialog = new OpenFileDialog()
            {
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter()
                    {
                        Name = "Playlist Files",
                        Extensions = new(){ "xspf", "asx", "wvx", "b4s", "m3u", "m3u8", "pls", "smil", "smi", "zpl"}
                    }
                }
            };
            var files = await dialog.ShowAsync(Window);
            IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(files[0]);
            foreach (string s in reader.FilePaths)
            {
                if (!File.Exists(s))
                    continue; // TODO: show something to the user
            }
            Player.Queue.Add(reader.FilePaths.ToArray());
        }
        public void ClearQueueCommand() => Player.Queue.Clear();

        private TimeSpan timeRemaining = new();
        public TimeSpan TimeRemaining
        {
            get => timeRemaining;
            set => this.RaiseAndSetIfChanged(ref timeRemaining, value);
        }

        public void StartThings()
        {
            Player.Queue.QueueChanged += Queue_QueueChanged;
            Player.SongChanged += Player_SongChanged;
            Player.SongStopped += Player_SongStopped;
            ProgressTimer.Elapsed += ProgressTimer_Elapsed;
            Update();
        }

        private async void ProgressTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                
                TimeSpan x = new();
                int i = 1;
                int i2 = 0;
                foreach (var track in Player.Queue.Queue)
                {
                    i++;
                    if (i <= Player.Queue.Position) continue;
                    var y = Queue[i2];
                    x += TimeSpan.FromSeconds(y.Length);
                    i2++;
                }
                x -= Player.CurrentTime;
                TimeRemaining = x;
            });
        }

        private void Player_SongStopped(object sender, EventArgs e) => Update();

        private void Player_SongChanged(object sender, EventArgs e) => Update();

        public void CloseThings()
        {
            Player.Queue.QueueChanged -= Queue_QueueChanged;
            Player.SongChanged -= Player_SongChanged;
            Player.SongStopped -= Player_SongStopped;
            ProgressTimer.Elapsed -= ProgressTimer_Elapsed;
        }

        private void Queue_QueueChanged(object sender, EventArgs e) => Update();
    }
    
    public class QueueManagementEntry
    {
        public string Title { get; init; }
        public string Artist { get; init; }
        public int Position { get; init; }
        public bool IsCurrentTrack { get; set; }
        public int Length { get; init; }
    }
}
