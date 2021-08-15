using ATL;
using ATL.Playlist;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
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
    public class MainWindowViewModel : ViewModelBase
    {
        public Player Player { get; private set; }
        public Timer ProgressTimer { get; private set; } = new(100);
        public Library Library { get; private set; }
        public Track CurrentTrack { get; private set; }
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
            var library = new LiteDatabase($"Filename=\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "database.fdb2")}\";Connection=shared");
            Library = new Library(library);
            InitializeLibrary();
        }

        public const string ProjectName = "FRESHMusicPlayer for Mac and Linux Release Candidate 1";
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
            CurrentTrack = new Track(Player.FilePath);
            Artist = string.IsNullOrEmpty(CurrentTrack.Artist) ? Resources.UnknownArtist : CurrentTrack.Artist;
            Title = string.IsNullOrEmpty(CurrentTrack.Title) ? Resources.UnknownTitle : CurrentTrack.Title;
            if (CurrentTrack.EmbeddedPictures.Count != 0)
                CoverArt = new Bitmap(new MemoryStream(CurrentTrack.EmbeddedPictures[0].PictureData));
            this.RaisePropertyChanged(nameof(TotalTime));
            this.RaisePropertyChanged(nameof(TotalTimeSeconds));
            WindowTitle = $"{CurrentTrack.Artist} - {CurrentTrack.Title} | {ProjectName}";
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

        public void SkipPreviousCommand()
        {
            if (!Player.FileLoaded) return;
            if (Player.CurrentTime.TotalSeconds <= 5) Player.PreviousSong();
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
                Player.ResumeMusic();
                Integrations.Update(CurrentTrack, PlaybackStatus.Playing);
            }
            else
            {
                Player.PauseMusic();
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
        public void SkipNextCommand()
        {
            Player.NextSong();
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

        #region Library

        public async void InitializeLibrary()
        {
            LoggingHandler.Log("Showing library!");
            AllTracks?.Clear();
            CategoryThings?.Clear();
            switch (SelectedTab)
            {
                case 0:
                    foreach (var track in await Task.Run(() => Library.Read()))
                        AllTracks.Add(track);
                    break;
                case 1:
                    foreach (var artist in await Task.Run(() => Library.Read("Artist").Select(x => x.Artist).Distinct()))
                        CategoryThings.Add(artist);
                    break;
                case 2:
                    foreach (var album in await Task.Run(() => Library.Read("Album").Select(x => x.Album).Distinct()))
                        CategoryThings.Add(album);
                    break;
                case 3:
                    foreach (var playlist in await Task.Run(() => Library.Database.GetCollection<DatabasePlaylist>("playlists").Query().OrderBy("Name").ToEnumerable()))
                        CategoryThings.Add(playlist.Name);
                    break;
            }
            UpdateLibraryInfo();
        }
        public void UpdateLibraryInfo() => LibraryInfoText = $"{Resources.Tracks}: {AllTracks?.Count} ・ {TimeSpan.FromSeconds(AllTracks.Sum(x => x.Length)):hh\\:mm\\:ss}";

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
                Player.PlayMusic();
            }
            else
            {
                if (!string.IsNullOrEmpty(Program.Config.FilePath))
                {
                    PauseAfterCurrentTrack = true;
                    Player.PlayMusic(Program.Config.FilePath);
                    Player.CurrentTime.Add(TimeSpan.FromSeconds(Program.Config.FilePosition));
                }
            }
            await Dispatcher.UIThread.InvokeAsync(() => SelectedTab = Program.Config.CurrentTab, DispatcherPriority.ApplicationIdle);
            // this delays the tab switch until avalonia is ready
            
            HandleIntegrations();

            (GetMainWindow() as MainWindow).RootPanel.Opacity = 1; // this triggers the startup fade
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
            this.RaisePropertyChanged(nameof(VisibleNotifications));
            this.RaisePropertyChanged(nameof(AreThereAnyNotifications));
            foreach (Notification box in Notifications.Notifications)
            {
                if (box.DisplayAsToast && !box.Read)
                {
                    var button = Window.FindControl<Button>("NotificationButton");
                    button.ContextFlyout.ShowAt(button);
                }
            }
        }

        public async void CloseThings()
        {
            LoggingHandler.Log("FMP is shutting down!");
            Library?.Database.Dispose();
            Integrations.Dispose();
            Program.Config.Volume = Volume;
            Program.Config.CurrentTab = SelectedTab;
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
            var notification = new Notification()
            {
                ContentText = Properties.Resources.Notification_Scanning
            };
            Notifications.Add(notification);
            var filesToImport = new List<string>();
            var library = Library.Read();
            await Task.Run(() =>
            {
                foreach (var folder in Program.Config.AutoImportPaths)
                {
                    var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                        .Where(name => name.EndsWith(".mp3")
                            || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
                            || name.EndsWith(".flac") || name.EndsWith(".aiff")
                            || name.EndsWith(".wma")
                            || name.EndsWith(".aac")).ToArray();
                    foreach (var file in files)
                    {
                        if (!library.Select(x => x.Path).Contains(file))
                            filesToImport.Add(file);
                    }
                }
                Library.Import(filesToImport);
            });
            Notifications.Remove(notification);
        }

        private int selectedTab;
        public int SelectedTab
        {
            get => selectedTab;
            set
            {
                this.RaiseAndSetIfChanged(ref selectedTab, value);
                InitializeLibrary();
            }
        }

        public ObservableCollection<DatabaseTrack> AllTracks { get; set; } = new();
        public ObservableCollection<string> CategoryThings { get; set; } = new();

        private string libraryInfoText;
        public string LibraryInfoText
        {
            get => libraryInfoText;
            set => this.RaiseAndSetIfChanged(ref libraryInfoText, value);
        }

        private string artistsSelectedItem;
        public string ArtistsSelectedItem
        {
            get => artistsSelectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref artistsSelectedItem, value);
                ShowTracksForArtist(value);
            }
        }
        public async void ShowTracksForArtist(string artist)
        {
            if (artist is null) return;
            AllTracks?.Clear();
            foreach (var track in await Task.Run(() => Library.ReadTracksForArtist(artist)))
                AllTracks?.Add(track);
            UpdateLibraryInfo();
        }

        private string albumsSelectedItem;
        public string AlbumsSelectedItem
        {
            get => albumsSelectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref albumsSelectedItem, value);
                ShowTracksForAlbum(value);
            }
        }
        public async void ShowTracksForAlbum(string album)
        {
            if (album is null) return;
            AllTracks?.Clear();
            foreach (var track in await Task.Run(() => Library.ReadTracksForAlbum(album)))
                AllTracks?.Add(track);
            UpdateLibraryInfo();
        }

        private string playlistsSelectedItem;
        public string PlaylistsSelectedItem
        {
            get => playlistsSelectedItem;
            set
            {
                this.RaiseAndSetIfChanged(ref playlistsSelectedItem, value);
                ShowTracksForPlaylist(value);
            }
        }
        public async void ShowTracksForPlaylist(string playlist)
        {
            if (playlist is null) return;
            AllTracks.Clear();
            foreach (var track in await Task.Run(() => Library.ReadTracksForPlaylist(playlist)))
                AllTracks.Add(track);
            UpdateLibraryInfo();
        }

        public void PlayCommand(string path)
        {
            Player.Queue.Clear();
            Player.Queue.Add(path);
            Player.PlayMusic();
        }
        public void EnqueueCommand(string path)
        {
            Player.Queue.Add(path);
        }
        public void DeleteCommand(string path)
        {
            Library.Remove(path);
            InitializeLibrary();
        }
        public void EnqueueAllCommand()
        {
            Player.Queue.Add(AllTracks.Select(x => x.Path).ToArray());
        }
        public void PlayAllCommand()
        {
            Player.Queue.Clear();
            Player.Queue.Add(AllTracks.Select(x => x.Path).ToArray());
            Player.PlayMusic();
        }

        // Searching
        public ObservableCollection<DatabaseTrack> SearchTracks { get; set; } = new();
        private string searchTerm;
        public string SearchTerm
        {
            get => searchTerm;
            set
            {
                this.RaiseAndSetIfChanged(ref searchTerm, value);
                PerformSearch(value);
            }
        }

        private Queue<string> searchQueries = new();
        private bool isSearchOperationRunning = false;
        private async void PerformSearch(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                SearchTracks.Clear();
                return;
            }
            searchQueries.Enqueue(query.ToUpper());
            async Task GetResults()
            {
                isSearchOperationRunning = true;
                var query = searchQueries.Dequeue();
                SearchTracks.Clear();
                await Task.Run(() =>
                {
                    foreach (var thing in Library.Database.GetCollection<DatabaseTrack>("tracks")
                    .Query()
                    .Where(x => x.Title.ToUpper().Contains(query) || x.Artist.ToUpper().Contains(query) || x.Album.ToUpper().Contains(query))
                    .OrderBy("Title")
                    .ToList())
                    {
                        if (searchQueries.Count > 1) break;
                        SearchTracks.Add(thing);
                    }
                });
                isSearchOperationRunning = false;
                if (searchQueries.Count != 0) await GetResults();
            }
            if (!isSearchOperationRunning) await GetResults();
        }
        public void ClearSearchCommand() => SearchTerm = string.Empty;

        // Import Tab
        private string filePathOrURL;
        public string FilePathOrURL
        {
            get => filePathOrURL;
            set => this.RaiseAndSetIfChanged(ref filePathOrURL, value);
        }

        private List<string> acceptableFilePaths = "wav;aiff;mp3;wma;3g2;3gp;3gp2;3gpp;asf;wmv;aac;adts;avi;m4a;m4a;m4v;mov;mp4;sami;smi;flac".Split(';').ToList();
        // ripped directly from fmp-wpf 'cause i'm lazy
        public async void BrowseTracksCommand()
        {
            if (await FreedesktopPortal.IsPortalAvailable())
            {
                var files = await FreedesktopPortal.OpenFiles(Resources.Import, new Dictionary<string, object>()
                {
                    {"multiple", true},
                    {"accept_label", Resources.Import},
                    {"filters", new[]
                    {
                        (Resources.FileFilter_AudioFiles, acceptableFilePaths.Select(type => (0u, "*." + type)).ToArray()),
                        (Resources.FileFilter_Other, new[]
                        {
                            (0u, "*")
                        })
                    }}
                });
                if (files.Length != 0) await Task.Run(() => Library.Import(files));
                return;
            }

            var dialog = new OpenFileDialog()
            {
                Filters = new List<FileDialogFilter>
                {
                    new FileDialogFilter()
                    {
                        Name = Resources.FileFilter_AudioFiles,
                        Extensions = acceptableFilePaths
                    },
                    new FileDialogFilter()
                    {
                        Name = Resources.FileFilter_Other,
                        Extensions = new List<string>() { "*" }
                    }
                },
                AllowMultiple = true
            };
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var files = await dialog.ShowAsync(desktop.MainWindow);
                if (files.Length > 0) await Task.Run(() => Library.Import(files));
            }

        }
        public async void BrowsePlaylistFilesCommand()
        {
            string[] acceptableFiles = { "xspf", "asx", "wvx", "b4s", "m3u", "m3u8", "pls", "smil", "smi", "zpl" };
            string[] files = null;

            if (await FreedesktopPortal.IsPortalAvailable())
            {
                files = await FreedesktopPortal.OpenFiles(Resources.ImportPlaylistFiles, new Dictionary<string, object>()
                {
                    {"multiple", true},
                    {"accept_label", Resources.ImportPlaylistFiles},
                    {"filters", new[]
                    {
                        (Resources.FileFilter_PlaylistFiles, acceptableFiles.Select(type => (0u, "*." + type)).ToArray()),
                    }}
                });
            }

            if (files == null)
            {
                var dialog = new OpenFileDialog()
                {
                    Filters = new List<FileDialogFilter>
                    {
                        new FileDialogFilter()
                        {
                            Name = Resources.FileFilter_PlaylistFiles,
                            Extensions = acceptableFiles.ToList()
                        }
                    }
                };

                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    files = await dialog.ShowAsync(desktop.MainWindow);
                }
            }

            if (files is not { Length: > 0 }) return;

            var reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(files[0]);
            foreach (var s in reader.FilePaths)
            {
                if (!File.Exists(s))
                {
                    Notifications.Add(new()
                    {
                        ContentText = string.Format(Properties.Resources.Notification_FileInPlaylistMissing,
                            Path.GetFileName(s)),
                        DisplayAsToast = true,
                        IsImportant = true,
                        Type = NotificationType.Failure
                    });
                    continue;
                }
            }

            Player.Queue.Add(reader.FilePaths.ToArray());
            await Task.Run(() => Library.Import(reader.FilePaths.ToArray()));
            Player.PlayMusic();
        }
        public async void BrowseFoldersCommand()
        {
            string directory = null;
            if (await FreedesktopPortal.IsPortalAvailable())
            {
                var result = await FreedesktopPortal.OpenFiles(Resources.ImportFolders, new Dictionary<string, object>()
                {
                    {"multiple", true},
                    {"accept_label", Resources.ImportFolders},
                    {"directory", true}
                });

                if (result.Length == 1)
                {
                    directory = result[0];
                }
                else
                {
                    //The dialog was closed
                    return;
                }
            }

            if (directory == null)
            {
                var dialog = new OpenFolderDialog()
                {

                };
                if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    directory = await dialog.ShowAsync(desktop.MainWindow);
                }
            }

            if (directory != null)
            {
                var paths = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(name => name.EndsWith(".mp3")
                        || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
                        || name.EndsWith(".flac") || name.EndsWith(".aiff")
                        || name.EndsWith(".wma")
                        || name.EndsWith(".aac")).ToArray();
                Player.Queue.Add(paths);
                await Task.Run(() => Library.Import(paths));
                Player.PlayMusic();
            }
        }
        public async void OpenTrackCommand()
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
            var files = await dialog.ShowAsync(Window);
            if (files.Length > 0)
            {
                Player.Queue.Add(files);
                Player.PlayMusic();
            }
        }
        public void ImportFilePathCommand()
        {
            if (string.IsNullOrEmpty(FilePathOrURL)) return;
            Player.Queue.Add(FilePathOrURL);
            Library.Import(FilePathOrURL);
            Player.PlayMusic();
        }

        public async void GoToArtistCommand()
        {
            if (CurrentTrack is null) return;
            SelectedTab = 1;
            await Task.Delay(100);
            ShowTracksForArtist(CurrentTrack.Artist);
        }
        public async void GoToAlbumCommand()
        {
            if (CurrentTrack is null) return;
            SelectedTab = 2;
            await Task.Delay(100);
            ShowTracksForAlbum(CurrentTrack.Album);
        }
        #endregion

        #region NavBar
        public void OpenSettingsCommand()
        {
            new Views.Settings().SetThings(Program.Config, Library).Show(Window);
        }

        public void OpenQueueManagementCommand()
        {
            new QueueManagement().SetStuff(Player, Library, ProgressTimer).Show(Window);
        }

        public void OpenPlaylistManagementCommand()
        {
            new PlaylistManagement().SetStuff(this, Player.FilePath ?? null).Show(Window);
        }

        public void OpenLyricsCommand()
        {
            new Lyrics().SetStuff(this).Show(Window);
        }

        public void OpenTagEditorCommand()
        {
            var tracks = new List<string>();
            if (Player.FileLoaded) tracks.Add(Player.FilePath);
            else tracks = Player.Queue.Queue;
            new Views.TagEditor.TagEditor().SetStuff(Player, Library).SetInitialFiles(tracks).Show();
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
