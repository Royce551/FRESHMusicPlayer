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

namespace FRESHMusicPlayer.ViewModels
{
    public class QueueManagementViewModel : ViewModelBase
    {
        public Player Player { get; set; }
        public Library Library { get; set; }
        public Timer ProgressTimer { get; set; }

        public ObservableCollection<QueueManagementEntry> Queue { get; set; } = new();

        private bool isReady = false;

        public void Update()
        {
            isReady = false;
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
            isReady = true;
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
