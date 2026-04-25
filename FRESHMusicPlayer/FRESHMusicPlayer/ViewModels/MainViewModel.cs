using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
    private ViewModelBase? selectedView;


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
        UpdateVolume();

        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd($"FRESHMusicPlayer/{Assembly.GetEntryAssembly()!.GetName().Version} ( https://github.com/Royce551/FRESHMusicPlayer )");

        StartIntegrations();

        NavigateTo(Config.Page switch
        {
            Page.Tracks => new TracksViewModel(),
            Page.Artists => new ArtistsViewModel(),
            Page.Albums => new ArtistsViewModel(),
            Page.Playlists => new PlaylistsViewModel(),
            Page.Import => new ImportViewModel(),
            _ => new TracksViewModel(),
        });

        ProgressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        ProgressTimer.Tick += ProgressTimer_Tick;

        PlaybackIntegrations.Add(platformWrapper.GetPlatformPlaybackIntegration(this, MainWindow));

        Notifications.CollectionChanged += Notifications_CollectionChanged;
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
            if (value)
            {
                Player.Pause();
                WindowTitle = WindowName;
                _ = UpdateIntegrationsAsync(PlaybackStatus.Paused);
            }
            else
            {
                Player.Resume();
                WindowTitle = $"{Player.Metadata.Title} • {string.Join(", ", Player.Metadata.Artists)} - {WindowName}";
                _ = UpdateIntegrationsAsync(PlaybackStatus.Playing);
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
    private bool pauseAfterCurrentTrack = false;

    [ObservableProperty]
    private string windowTitle = WindowName;
    [ObservableProperty]
    private string title = "Nothing playing";
    [ObservableProperty]
    private string artist = "Nothing playing";
    [ObservableProperty]
    private string progressIndicator1 = "00:00";
    [ObservableProperty]
    private string progressIndicator2 = "00:00";

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
    private double totalTimeSeconds = 1;
    [ObservableProperty]
    private Bitmap? coverArt = null;
    [ObservableProperty]
    private Bitmap? coverArtFullSize = null;

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

        if (PauseAfterCurrentTrack)
        {
            TogglePause();
            PauseAfterCurrentTrack = false;
        }

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

            UpdateVolume();
        }
    }

    private void UpdateVolume()
    {
        if (Config.Volume > 0.99) Player.Volume = 1;
        else if (Config.Volume < 0.01) Player.Volume = 0;
        else Player.Volume = (float)(((Math.Pow(Math.E, Math.Log(40) * Config.Volume)) / 40) * 1.066 - 0.02745);
    }

    public void HandleAppClosing()
    {
        var dataFolderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Squidhouse Software", "Kotomi");
    }

    public void NavigateTo(ViewModelBase page)
    {
        SelectedView?.OnNavigatingAway();

        page.MainView = this;
        SelectedView = page;
        page.AfterPageLoaded();

        Config.Page = page switch
        {
            TracksViewModel => Page.Tracks,
            ArtistsViewModel => Page.Artists,
            AlbumsViewModel => Page.Albums,
            PlaylistsViewModel => Page.Playlists,
            ImportViewModel => Page.Import,
            _ => Page.Tracks
        };

        OnPropertyChanged(nameof(TracksTabFontWeight));
        OnPropertyChanged(nameof(ArtistsTabFontWeight));
        OnPropertyChanged(nameof(AlbumsTabFontWeight));
        OnPropertyChanged(nameof(PlaylistsTabFontWeight));
        OnPropertyChanged(nameof(ImportTabFontWeight));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowToastNotifications))]
    private ViewModelBase? sidePaneView;

    [ObservableProperty]
    private double sidePanelWidth;

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
    public FontWeight TracksTabFontWeight => SelectedView is TracksViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight ArtistsTabFontWeight => SelectedView is ArtistsViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight AlbumsTabFontWeight => SelectedView is AlbumsViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight PlaylistsTabFontWeight => SelectedView is PlaylistsViewModel ? FontWeight.Bold : FontWeight.Normal;
    public FontWeight ImportTabFontWeight => SelectedView is ImportViewModel ? FontWeight.Bold : FontWeight.Normal;

    public void OpenTracksTab() => NavigateTo(new TracksViewModel());
    public void OpenArtistsTab() => NavigateTo(new ArtistsViewModel());
    public void OpenAlbumsTab() => NavigateTo(new AlbumsViewModel());
    public void OpenPlaylistsTab() => NavigateTo(new PlaylistsViewModel());
    public void OpenImportTab() => NavigateTo(new ImportViewModel());

    public async void OpenSettingsCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Settings", 450);

    public async void OpenQueueCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Queue", 300);

    public async void OpenTrackInfoCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.TrackInfo", 250, true);

    public async void OpenLyricsCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Lyrics", 250, true);

    public async void OpenNotificationsCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Notifications", 300);

    public async void OpenSearchCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Search", 300);

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
        NavigateTo(new ArtistsViewModel(Player.FileLoaded ? Player.Metadata.Artists[0] : null));
    }

    public void GoToAlbum()
    {
        NavigateTo(new AlbumsViewModel(Player.FileLoaded ? Player.Metadata.Album : null));
    }

    public List<IPlaybackIntegration> PlaybackIntegrations { get; } = new List<IPlaybackIntegration>();

    public async Task UpdateIntegrationsAsync(PlaybackStatus status)
    {
        await Task.WhenAll(PlaybackIntegrations.Select(x => x.UpdateAsync(Player.Metadata, status)));
    }

    private void StartIntegrations()
    {
        if (Config.IntegrateDiscordRichPresence) StartIntegration(new DiscordIntegration(HttpClient));
    }

    private void StartIntegration(IPlaybackIntegration integration)
    {
        if (!PlaybackIntegrations.Contains(integration))
            PlaybackIntegrations.Add(integration);
    }

    public void Receive(PropertyChangedMessage<bool> message)
    {
        if (message is { Sender: ConfigurationFile, PropertyName: nameof(ConfigurationFile.IntegrateDiscordRichPresence)})
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
    }

    [ObservableProperty]
    private string? openDialogPath;

    public void OpenDialogOpen()
    {
        if (string.IsNullOrEmpty(OpenDialogPath)) return;

        AddToQueueAndHandleAutoQueue(OpenDialogPath);
        Player.PlayAsync();
    }

    [ObservableProperty]
    private bool showDragDropOverlay = false;
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
