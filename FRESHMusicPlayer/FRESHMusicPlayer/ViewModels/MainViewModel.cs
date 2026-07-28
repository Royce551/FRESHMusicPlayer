using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.Views;
using LiteDB;
using SIADL.Avalonia;
using Splat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels;

public partial class MainViewModel : ViewModelBase, IRecipient<PropertyChangedMessage<bool>>
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavbarVisible))]
    public partial Control? SelectedView { get; set; }

    private ViewModelBase selectedViewModel;

    public bool IsNavbarVisible => true;

    public Player Player { get; private set; }

    public GUILibrary Library { get; private set; }

    public MainWindow MainWindow { get; private set; } = default!;

    public ConfigurationFile Config { get; private set; } = default!;

    public HttpClient HttpClient { get; private set; }

    public ObservableCollection<Notification> Notifications { get; private set; } = new ObservableCollection<Notification>();

    /// <summary>
    /// This is for the designer. Should not be used for any other purpose.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public MainViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    {
    }

    public MainViewModel(MainWindow mainWindow)
    {
        MainWindow = mainWindow;

        if (!Design.IsDesignMode)
        {
        }

        Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;

        var platformWrapper = Locator.Current.GetService<IPlatformWrapper>() ?? throw new PlatformNotSupportedException();
        BackendManager.LoadBackend(platformWrapper.GetPlatformAudioBackend(this, mainWindow));
        platformWrapper.SetupFMPCore();

        Player = new Player();
        Player.SongLoading += Player_SongLoading;
        Player.SongChanged += Player_SongChanged;
        Player.SongStopped += Player_SongStopped;
        Player.SongException += Player_SongException;

        Directory.CreateDirectory(App.DataFolderLocation);

        LiteDatabase library;
        //try
        //{
        library = new LiteDatabase(Path.Combine(App.DataFolderLocation, "database.fdb3"));

        Library = new GUILibrary(library, this);
        //}
        //catch (IOException)
        //{
        //    // TODO: single instance handling
        //}

        Config = ConfigurationFile.Read(Path.Combine(App.DataFolderLocation, "Configuration"));
        Config.IsActive = true;
        IsActive = true;
        UpdateRequestedTheme();

        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"FRESHMusicPlayer/{Assembly.GetEntryAssembly()!.GetName().Version} ( https://github.com/Royce551/FRESHMusicPlayer )");

        StartIntegrations();

        NavigateTo(Config.Page);

        ProgressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        ProgressTimer.Tick += ProgressTimer_Tick;

        PlaybackIntegrations.Add(platformWrapper.GetPlatformPlaybackIntegration(this, MainWindow));

        Notifications.CollectionChanged += Notifications_CollectionChanged;

        _ = PerformAutoImportAsync();
    }

    private void UIThread_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        string logPath = Path.Combine(App.DataFolderLocation, "Logs");
        string fileName = $"{DateTime.Now:s}.txt".Replace(':', '-');
        if (!Directory.Exists(logPath)) Directory.CreateDirectory(logPath);
        File.WriteAllText(Path.Combine(logPath, fileName),
            $"FRESHMusicPlayer {Assembly.GetEntryAssembly()?.GetName().Version}\n" +
            $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n" +
            $"{Environment.OSVersion.VersionString}\n" +
            $"{e.Exception}");
        Notifications.Add(new Notification(this)
        {
            ContentText = $"An error occurred :(\n\nPlease report this with the debug log at https://github.com/royce551/freshmusicplayer/issues.",
            ButtonText = "Open debug lug",
            Type = NotificationType.Failure,
            DisplayAsToast = true,
            OnButtonClicked = () =>
            {
                SIADLUtilities.OpenURL(logPath);
                SIADLUtilities.OpenURL(Path.Combine(logPath, fileName));
                return true;
            }
        });

        e.Handled = true;
    }

    public bool NotificationsNotEmpty => Notifications.Count > 0;
    public string? CurrentNotificationStatusBarText => Notifications.FirstOrDefault(x => !string.IsNullOrEmpty(x.StatusBarText))?.StatusBarText ?? null;

    public ObservableCollection<Notification> ActiveToastNotifications { get; private set; } = new();
    public bool ShowToastNotifications => SidePaneView is not NotificationsViewModel;

    private async void Notifications_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnPropertyChanged(nameof(NotificationsNotEmpty));
        OnPropertyChanged(nameof(CurrentNotificationStatusBarText));

        if (e.Action == NotifyCollectionChangedAction.Add)
        {
            foreach (var notif in e.NewItems!.OfType<Notification>())
            {
                if (notif.DisplayAsToast)
                {
                    ActiveToastNotifications.Add(notif);
                    if (notif.ToastDisplayTime != null)
                    {
                        await Task.Delay(notif.ToastDisplayTime.Value);
                        ActiveToastNotifications.Remove(notif);
                    }
                }
            }
        }
        else if (e.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var notif in e.OldItems!.OfType<Notification>())
            {
                ActiveToastNotifications.Remove(notif);
            }
        }
    }

    private void ProgressTimer_Tick(object? sender, EventArgs e) => ProgressTick();

    public bool Paused
    {
        get => Player.Paused;
        set
        {
            if (Player.FileLoaded)
            {
                if (value)
                {
                    Player.Pause();
                    if (Player.FileLoaded)
                    {
                        WindowTitle = WindowName;
                        _ = UpdateIntegrationsAsync(PlaybackStatus.Paused);
                    }

                }
                else
                {
                    Player.Resume();
                    if (Player.FileLoaded)
                    {
                        WindowTitle = $"{Player.Metadata.Title} • {string.Join(", ", Player.Metadata.Artists)} - {WindowName}";
                        _ = UpdateIntegrationsAsync(PlaybackStatus.Playing);
                    }

                }
            }
            
            OnPropertyChanged(nameof(Player.Paused));
            MainWindow.UpdateIconStates();
        }
    }

    public void TogglePause() => Paused = !Paused;

    public async void Next() => await Player.NextAsync();

    public async void Previous()
    {
        if (!Player.FileLoaded) return;
        if (CurrentTimeSeconds <= 5) await Player.PreviousAsync();
        else
        {
            Player.CurrentTime = TimeSpan.FromSeconds(0);
            await AnimateProgressTo0Async();
        }
    }

    public void ToggleShuffle()
    {
        Player.Queue.Shuffle = !Player.Queue.Shuffle;
        MainWindow.UpdateIconStates();
    }

    public void ToggleRepeat()
    {
        if (Player.Queue.RepeatMode == RepeatMode.None) Player.Queue.RepeatMode = RepeatMode.RepeatAll;
        else if (Player.Queue.RepeatMode == RepeatMode.RepeatAll) Player.Queue.RepeatMode = RepeatMode.RepeatOne;
        else Player.Queue.RepeatMode = RepeatMode.None;
        MainWindow.UpdateIconStates();
    }

    private double volumeBeforeMute;
    public void ToggleMute()
    {
        if (Volume != 0)
        {
            volumeBeforeMute = Volume;
            Volume = 0;
        }
        else Volume = volumeBeforeMute; // set directly, otherwise it'll be log scaled
    }

    [ObservableProperty]
    public partial bool PauseAfterCurrentTrack { get; set; } = false;

    [ObservableProperty]
    public partial string WindowTitle { get; set; } = WindowName;

    [ObservableProperty]
    public partial string Title { get; set; } = "Nothing playing";

    [ObservableProperty]
    public partial string Artist { get; set; } = "Nothing playing";

    [ObservableProperty]
    public partial string ProgressIndicator1 { get; set; } = "00:00";

    [ObservableProperty]
    public partial string ProgressIndicator2 { get; set; } = "00:00";

    //private double currentTimeSeconds = 0;
    public double CurrentTimeSeconds
    {
        get
        {
            if (Player.FileLoaded) return Player.CurrentTime.TotalSeconds;
            else return 0;
        }
        set
        {
            if (IsDragging && Player.FileLoaded)
                Player.CurrentTime = TimeSpan.FromSeconds(value);
        }
    }

    [ObservableProperty]
    public partial double TotalTimeSeconds { get; set; } = 1;

    [ObservableProperty]
    public partial Bitmap? CoverArt { get; set; } = null;

    [ObservableProperty]
    public partial Bitmap? CoverArtFullSize { get; set; } = null;

    public const string WindowName = "FRESHMusicPlayer";

    public DispatcherTimer ProgressTimer { get; private set; }

    private void ProgressTick()
    {
        var time = Player.CurrentTime;
        ProgressIndicator1 = time.ToString("mm\\:ss");

        if (Config.ShowRemainingTime) ProgressIndicator2 = $"-{time - Player.CurrentBackend.TotalTime:mm\\:ss}";
        if (Config.ShowTimeInWindow) WindowTitle = $"{time:mm\\:ss}/{Player.CurrentBackend.TotalTime:mm\\:ss} - {WindowName}";

        OnPropertyChanged(nameof(CurrentTimeSeconds));

        Player.AvoidNextQueue = false;
        ProgressTimer.Start();
    }

    public void ToggleShowRemainingTime()
    {
        var newShowRemainingTime = !Config.ShowRemainingTime;

        Config.ShowRemainingTime = newShowRemainingTime;
        if (ProgressTimer.IsEnabled && !newShowRemainingTime)
        {
            if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2 = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
            else ProgressIndicator2 = "∞";
        }
    }

    private async void Player_SongException(object? sender, PlaybackExceptionEventArgs e)
    {
        var message = new StringBuilder();
        message.AppendLine("A playback error occurred:");
        message.AppendLine();
        foreach (var problem in e.Problems)
        {
            var problemString = problem.Value switch
            {
                BackendLoadResult.NotSupported => "Not supported by this backend",
                BackendLoadResult.Invalid => "Invalid for this backend",
                BackendLoadResult.Corrupt => "File appears to be corrupt",
                BackendLoadResult.UnknownError => "Unknown error",
                _ => throw new InvalidOperationException()
            };
            message.AppendLine($"{problem.Key}: {problemString}");
        }
        if (Player.Queue.Position < Player.Queue.Queue.Count)
        {
            message.AppendLine();
            message.AppendLine("Skipped to the next track");
            await Task.Delay(100); // it's a little silly but it works
            Dispatcher.UIThread.Invoke(() => Next());
        }
        
        Notifications.Add(new Notification(this)
        {
            ContentText = message.ToString(),
            DisplayAsToast = true,
            ToastDisplayTime = TimeSpan.FromMinutes(1),
            Type = NotificationType.Failure
        });
    }

    private bool coverArtIsVisible = false;
    public bool SetCoverArtVisibility(bool show)
    {
        if (show && !coverArtIsVisible)
        {
            coverArtIsVisible = true;
            _ = MainWindow.AnimateCoverArtShowAsync();
            return true;
        }
        else if (!show && coverArtIsVisible)
        {
            coverArtIsVisible = false;
            _ = MainWindow.AnimateCoverArtHideAsync();
            return true;
        }
        return false;
    }

    private async void Player_SongStopped(object? sender, PlaybackStoppedEventArgs e)
    {
        ProgressTimer.Stop();

        if (e.IsEndOfPlayback)
        {
            WindowTitle = WindowName;
            SetCoverArtVisibility(false);
            await AnimateProgressTo0Async();
            OnPropertyChanged(nameof(CurrentTimeSeconds));
            ProgressIndicator1 = ProgressIndicator2 = "00:00";
            Title = Artist = "Nothing playing";
            CoverArt = null;
            _ = UpdateIntegrationsAsync(PlaybackStatus.Stopped);
        }
        else
        {
            WindowTitle = $"Loading... - {WindowName}";
            Title = "Loading...";
            Artist = "Loading...";
            //CoverArt = null;
            _ = UpdateIntegrationsAsync(PlaybackStatus.Changing);
        }
    }

    private IMetadataProvider? previousMetadata;

    private async void Player_SongChanged(object? sender, EventArgs e)
    {
        if (!Player.FileLoaded)
        {
            Debug.WriteLine("This is weird");
            return;
        }

        // TODO: handle exceptions
        WindowTitle = $"{Player.Metadata.Title} • {string.Join(", ", Player.Metadata.Artists)} - {WindowName}";
        Title = Player.Metadata.Title;
        Artist = string.Join(", ", Player.Metadata.Artists) == "" ? "No artist" : string.Join(", ", Player.Metadata.Artists);

        if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2 = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
        else ProgressIndicator2 = "∞";

        if (Player.Metadata.CoverArt is null)
        {
            CoverArt = null;
            if (SetCoverArtVisibility(false)) CoverArtChanged?.Invoke(null, EventArgs.Empty);
        }
        else _ = LoadCoverArtAsync();  

        _ = UpdateIntegrationsAsync(PlaybackStatus.Playing);
        UpdateReplayGain();

        if (PauseAfterCurrentTrack)
        {
            TogglePause();
            PauseAfterCurrentTrack = false;
        }

        MainWindow.UpdateIconStates();

        await AnimateProgressTo0Async();

        TotalTimeSeconds = Player.TotalTime.TotalSeconds;
        ProgressTimer.Start();  
    }
    public event EventHandler<EventArgs>? CoverArtChanged;

    private async Task LoadCoverArtAsync()
    {
        var coverChanged = !(previousMetadata?.CoverArt?.SequenceEqual(Player.Metadata.CoverArt) ?? false);
        if (previousMetadata == null || coverChanged || CoverArt == null)
        {
            await Task.Run(() =>
            {
                CoverArt = Bitmap.DecodeToWidth(new MemoryStream(Player.Metadata.CoverArt), 128);
                CoverArtFullSize = Bitmap.DecodeToWidth(new MemoryStream(Player.Metadata.CoverArt), 900); // doing these separately for clearer results
            });
            if (!coverArtIsVisible && currentSidePanePath != "FRESHMusicPlayer.TrackInfo")
                SetCoverArtVisibility(true);
            CoverArtChanged?.Invoke(null, EventArgs.Empty);
        }

        previousMetadata = Player.Metadata;
    }

    public bool IsDragging { get; set; } = false;

    private async Task AnimateProgressTo0Async()
    {
        await MainWindow.AnimateProgressTo0Async();
    }

    private void Player_SongLoading(object? sender, EventArgs e)
    {

    }

    public double Volume
    {
        get => Config.Volume;
        set
        {
            OnPropertyChanged(nameof(Volume));
            Config.Volume = value;

            Player.Volume = (float)value * replayGainAdjustment;
            //UpdateVolume();
        }
    }

    //private void UpdateVolume()
    //{
    //    if (Config.Volume > 0.99) Player.Volume = 1;
    //    else if (Config.Volume < 0.01) Player.Volume = 0;
    //    else Player.Volume = (float)(((Math.Pow(Math.E, Math.Log(40) * Config.Volume)) / 40) * 1.066 - 0.02745);
    //}

    private float replayGainAdjustment = 0;
    public void UpdateReplayGain()
    {
        if (!Config.UseReplayGain)
        {
            replayGainAdjustment = 1;
            return;
        }

        if (Player.Metadata is FileMetadataProvider file)
        {
            replayGainAdjustment = 1;

            float albumGain = 0;
            float albumPeak = 1;
            bool albumGainIsPresent = false;

            float trackGain = 0;
            float trackPeak = 1;
            bool trackGainIsPresent = false;

            if (file.ATLTrack.AdditionalFields.ContainsKey("replaygain_album_gain"))
            {
                float.TryParse(file.ATLTrack.AdditionalFields["replaygain_album_gain"].Replace("dB", string.Empty).Trim(), out albumGain);
                albumGainIsPresent = true;
            }
            if (file.ATLTrack.AdditionalFields.ContainsKey("replaygain_track_gain"))
            {
                float.TryParse(file.ATLTrack.AdditionalFields["replaygain_track_gain"].Replace("dB", string.Empty).Trim(), out trackGain);
                trackGainIsPresent = true;
            }
            if (file.ATLTrack.AdditionalFields.ContainsKey("replaygain_album_peak"))
                float.TryParse(file.ATLTrack.AdditionalFields["replaygain_album_peak"].Trim(), out albumPeak);
            if (file.ATLTrack.AdditionalFields.ContainsKey("replaygain_track_peak"))
                float.TryParse(file.ATLTrack.AdditionalFields["replaygain_track_peak"].Trim(), out trackPeak);

            float decibelsToAdjust = 0;
            float peak = 0;
            if (Config.PerformReplayGainByTrack)
            {
                decibelsToAdjust = trackGainIsPresent ? trackGain : albumGain;
                peak = trackPeak;
            }
            else if (Config.PerformReplayGainByAlbum)
            {
                decibelsToAdjust = albumGainIsPresent ? albumGain : trackGain;
                peak = albumPeak;
            }

            if (!trackGainIsPresent && !albumGainIsPresent)
            {
                LoggingHandler.Log("ReplayGain: Using fallback adjustment");
                decibelsToAdjust = (float)Config.ReplayGainFallbackPreAmp;
                peak = 1;
            }
            else decibelsToAdjust += (float)Config.ReplayGainPreAmp;

            replayGainAdjustment = Math.Min((float)Math.Pow(10, decibelsToAdjust / 20), (1 / peak));
            LoggingHandler.Log($"ReplayGain: Specified adjustment is {decibelsToAdjust}dB and peak is {peak}. Applying adjustment of {replayGainAdjustment}");
        }
        Player.Volume = (float)Volume * replayGainAdjustment;
    }

    public void HandleAppClosing()
    {
        var dataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Squidhouse Software", "Kotomi");
    }

    private Dictionary<Page, (ViewModelBase vm, Control pg)> viewModelCache = new();
    public void NavigateTo(Page pageType, object? args = null, bool skipCache = false)
    {
        ViewModelBase page;
        Control view;

        var pageIsInCache = viewModelCache.TryGetValue(pageType, out var cachedViewModel);
        if (!skipCache && pageIsInCache)
        {
            page = cachedViewModel.vm;
            view = cachedViewModel.pg;
        }
        else
        {
            if (pageIsInCache) cachedViewModel.vm.OnNavigatingAway();

            page = pageType switch
            {
                Page.Tracks => new TracksViewModel(),
                Page.Artists => new ArtistsViewModel(args as string),
                Page.Albums => new AlbumsViewModel(args as string),
                Page.Import => new ImportViewModel(),
                Page.Playlists => new PlaylistsViewModel(),
                _ => throw new InvalidOperationException()
            };
            view = page switch
            {
                TracksViewModel => new TracksView(),
                ArtistsViewModel => new ArtistsView(),
                AlbumsViewModel => new AlbumsView(),
                ImportViewModel => new ImportView(),
                PlaylistsViewModel => new PlaylistsView(),
                _ => throw new InvalidOperationException()
            };
            view.DataContext = page;

            viewModelCache[pageType] = (page, view);
            page.MainView = this;
            page.AfterPageLoaded();
        }

        backLog.Push((pageType, args, skipCache));

        SelectedView = view;
        selectedViewModel = page;

        Config.Page = pageType;

        OnPropertyChanged(nameof(TracksTabFontWeight));
        OnPropertyChanged(nameof(ArtistsTabFontWeight));
        OnPropertyChanged(nameof(AlbumsTabFontWeight));
        OnPropertyChanged(nameof(PlaylistsTabFontWeight));
        OnPropertyChanged(nameof(ImportTabFontWeight));
    }

    private Stack<(Page pageType, object? args, bool skipCache)> backLog = new(16);
    private Stack<(Page pageType, object? args, bool skipCache)> forwardLog = new(16);
    public void NavigateBack()
    {
        if (backLog.Count <= 1) return;

        var forwardPage = backLog.Pop();
        var lastPage = backLog.Pop();
        NavigateTo(lastPage.pageType, lastPage.args, lastPage.skipCache);
        forwardLog.Push(forwardPage);
    }

    public void NavigateForward()
    {
        if (forwardLog.Count == 0) return;

        var nextPage = forwardLog.Pop();
        NavigateTo(nextPage.pageType, nextPage.args, nextPage.skipCache);
        backLog.Push(nextPage);
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowToastNotifications))]
    public partial ViewModelBase? SidePaneView { get; set; }

    [ObservableProperty]
    public partial double SidePanelWidth { get; set; }

    private string? currentSidePanePath = null;

    public async Task OpenSidePaneAsync(string path, double width, bool onLeft = false)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && desktopLifetime.MainWindow != null)
        {
            if (currentSidePanePath != null)
            {
                if (path == currentSidePanePath)
                {
                    await MainWindow.AnimateSidePaneOutAsync();
                    SidePaneView?.OnNavigatingAway();
                    currentSidePanePath = null;
                    SidePaneView = null;
                    return;
                }
                else
                {
                    await MainWindow.AnimateSidePaneOutAsync();
                    SidePaneView?.OnNavigatingAway();
                    currentSidePanePath = null;
                    SidePaneView = null;
                }
            }

            currentSidePanePath = path;

            SidePaneView = path switch
            {
                "FRESHMusicPlayer.Queue" => new QueueViewModel(),
                "FRESHMusicPlayer.Settings" => new SettingsViewModel(this),
                "FRESHMusicPlayer.TrackInfo" => new TrackInfoViewModel(),
                "FRESHMusicPlayer.Lyrics" => new LyricsViewModel(this),
                "FRESHMusicPlayer.Notifications" => new NotificationsViewModel(this),
                "FRESHMusicPlayer.Search" => new SearchViewModel(),
                _ => new ViewModelBase()
            };
            SidePaneView.MainView = this;
            SidePaneView.AfterPageLoaded();

            SidePanelWidth = width;
            await MainWindow.AnimateSidePaneInAsync(width, onLeft);
        }
    }

    // this will need to be changed when tabs become more dynamic, but for now, this works
    public FontWeight TracksTabFontWeight => selectedViewModel is TracksViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight ArtistsTabFontWeight => selectedViewModel is ArtistsViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight AlbumsTabFontWeight => selectedViewModel is AlbumsViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight PlaylistsTabFontWeight => selectedViewModel is PlaylistsViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight ImportTabFontWeight => selectedViewModel is ImportViewModel ? FontWeight.Bold : FontWeight.Normal;

    public void OpenTracksTab() => NavigateTo(Page.Tracks);
    public void OpenArtistsTab() => NavigateTo(Page.Artists);
    public void OpenAlbumsTab() => NavigateTo(Page.Albums);
    public void OpenPlaylistsTab() => NavigateTo(Page.Playlists);
    public void OpenImportTab() => NavigateTo(Page.Import);
    public async void OpenSettingsCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Settings", 450);

    public async void OpenQueueCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Queue", 300);

    public async void OpenTrackInfoCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.TrackInfo", 250, true);

    public async void OpenLyricsCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Lyrics", 250, true);

    public async void OpenNotificationsCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Notifications", 300);

    public async void OpenSearchCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Search", 300);

    public async Task OpenPlaylistManagement()
    {
        var window = new PlaylistManagementWindow();
        var vm = new PlaylistManagementViewModel(Player.FilePath)
        {
            MainView = this
        };
        window.DataContext = vm;
        await vm.UpdatePlaylistsAsync();

        await window.ShowDialog(MainWindow);
    }

    public bool AutoQueueIsQueued { get; set; } = false;

    public void AddToQueueAndHandleAutoQueue(string[] filePaths)
    {
        if (AutoQueueIsQueued) Player.Queue.Clear();
        AutoQueueIsQueued = false;
        Player.Queue.Add(filePaths);
    }

    public void AddToQueueAndHandleAutoQueue(string filePath) => AddToQueueAndHandleAutoQueue([filePath]);

    public void GoToArtist()
    {
        NavigateTo(Page.Artists, Player.FileLoaded ? Player.Metadata.Artists[0] : null, true);
    }

    public void GoToAlbum()
    {
        NavigateTo(Page.Albums, Player.FileLoaded ? Player.Metadata.Album : null, true);
    }

    public List<IPlaybackIntegration> PlaybackIntegrations { get; } = new List<IPlaybackIntegration>();

    public async Task UpdateIntegrationsAsync(PlaybackStatus status)
    {
        await Task.WhenAll(PlaybackIntegrations.Select(x => x.UpdateAsync(Player.Metadata, status)));
    }

    private void StartIntegrations()
    {
        if (Config.IntegrateDiscordRichPresence) StartIntegration(new DiscordIntegration(HttpClient));
        if (Config.IntegrateLastFM) StartIntegration(new LastFMIntegration(this));
    }

    private void StartIntegration(IPlaybackIntegration integration)
    {
        if (!PlaybackIntegrations.Contains(integration))
            PlaybackIntegrations.Add(integration);
    }

    public void Receive(PropertyChangedMessage<bool> message)
    {
        if (message is { Sender: ConfigurationFile, PropertyName: nameof(ConfigurationFile.IntegrateDiscordRichPresence) })
        {
            if (Config.IntegrateDiscordRichPresence) StartIntegration(new DiscordIntegration(HttpClient));
            else
            {
                var discordIntegration = PlaybackIntegrations.OfType<DiscordIntegration>().FirstOrDefault();
                if (discordIntegration != null)
                {
                    discordIntegration.Close();
                    PlaybackIntegrations.Remove(discordIntegration);
                }
            }
        }
        else if (message is { Sender: ConfigurationFile, PropertyName: nameof(ConfigurationFile.IntegrateLastFM) })
        {
            if (Config.IntegrateLastFM) StartIntegration(new LastFMIntegration(this));
            else
            {
                var lastFMIntegration = PlaybackIntegrations.OfType<LastFMIntegration>().FirstOrDefault();
                if (lastFMIntegration != null)
                {
                    lastFMIntegration.Close();
                    PlaybackIntegrations.Remove(lastFMIntegration);
                }
            }
        }
        else if (message is { Sender: ConfigurationFile, PropertyName: nameof(ConfigurationFile.PreferDarkTheme) } or { Sender: ConfigurationFile, PropertyName: nameof(ConfigurationFile.PreferLightTheme) })
            UpdateRequestedTheme();
    }
 
    [ObservableProperty]
    public partial string? OpenDialogPath { get; set; }

    public void OpenDialogOpen()
    {
        if (string.IsNullOrEmpty(OpenDialogPath)) return;

        AddToQueueAndHandleAutoQueue(OpenDialogPath);
        Player.PlayAsync();
    }

    [ObservableProperty]
    public partial bool ShowDragDropOverlay { get; set; } = false;

    public void OpenMiniPlayer()
    {
        var miniplayer = new MiniPlayerWindow() { DataContext = new MiniPlayerViewModel(this) };
        miniplayer.Show(MainWindow);
    }

    [ObservableProperty]
    public partial bool IsShiftHeld { get; set; } = false;

    private bool CheckIfFileEndsWithAutoImportableFileExtension(string name) => name.EndsWith(".mp3")
        || name.EndsWith(".wav") || name.EndsWith(".m4a") || name.EndsWith(".ogg")
        || name.EndsWith(".flac") || name.EndsWith(".aiff")
        || name.EndsWith(".wma")
        || name.EndsWith(".aac");

    public async Task PerformAutoImportAsync()
    {
        if (Config.AutoImportPaths.Count > 0)
        {
            LoggingHandler.Log("Auto Import: Scanning for new tracks...");
            var notification = new Notification(this)
            {
                ContentText = "Scanning for new tracks...",
                StatusBarText = "Scanning for new tracks...",
                Type = NotificationType.Progress,
            };
            Notifications.Add(notification);
            var filesToImport = new List<string>();
            var library = Library.GetAllTracks();
            await Task.Run(async () =>
            {
                foreach (var folder in Config.AutoImportPaths)
                {
                    var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                        .Where(name => CheckIfFileEndsWithAutoImportableFileExtension(name)).ToArray();
                    foreach (var file in files)
                    {
                        if (!library.Select(x => x.Path).Contains(file))
                            filesToImport.Add(file);
                    }
                }
                if (filesToImport.Count > 0) await Library.ImportAsync(filesToImport);
            });
            Notifications.Remove(notification);
        }
        
        foreach (var folder in Config.AutoImportPaths)
            AddAutoImportFileWatcher(folder);

        var watchersToRemove = autoImportFileWatches
            .Where(x => !Config.AutoImportPaths.Any(p => string.Equals(p, x.Path)))
            .ToList();

        foreach (var match in watchersToRemove)
        {
            LoggingHandler.Log($"Auto Import: Folder removed from config, removing auto import watch for {match.Path}");
            try
            {
                match.EnableRaisingEvents = false;
                match.Dispose();
            }
            catch
            {
                // ignored
            }

            autoImportFileWatches.Remove(match);
        }
    }
    private List<FileSystemWatcher> autoImportFileWatches = new();
    public void AddAutoImportFileWatcher(string folder)
    {
        if (autoImportFileWatches.Any(x => x.Path == folder)) return;

        LoggingHandler.Log($"Auto Import: Creating file watcher for {folder}");
        var autoImportPathWatcher = new FileSystemWatcher(folder)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };
        autoImportPathWatcher.Created += async (s, e) =>
        {
            LoggingHandler.Log($"Auto Import: {e.FullPath} was created, importing...");

            var attributes = File.GetAttributes(e.FullPath);
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                var filesToImport = new List<string>();
                var files = Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories)
                            .Where(name => CheckIfFileEndsWithAutoImportableFileExtension(name)).ToArray();
                foreach (var file in files)
                {
                    if (!Library.GetAllTracks().Select(x => x.Path).Contains(file))
                        filesToImport.Add(file);
                }
                await Library.ImportAsync(filesToImport);
            }
            else
            {
                if (CheckIfFileEndsWithAutoImportableFileExtension(e.FullPath) && !Library.GetAllTracks().Select(x => x.Path).Contains(e.FullPath))
                {
                    await Library.ImportAsync(e.FullPath);
                }
            }
        };

        autoImportFileWatches.Add(autoImportPathWatcher);
    }

    public void UpdateRequestedTheme()
    {
        if (App.Current is null) throw new InvalidOperationException();

        if (Config.PreferDarkTheme) App.Current.RequestedThemeVariant = ThemeVariant.Dark;
        else if (Config.PreferLightTheme) App.Current.RequestedThemeVariant = ThemeVariant.Light;
        else App.Current.RequestedThemeVariant = ThemeVariant.Default;
    }
}

public enum Page
{
    Tracks,
    Artists,
    Albums,
    Playlists,
    Import
}

public class CombineMarginsConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (!values.All(x => x is Thickness)) throw new NotSupportedException();

        var valuesAsThickness = values.OfType<Thickness>();

        var y = new Thickness(valuesAsThickness.Sum(x => x.Left),
                             valuesAsThickness.Sum(x => x.Top),
                             valuesAsThickness.Sum(x => x.Right),
                             valuesAsThickness.Sum(x => x.Bottom));
        return y;
    }
}
