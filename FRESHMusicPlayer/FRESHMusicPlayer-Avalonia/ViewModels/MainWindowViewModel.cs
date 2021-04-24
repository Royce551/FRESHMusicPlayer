using ATL;
using FRESHMusicPlayer;
using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;
using Avalonia.Media.Imaging;
using System.IO;
using System.Timers;
using LiteDB;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer_Avalonia.Views;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Linq;
using System.Diagnostics;
using Avalonia.Controls;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using ATL.Playlist;

namespace FRESHMusicPlayer_Avalonia.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public Player Player { get; private set; } = new();
        public Timer ProgressTimer { get; private set; } = new(100);
        public Library Library { get; private set; }

        public MainWindowViewModel()
        {
            var library = new LiteDatabase($"Filename=\"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "database.fdb2")}\";Connection=shared");
            Library = new Library(library);
            InitializeLibrary();
            Player.SongChanged += Player_SongChanged;
            Player.SongStopped += Player_SongStopped;
            Player.SongException += Player_SongException;
            ProgressTimer.Elapsed += ProgressTimer_Elapsed; // TODO: remove shared
        }

        private const string projectName = "FRESHMusicPlayer Cross-Platform Edition™ Dev. Build 4";
        private string windowTitle = projectName;
        public string WindowTitle
        {
            get => windowTitle;
            set => this.RaiseAndSetIfChanged(ref windowTitle, value);
        }

        #region Core
        private void Player_SongException(object? sender, PlaybackExceptionEventArgs e)
        {
            // TODO: error handling
        }

        private void Player_SongStopped(object? sender, EventArgs e)
        {
            Artist = "Nothing Playing";
            Title = "Nothing Playing";
            CoverArt = null;
            WindowTitle = projectName;
            ProgressTimer.Stop();
        }

        private void ProgressTimer_Elapsed(object sender, ElapsedEventArgs e) => ProgressTick();

        public void ProgressTick()
        {
            this.RaisePropertyChanged(nameof(CurrentTime));
            this.RaisePropertyChanged(nameof(CurrentTimeSeconds));
            this.RaisePropertyChanged(nameof(TotalTime));
            this.RaisePropertyChanged(nameof(TotalTimeSeconds));
            Player.AvoidNextQueue = false;
        }

        private void Player_SongChanged(object? sender, EventArgs e)
        {
            var track = new Track(Player.FilePath);
            Artist = track.Artist;
            Title = track.Title;
            if (track.EmbeddedPictures.Count != 0)
                CoverArt = new Bitmap(new MemoryStream(track.EmbeddedPictures[0].PictureData));
            this.RaisePropertyChanged(nameof(TotalTime));
            this.RaisePropertyChanged(nameof(TotalTimeSeconds));
            WindowTitle = $"{track.Artist} - {track.Title} | {projectName}";
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
                Debug.WriteLine($"CurrentTimeSeconds set. Value is {value}");
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

        public async void InitializeLibrary()
        {
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
        public void UpdateLibraryInfo() => LibraryInfoText = $"Tracks: {AllTracks.Count} ・ {TimeSpan.FromSeconds(AllTracks.Sum(x => x.Length)):hh\\:mm\\:ss}";

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
            AllTracks.Clear();
            foreach (var track in await Task.Run(() => Library.ReadTracksForArtist(artist)))
                AllTracks.Add(track);
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
            AllTracks.Clear();
            foreach (var track in await Task.Run(() => Library.ReadTracksForAlbum(album)))
                AllTracks.Add(track);
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

        private List<string> acceptableFilePaths = "*.wav;*.aiff;*.mp3;*.wma;*.3g2;*.3gp;*.3gp2;*.3gpp;*.asf;*.wmv;*.aac;*.adts;*.avi;*.m4a;*.m4a;*.m4v;*.mov;*.mp4;*.sami;*.smi;*.flac".Split(';').ToList();
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
                        Extensions = new List<string>(){"*"}
                    }
                }
            };
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var files = await dialog.ShowAsync(desktop.MainWindow);
                await Task.Run(() => Library.Import(files));
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
                        Extensions = new(){ "*.xspf", "*.asx", "*.wvx", "*.b4s", "*.m3u", "*.m3u8", "*.pls", "*.smil", "*.smi", "*.zpl"}
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
                await dialog.ShowAsync(desktop.MainWindow);
                var paths = Directory.EnumerateFiles(dialog.Directory, "*", SearchOption.AllDirectories)
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
        public void ImportFilePathCommand()
        {
            if (string.IsNullOrEmpty(FilePathOrURL)) return;
            Player.Queue.Add(FilePathOrURL);
            Library.Import(FilePathOrURL);
            Player.PlayMusic();
        }
        #endregion

        #region NavBar
        public void OpenSettingsCommand()
        {
            new Views.Settings().Show();
        }
        #endregion
    }
}
