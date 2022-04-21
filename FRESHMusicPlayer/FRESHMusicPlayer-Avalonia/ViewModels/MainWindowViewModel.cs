using ATL;
using ATL.Playlist;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Integrations;
using FRESHMusicPlayer.Handlers.Notifications;
using FRESHMusicPlayer.Properties;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.Views;
using LiteDB;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;

namespace FRESHMusicPlayer.ViewModels
{
    public enum Tab
    {
        Tracks,
        Artists,
        Albums,
        Playlists,
        Import,
        Fullscreen
    }
    public enum Pane
    {
        None,
        Settings,
        QueueManagement,
        Search,
        Notifications,
        TrackInfo,
        Lyrics
    }

    public class MainWindowViewModel : ViewModelBase
    {
        public Player Player { get; private set; }
        public Timer ProgressTimer { get; private set; } = new(100);
        public Library Library { get; private set; }
        public IMetadataProvider CurrentTrack { get; private set; }
        public IntegrationHandler Integrations { get; private set; } = new();
        public NotificationHandler Notifications { get; private set; } = new();

        private Window Window
        {
            get
            {
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                    return desktop.MainWindow;
                else return null;
            }
        }

        public MainWindowViewModel()
        {
            Player = new();
            StartThings();
#if DEBUG // allow multiple instances of FMP in debug (at the expense of stability with heavy library use)
            var library = new LiteDatabase($"Filename=\"{Path.Combine(App.DataFolderLocation, "database.fdb2")}\";Connection=shared");
#elif !DEBUG
            var library = new LiteDatabase(Path.Combine(App.DataFolderLocation, "database.fdb2"));
#endif
            Library = new Library(library);
        }

        public const string ProjectName = "FRESHMusicPlayer";
        private string windowTitle = ProjectName;
        public string WindowTitle
        {
            get => windowTitle;
            set => this.RaiseAndSetIfChanged(ref windowTitle, value);
        }

        public ObservableCollection<Notification> VisibleNotifications => new(Notifications.Notifications);
        public bool AreThereAnyNotifications => Notifications.Notifications.Count > 0;

        public void ClearAllNotificationsCommand() => Notifications.ClearAll();

#region Core
        private void Player_SongException(object sender, PlaybackExceptionEventArgs e)
        {
            Notifications.Add(new()
            {
                ContentText = string.Format(Properties.Resources.Notification_PlaybackError, e.Details),
                DisplayAsToast = true,
                IsImportant = true,
                Type = NotificationType.Failure
            });
            SkipNextCommand();
            LoggingHandler.Log($"Player: An exception was thrown: {e.Exception}");
        }

        private void Player_SongStopped(object sender, EventArgs e)
        {
            LoggingHandler.Log("Player: Stopping!");
            Artist = Properties.Resources.NothingPlaying;
            Title = Properties.Resources.NothingPlaying;
            CoverArt = null;
            WindowTitle = ProjectName;
            ProgressTimer.Stop();
            Integrations.Update(CurrentTrack, PlaybackStatus.Stopped);
        }

        private async void ProgressTimer_Elapsed(object sender, ElapsedEventArgs e) => await Dispatcher.UIThread.InvokeAsync(() => ProgressTick());

        public void ProgressTick()
        {
            this.RaisePropertyChanged(nameof(CurrentTime));
            this.RaisePropertyChanged(nameof(CurrentTimeSeconds));

            if (Program.Config.ShowTimeInWindow) WindowTitle = $"{CurrentTime:mm\\:ss}/{TotalTime:mm\\:ss} | {ProjectName}";
            if (Program.Config.ShowRemainingProgress) this.RaisePropertyChanged(nameof(TotalTime));

            Player.AvoidNextQueue = false;
        }

        private void Player_SongChanged(object sender, EventArgs e)
        {
            LoggingHandler.Log("Player: SongChanged");
            CurrentTrack = Player.Metadata;
            Artist = string.IsNullOrEmpty(string.Join(", ", CurrentTrack.Artists)) ? Resources.UnknownArtist : string.Join(", ", CurrentTrack.Artists);
            Title = string.IsNullOrEmpty(CurrentTrack.Title) ? Resources.UnknownTitle : CurrentTrack.Title;
            if (CurrentTrack.CoverArt is not null)
                CoverArt = new Bitmap(new MemoryStream(CurrentTrack.CoverArt));
            this.RaisePropertyChanged(nameof(TotalTime));
            this.RaisePropertyChanged(nameof(TotalTimeSeconds));
            WindowTitle = $"{string.Join(", ", CurrentTrack.Artists)} - {CurrentTrack.Title} | {ProjectName}";
            ProgressTimer.Start();
            Integrations.Update(CurrentTrack, PlaybackStatus.Playing);
            UpdatePausedState();

            if (PauseAfterCurrentTrack && !Player.Paused)
            {
                PlayPauseCommand();
                PauseAfterCurrentTrack = false;
            }
        }

        public bool RepeatModeNone { get => Player.Queue.RepeatMode == RepeatMode.None; }
        public bool RepeatModeAll { get => Player.Queue.RepeatMode == RepeatMode.RepeatAll; }
        public bool RepeatModeOne { get => Player.Queue.RepeatMode == RepeatMode.RepeatOne; }
        public RepeatMode RepeatMode
        {
            get => Player.Queue.RepeatMode;
            set
            {
                Player.Queue.RepeatMode = value;
            }
        }
        private bool paused = false;
        public bool Paused
        {
            get => paused;
            set => this.RaiseAndSetIfChanged(ref paused, value);
        }
        public bool Shuffle
        {
            get => Player.Queue.Shuffle;
            set => Player.Queue.Shuffle = value;
        }

        public async void SkipPreviousCommand()
        {
            if (!Player.FileLoaded) return;
            if (Player.CurrentTime.TotalSeconds <= 5) await Player.PreviousAsync();
            else
            {
                Player.CurrentTime = TimeSpan.FromSeconds(0);
                ProgressTimer.Start(); // to resync the progress timer
            }
        }
        public void RepeatCommand()
        {
            if (Player.Queue.RepeatMode == RepeatMode.None)
            {
                Player.Queue.RepeatMode = RepeatMode.RepeatAll;
            }
            else if (Player.Queue.RepeatMode == RepeatMode.RepeatAll)
            {
                Player.Queue.RepeatMode = RepeatMode.RepeatOne;
            }
            else
            {
                Player.Queue.RepeatMode = RepeatMode.None;
            }
            this.RaisePropertyChanged(nameof(RepeatModeNone));
            this.RaisePropertyChanged(nameof(RepeatModeAll));
            this.RaisePropertyChanged(nameof(RepeatModeOne));
        }
        public void PlayPauseCommand()
        {
            if (Player.Paused)
            {
                Player.Resume();
                Integrations.Update(CurrentTrack, PlaybackStatus.Playing);
            }
            else
            {
                Player.Pause();
                Integrations.Update(CurrentTrack, PlaybackStatus.Paused);
            }
            UpdatePausedState();
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
            this.RaisePropertyChanged(nameof(Shuffle));
        }
        public async void SkipNextCommand()
        {
            await Player.NextAsync();
        }
        public void PauseAfterCurrentTrackCommand() => PauseAfterCurrentTrack = !PauseAfterCurrentTrack;

        public void ShowRemainingProgressCommand() => Program.Config.ShowRemainingProgress = !Program.Config.ShowRemainingProgress;

        private void UpdatePausedState() => Paused = Player.Paused;

        private TimeSpan currentTime;
        public TimeSpan CurrentTime
        {
            get
            {
                if (Player.FileLoaded)
                    return Player.CurrentTime;
                else return TimeSpan.Zero;

            }
            set
            {
                this.RaiseAndSetIfChanged(ref currentTime, value);
            }
        }

        private double currentTimeSeconds;
        public double CurrentTimeSeconds
        {
            get
            {
                if (Player.FileLoaded)
                    return Player.CurrentTime.TotalSeconds;
                else return 0;

            }
            set
            {
                if (TimeSpan.FromSeconds(value) >= TotalTime) return;
                Player.CurrentTime = TimeSpan.FromSeconds(value);
                ProgressTick();
                this.RaiseAndSetIfChanged(ref currentTimeSeconds, value);
            }
        }
        private TimeSpan totalTime;
        public TimeSpan TotalTime
        {
            get
            {
                if (Player.FileLoaded)
                    return Player.TotalTime;
                else return TimeSpan.Zero;
            }
            set
            {
                this.RaiseAndSetIfChanged(ref totalTime, value);
            }
        }
        private double totalTimeSeconds;
        public double TotalTimeSeconds
        {
            get
            {
                if (Player.FileLoaded)
                    return Player.TotalTime.TotalSeconds;
                else return 0;
            }
            set => this.RaiseAndSetIfChanged(ref totalTimeSeconds, value);
        }

        private Bitmap coverArt;
        public Bitmap CoverArt
        {
            get => coverArt;
            set => this.RaiseAndSetIfChanged(ref coverArt, value);
        }

        private string artist = Properties.Resources.NothingPlaying;
        public string Artist
        {
            get => artist;
            set => this.RaiseAndSetIfChanged(ref artist, value);
        }
        private string title = Properties.Resources.NothingPlaying;
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

        private bool pauseAfterCurrentTrack = false;
        public bool PauseAfterCurrentTrack
        {
            get => pauseAfterCurrentTrack;
            set => this.RaiseAndSetIfChanged(ref pauseAfterCurrentTrack, value);
        }

        #endregion

        public FontWeight TracksTabWeight => SelectedTab == Tab.Tracks ? FontWeight.Bold : FontWeight.Regular;
        public FontWeight ArtistsTabWeight => SelectedTab == Tab.Artists ? FontWeight.Bold : FontWeight.Regular;
        public FontWeight AlbumsTabWeight => SelectedTab == Tab.Albums ? FontWeight.Bold : FontWeight.Regular;
        public FontWeight PlaylistsTabWeight => SelectedTab == Tab.Playlists ? FontWeight.Bold : FontWeight.Regular;
        public FontWeight ImportTabWeight => SelectedTab == Tab.Import ? FontWeight.Bold : FontWeight.Regular;

        private Tab selectedTab = Tab.Tracks;
        public Tab SelectedTab
        {
            get => selectedTab;
            set
            {
                selectedTab = value;
                this.RaisePropertyChanged(nameof(MainContent));
                this.RaisePropertyChanged(nameof(TracksTabWeight));
                this.RaisePropertyChanged(nameof(ArtistsTabWeight));
                this.RaisePropertyChanged(nameof(AlbumsTabWeight));
                this.RaisePropertyChanged(nameof(PlaylistsTabWeight));
                this.RaisePropertyChanged(nameof(ImportTabWeight));
            }
        }
        private Pane selectedPane = Pane.None;
        public Pane SelectedPane
        {
            get => selectedPane;
            set
            {
                selectedPane = value;
                this.RaisePropertyChanged(nameof(AuxPaneContent));
            }
        }

        public UserControl MainContent
        {
            get
            {
                switch (SelectedTab)
                {
                    case Tab.Tracks:
                        return new LibraryTab().SetStuff(this, SelectedTab, null);
                    case Tab.Artists:
                        return new LibraryTab().SetStuff(this, SelectedTab, null);
                    case Tab.Albums:
                        return new LibraryTab().SetStuff(this, SelectedTab, null);
                    case Tab.Playlists:
                        return new LibraryTab().SetStuff(this, SelectedTab, null);
                    case Tab.Import:
                        return new ImportTab().SetStuff(this);
                    case Tab.Fullscreen:
                        return new UserControl
                        {
                            Content = new TextBlock
                            {
                                Text = "Fullscreen Page"
                            }
                        };
                    default:
                        throw new Exception("???");
                }
            }
        }
        public UserControl AuxPaneContent
        {
            get
            {
                switch (SelectedPane)
                {
                    case Pane.Settings:
                        return new Views.Settings().SetThings(Program.Config, Library);
                    case Pane.QueueManagement:
                        return new Views.QueueManagement().SetStuff(Player, Library, ProgressTimer);
                    case Pane.Search:
                        return new UserControl
                        {
                            Content = new TextBlock
                            {
                                Text = "Search"
                            }
                        };
                    case Pane.TrackInfo:
                        return new TrackInfo().SetStuff(Player);
                    case Pane.Notifications:
                        return new UserControl
                        {
                            Content = new TextBlock
                            {
                                Text = "Notifications"
                            }
                        };
                    case Pane.Lyrics:
                        return new UserControl
                        {
                            Content = new TextBlock
                            {
                                Text = "Lyrics"
                            }
                        };
                    case Pane.None:
                        return null;
                    default:
                        throw new Exception("????");
                }
            }
        }

        private int auxPaneWidth = 0;
        public int AuxPaneWidth
        {
            get => auxPaneWidth;
            set => this.RaiseAndSetIfChanged(ref auxPaneWidth, value);
        }

#region Library
        public async void StartThings()
        {
            LoggingHandler.Log("Hi! I'm FMP!\n" +
            $"{ProjectName}\n" +
            $"{RuntimeInformation.FrameworkDescription}\n" +
            $"{Environment.OSVersion.VersionString}\n");

            Player.SongChanged += Player_SongChanged;
            Player.SongStopped += Player_SongStopped;
            Player.SongException += Player_SongException;
            ProgressTimer.Elapsed += ProgressTimer_Elapsed;
            Notifications.NotificationInvalidate += Notifications_NotificationInvalidate;

            LoggingHandler.Log("Handling config...");

            Volume = Program.Config?.Volume ?? 1f;

            LoggingHandler.Log("Handling command line args...");
            var args = Environment.GetCommandLineArgs().ToList(); // TODO: handle at startup
            args.RemoveRange(0, 1);
            if (args.Count != 0)
            {
                Player.Queue.Add(args.ToArray());
                await Player.PlayAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(Program.Config.FilePath))
                {
                    PauseAfterCurrentTrack = true;
                    await Player.PlayAsync(Program.Config.FilePath);
                    Player.CurrentTime.Add(TimeSpan.FromSeconds(Program.Config.FilePosition));
                }
            }
            //await Dispatcher.UIThread.InvokeAsync(() => SelectedTab = Program.Config.CurrentTab, DispatcherPriority.ApplicationIdle);
            // this delays the tab switch until avalonia is ready
            
            HandleIntegrations();

            //(GetMainWindow() as MainWindow).RootPanel.Opacity = 1; // this triggers the startup fade
            await PerformAutoImport();
        }

        public void HandleIntegrations()
        {
            if (Program.Config.IntegrateDiscordRPC) Integrations.Add(new DiscordIntegration());
            else
            {
                var discord = Integrations.AllIntegrations.Find(x => x is DiscordIntegration);
                if (discord is not null) Integrations.Remove(discord);
            }

            if (Program.Config.PlaybackTracking) Integrations.Add(new PlaytimeLoggingIntegration(Player));
            else
            {
                var playtime = Integrations.AllIntegrations.Find(x => x is PlaytimeLoggingIntegration);
                if (playtime is not null) Integrations.Remove(playtime);
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Program.Config.IntegrateMPRIS) Integrations.Add(new MPRISIntegration(this, Window));
            else
            {
                var mpris = Integrations.AllIntegrations.Find(x => x is MPRISIntegration);
                if (mpris is not null) Integrations.Remove(mpris);
            }
        }

        private void Notifications_NotificationInvalidate(object sender, EventArgs e)
        {
            //this.RaisePropertyChanged(nameof(VisibleNotifications));
            //this.RaisePropertyChanged(nameof(AreThereAnyNotifications));
            //foreach (Notification box in Notifications.Notifications)
            //{
            //    if (box.DisplayAsToast && !box.Read)
            //    {
            //        var button = Window.FindControl<Button>("NotificationButton");
            //        button.ContextFlyout.ShowAt(button);
            //    }
            //}
        }

        public async void CloseThings()
        {
            LoggingHandler.Log("FMP is shutting down!");
            Library?.Database.Dispose();
            Integrations.Dispose();
            Program.Config.Volume = Volume;
            //Program.Config.CurrentTab = SelectedTab;
            if (Player.FileLoaded)
            {
                Program.Config.FilePath = Player.FilePath;
                Program.Config.FilePosition = Player.CurrentTime.TotalSeconds;
            }
            else
            {
                Program.Config.FilePath = null;
            }
            await ConfigurationHandler.Write(Program.Config);
            LoggingHandler.Log("Goodbye!");
        }

        public async Task PerformAutoImport()
        {
            if (Program.Config.AutoImportPaths.Count <= 0) return; // not really needed but prevents going through unneeded
                                                                   // effort (and showing the notification)
                                                                   //var notification = new Notification()
                                                                   //{
                                                                   //    ContentText = Properties.Resources.Notification_Scanning
                                                                   //};
                                                                   //Notifications.Add(notification);
            //var filesToImport = new List<string>();
            //var library = Library.Read();
            //await Task.Run(() =>
            //{
            //    foreach (var folder in Program.Config.AutoImportPaths)
            //    {
            //        var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
            //            .Where(name => name.EndsWith(".mp3")
            //                || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
            //                || name.EndsWith(".flac") || name.EndsWith(".aiff")
            //                || name.EndsWith(".wma")
            //                || name.EndsWith(".aac")).ToArray();
            //        foreach (var file in files)
            //        {
            //            if (!library.Select(x => x.Path).Contains(file))
            //                filesToImport.Add(file);
            //        }
            //    }
            //    Library.Import(filesToImport);
            //});
            //Notifications.Remove(notification);
        }
#endregion

#region NavBar
        public void OpenSettingsCommand()
        {
            SelectedPane = Pane.Settings;
        }

        public void OpenQueueManagementCommand()
        {
            SelectedPane = Pane.QueueManagement;
        }

        public void OpenPlaylistManagementCommand()
        {
            new PlaylistManagement().SetStuff(this, Player.FilePath ?? null).Show(Window);
        }

        public void OpenLyricsCommand()
        {
            SelectedPane = Pane.Lyrics;
        }

        public void OpenTagEditorCommand()
        {
            var tracks = new List<string>();
            if (Player.FileLoaded) tracks.Add(Player.FilePath);
            else tracks = Player.Queue.Queue;
            new Views.TagEditor.TagEditor().SetStuff(Player, Library).SetInitialFiles(tracks).Show(Window);
        }
#endregion
    }

    public class PauseAfterCurrentTrackToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool x)
            {
                if (x) return new SolidColorBrush(Color.FromRgb(213, 70, 63));
                else return Application.Current.FindResource("SecondaryTextColor");
            }
            else throw new Exception("idoit");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TotalTimeDisplayConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            var x = values[0];
            var z = values[1];
            if (x is TimeSpan currentTime && z is TimeSpan totalTime)
            {
                if (Program.Config.ShowRemainingProgress)
                {
                    return $"-{currentTime - totalTime:mm\\:ss}";
                }
                else
                {
                    return $"{totalTime:mm\\:ss}";
                }
            }
            else return "i dunno";
        }

        public object ConvertBack(List<object> value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
