using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System;
using System.Linq;
using FRESHMusicPlayer.Views;
using FRESHMusicPlayer.Handlers;
using LiteDB;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Media;

namespace FRESHMusicPlayer.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNavbarVisible))]
    private ViewModelBase? selectedView;

    public bool IsNavbarVisible => true;

    private string? windowTitleOverride;
    public string? WindowTitleOverride
    {
        get => windowTitleOverride;
        set
        {
            windowTitleOverride = value;
            if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime && desktopLifetime.MainWindow != null)
                if (string.IsNullOrEmpty(windowTitleOverride)) desktopLifetime.MainWindow.Title = "Kotomi";
                else desktopLifetime.MainWindow.Title = windowTitleOverride;
        }
    }

    public Player Player { get; private set; }

    public GUILibrary Library { get; private set; }

    public MainWindow MainWindow { get; private set; } = default!;

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

        progressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        progressTimer.Tick += ProgressTimer_Tick;
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
        else Player.CurrentTime = TimeSpan.FromSeconds(0);
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
            Player.CurrentTime = TimeSpan.FromSeconds(value);
        }
    }

    [ObservableProperty]
    private double totalTimeSeconds = 1;
    [ObservableProperty]
    private Bitmap? coverArt = null;

    public const string WindowName = "FRESHMusicPlayer";

    private DispatcherTimer progressTimer;

    private void ProgressTick()
    {
        var time = Player.CurrentTime;
        ProgressIndicator1 = time.ToString(@"mm\:ss");


        OnPropertyChanged(nameof(CurrentTimeSeconds));

        Player.AvoidNextQueue = false;
        progressTimer.Start();
    }

    private void Player_SongException(object? sender, PlaybackExceptionEventArgs e)
    {
      
    }

    private void Player_SongStopped(object? sender, PlaybackStoppedEventArgs e)
    {
        progressTimer.Stop();

        if (e.IsEndOfPlayback)
        {
            Title = WindowName;
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

    private void Player_SongChanged(object? sender, EventArgs e)
    {
        // TODO: handle exceptions
        progressTimer.Start();

        WindowTitle = $"{Player.Metadata.Title} • {string.Join(", ", Player.Metadata.Artists)} - {WindowName}";
        Title = Player.Metadata.Title;
        Artist = string.Join(", ", Player.Metadata.Artists) == "" ? "No artist" : string.Join(", ", Player.Metadata.Artists);

        if (Player.CurrentBackend.TotalTime.TotalSeconds != 0) ProgressIndicator2 = Player.CurrentBackend.TotalTime.ToString(@"mm\:ss");
        else ProgressIndicator2 = "∞";

        TotalTimeSeconds = Player.TotalTime.TotalSeconds;

        if (Player.Metadata.CoverArt is null)
        {
            CoverArt = null;
        }
        else
        {
            CoverArt = new Bitmap(new MemoryStream(Player.Metadata.CoverArt));
        }
    }

    private void Player_SongLoading(object? sender, EventArgs e)
    {

    }

    private double volume;
    public double Volume
    {
        get => volume;
        set
        {
            SetProperty(ref volume, value);
            if (volume > 0.99) Player.Volume = 1;
            else if (volume < 0.01) Player.Volume = 0;
            else Player.Volume = (float)(((Math.Pow(Math.E, Math.Log(40) * volume)) / 40) * 1.066 - 0.02745);
        }
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
            if (path == currentSidePanePath)
            {
                await MainWindow.AnimateSidePaneOutAsync();

                currentSidePanePath = null;

                SidePaneView = null;

                return;
            }

            currentSidePanePath = path;

            SidePaneView = new ViewModelBase();
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

    public async void OpenSettingsCommand() => await OpenSidePaneAsync("FRESHMusicPlayer.Test", 450);
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
