using ATL;
using ATL.Playlist;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Handlers.Integrations;
using FRESHMusicPlayer.Views;
using LiteDB;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public ConfigurationFile Config { get; private set; }
        public Track CurrentTrack { get; private set; }
        public IntegrationHandler Integrations { get; private set; } = new();

        private bool pauseAfterCurrentTrack = false;

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

        public const string ProjectName = "FRESHMusicPlayer for Mac and Linux Beta 9";
        private string windowTitle = ProjectName;
        public string WindowTitle
        {
            get => windowTitle;
            set => this.RaiseAndSetIfChanged(ref windowTitle, value);
        }

        #region Core
        private void Player_SongException(object sender, PlaybackExceptionEventArgs e)
        {
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

            if (Config.ShowTimeInWindow) WindowTitle = $"{CurrentTime:mm\\:ss}/{TotalTime:mm\\:ss} | {ProjectName}";

            Player.AvoidNextQueue = false;
        }

        private async void Player_SongChanged(object sender, EventArgs e)
        {
            LoggingHandler.Log("Player: SongChanged");
            CurrentTrack = new Track(Player.FilePath);
            Artist = CurrentTrack.Artist;
            Title = CurrentTrack.Title;
            if (CurrentTrack.EmbeddedPictures.Count != 0)
                CoverArt = new Bitmap(new MemoryStream(CurrentTrack.EmbeddedPictures[0].PictureData));
            this.RaisePropertyChanged(nameof(TotalTime));
            this.RaisePropertyChanged(nameof(TotalTimeSeconds));
            WindowTitle = $"{CurrentTrack.Artist} - {CurrentTrack.Title} | {ProjectName}";
            ProgressTimer.Start();
            Integrations.Update(CurrentTrack, PlaybackStatus.Playing);

            await Task.Delay(50); // HACK: this whole thing is a massive HACK.
            if (pauseAfterCurrentTrack && !Player.Paused)
            {
                PlayPauseCommand();
                pauseAfterCurrentTrack = false;
            }

            await Task.Delay(1000);
            this.RaisePropertyChanged(nameof(TotalTime));
            this.RaisePropertyChanged(nameof(TotalTimeSeconds));
            Volume = Player.Volume;
           
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
        public bool Shuffle
        {
            get => Player.Queue.Shuffle;
            set => Player.Queue.Shuffle = value;
        }

        public void SkipPreviousCommand()
        {
            if (Player.CurrentTime.TotalSeconds <= 5) Player.PreviousSong();
            else
            {
                if (!Player.FileLoaded) return;
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
        }
        public void PlayPauseCommand()
        {
            if (Player.Paused)
            {
                Player.ResumeMusic();
                Paused = false;
                Integrations.Update(CurrentTrack, PlaybackStatus.Playing);
            }
            else
            {
                Player.PauseMusic();
                Paused = true;
                Integrations.Update(CurrentTrack, PlaybackStatus.Paused);
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
        public void PauseAfterCurrentTrackCommand() => pauseAfterCurrentTrack = true;

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
        public void UpdateLibraryInfo() => LibraryInfoText = $"Tracks: {AllTracks?.Count} ・ {TimeSpan.FromSeconds(AllTracks.Sum(x => x.Length)):hh\\:mm\\:ss}";

        public async void StartThings()
        {
            LoggingHandler.Log("Hi! I'm FMP!\n" +
            $"{ProjectName}\n" +
            $"{RuntimeInformation.FrameworkDescription}\n" +
            $"{Environment.OSVersion.VersionString}\n");
            Player.SongChanged += Player_SongChanged;
            Player.SongStopped += Player_SongStopped;
            Player.SongException += Player_SongException;
            ProgressTimer.Elapsed += ProgressTimer_Elapsed; // TODO: put this in a more logical place
            LoggingHandler.Log("Handling config...");
            Config = Program.Config; // HACK: this is a hack
            Volume = Config?.Volume ?? 1f;

            LoggingHandler.Log("Handling command line args...");
            var args = Environment.GetCommandLineArgs().ToList();
            args.RemoveRange(0, 1);
            if (args.Count != 0)
            {
                Player.Queue.Add(args.ToArray());
                Player.PlayMusic();
            }
            else
            {
                if (!string.IsNullOrEmpty(Config.FilePath))
                {
                    pauseAfterCurrentTrack = true;
                    Player.PlayMusic(Config.FilePath);
                    Player.CurrentTime.Add(TimeSpan.FromSeconds(Config.FilePosition));
                }
            }
            await Dispatcher.UIThread.InvokeAsync(() => SelectedTab = Config.CurrentTab, DispatcherPriority.ApplicationIdle); // TODO: unhack the hack

            if (Config.IntegrateDiscordRPC)
                Integrations.Add(new DiscordIntegration());
            if (Config.PlaybackTracking)
                Integrations.Add(new PlaytimeLoggingIntegration(Player));
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Config.IntegrateMPRIS)
                Integrations.Add(new MPRISIntegration(this, Window));
        }

        public async void CloseThings()
        {
            LoggingHandler.Log("FMP is shutting down!");
            Library?.Database.Dispose();
            Integrations.Dispose();
            Config.Volume = Volume;
            Config.CurrentTab = SelectedTab;
            if (Player.FileLoaded)
            {
                Config.FilePath = Player.FilePath;
                Config.FilePosition = Player.CurrentTime.TotalSeconds;
            }
            else
            {
                Config.FilePath = null;
            }
            await ConfigurationHandler.Write(Config);
            LoggingHandler.Log("Goodbye!");
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
                if (files.Length > 0) await Task.Run(() => Library.Import(files));
            }
            
        }
        public async void BrowsePlaylistFilesCommand()
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
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var files = await dialog.ShowAsync(desktop.MainWindow);
                IPlaylistIO reader = PlaylistIOFactory.GetInstance().GetPlaylistIO(files[0]);
                foreach (string s in reader.FilePaths)
                {
                    if (!File.Exists(s))
                        continue; // TODO: show something to the user
                }
                Player.Queue.Add(reader.FilePaths.ToArray());
                await Task.Run(() => Library.Import(reader.FilePaths.ToArray()));
                Player.PlayMusic();
            }
        }
        public async void BrowseFoldersCommand()
        {
            var dialog = new OpenFolderDialog()
            {
                
            };
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var directory = await dialog.ShowAsync(desktop.MainWindow);
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
            new Views.Settings().SetThings(Config).Show(Window);
        }

        public void OpenQueueManagementCommand()
        {
            new Views.QueueManagement().SetStuff(Player, Library, ProgressTimer).Show(Window);
        }
        #endregion
    }
}
