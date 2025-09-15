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
