using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Views;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavbarVisible))]
    private ViewModelBase? selectedView;


    public bool IsNavbarVisible => true;

    public Player Player { get; private set; }

    public GUILibrary Library { get; private set; }

    public MainWindow MainWindow { get; private set; } = default!;

    public ConfigurationFile Config { get; private set; } = default!;

    /// <summary>
    /// This is for the designer. Should not be used for any other purpose.
    /// </summary>
    public MainViewModel()
    {
    }

    public MainViewModel(MainWindow mainWindow)
    {
        this.MainWindow = mainWindow;

        if (!Design.IsDesignMode)
        {
        }

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
        UpdateVolume();

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
    }

    private void ProgressTimer_Tick(object? sender, EventArgs e) => ProgressTick();

    public bool Paused
    {
        get => Player.Paused;
        set
        {
            if (value) Player.Pause();
            else Player.Resume();
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

    private void Player_SongException(object? sender, PlaybackExceptionEventArgs e)
    {

    }

    private bool coverArtIsVisible = false;
    private void SetCoverArtVisibility(bool show)
    {
        if (show && !coverArtIsVisible)
        {
            coverArtIsVisible = true;
            _ = MainWindow.AnimateCoverArtShowAsync();
        }
        else if (!show && coverArtIsVisible)
        {
            coverArtIsVisible = false;
            _ = MainWindow.AnimateCoverArtHideAsync();
        }
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
        }
        else
        {
            WindowTitle = $"Loading... - {WindowName}";
            Title = "Loading...";
            Artist = "Loading...";
            CoverArt = null;
        }
    }

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
            SetCoverArtVisibility(false);
        }
        else
        {
            CoverArt = Bitmap.DecodeToWidth(new MemoryStream(Player.Metadata.CoverArt), 128);
            CoverArtFullSize = Bitmap.DecodeToWidth(new MemoryStream(Player.Metadata.CoverArt), 900); // doing these separately for clearer results
            SetCoverArtVisibility(true);
        }

        await AnimateProgressTo0Async();
        if (Player.FileLoaded)
        {
            TotalTimeSeconds = Player.TotalTime.TotalSeconds;
            ProgressTimer.Start();
        }
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
    private ViewModelBase? sidePaneView;

    [ObservableProperty]
    private double sidePanelWidth;

    private string? currentSidePanePath = null;

    public async Task OpenSidePaneAsync(string path, double width)
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && desktopLifetime.MainWindow != null)
        {
            if (currentSidePanePath != null)
            {
                if (path == currentSidePanePath)
                {
                    await MainWindow.AnimateSidePaneOutAsync();
                    currentSidePanePath = null;
                    SidePaneView = null;
                    return;
                }
                else
                {
                    await MainWindow.AnimateSidePaneOutAsync();
                    currentSidePanePath = null;
                    SidePaneView = null;
                }
            }

            currentSidePanePath = path;

            SidePaneView = path switch
            {
                "FRESHMusicPlayer.Queue" => new QueueViewModel(),
                "FRESHMusicPlayer.Settings" => new SettingsViewModel(this),
                _ => new ViewModelBase()
            };
            SidePaneView.MainView = this;
            SidePaneView.AfterPageLoaded();

            SidePanelWidth = width;
            await MainWindow.AnimateSidePaneInAsync(width);
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
