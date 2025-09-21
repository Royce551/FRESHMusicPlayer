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

    private MainWindow mainWindow = default!;

    /// <summary>
    /// This is for the designer. Should not be used for any other purpose.
    /// </summary>
    public MainViewModel()
    {   
    }

    public MainViewModel(MainWindow mainWindow)
    {
        this.mainWindow = mainWindow;

        if (!Design.IsDesignMode)
        {
        }

        Player = new Player();
        Player.SongLoading += Player_SongLoading;
        Player.SongChanged += Player_SongChanged;
        Player.SongStopped += Player_SongStopped;
        Player.SongException += Player_SongException;

        LiteDatabase library;
        try
        {
            library = new LiteDatabase(Path.Combine(App.DataFolderLocation, "database.fdb3"));

            Library = new GUILibrary(library, this);
        }
        catch (IOException)
        {
            // TODO: single instance handling
        }

        progressTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        progressTimer.Tick += ProgressTimer_Tick;
    }

    private void ProgressTimer_Tick(object? sender, EventArgs e) => ProgressTick();

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
    [ObservableProperty]
    private double currentTimeSeconds = 0;
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

        CurrentTimeSeconds = time.TotalSeconds;
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
            CurrentTimeSeconds = 0;
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

    public void OpenTracksTab() => NavigateTo(new TracksViewModel());

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
                await mainWindow.AnimateSidePaneOutAsync();

                currentSidePanePath = null;

                SidePaneView = null;

                return;
            }

            currentSidePanePath = path;

            SidePaneView = new ViewModelBase();
            SidePanelWidth = width;
            await mainWindow.AnimateSidePaneInAsync(width);
        }
    }

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
