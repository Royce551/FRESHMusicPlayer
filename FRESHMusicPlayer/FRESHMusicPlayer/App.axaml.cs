using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.ViewModels;
using FRESHMusicPlayer.Views;
using LiteDB;
using SIADL.Avalonia;
using System;
using System.IO;
using System.Linq;

namespace FRESHMusicPlayer;

public partial class App : Application
{
    public static string DataFolderLocation
    {
        get
        {
            if (Directory.Exists("Data")) return "Data";
            else return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer");
        }
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDeveloperTools();
#endif
    }

    public override void OnFrameworkInitializationCompleted()
    {
        //// Line below is needed to remove Avalonia data validation.
        //// Without this line you will get duplicate validations from both Avalonia and CT
        //BindingPlugins.DataValidators.RemoveAt(0);
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string[] initialFiles;
            if (desktop.Args != null && desktop.Args.Length > 0) initialFiles = [.. desktop.Args.Where(x => x.Contains('.'))];
            else initialFiles = [];

            var mainWindow = new MainWindow();
            mainWindow.DataContext = new MainViewModel(mainWindow, initialFiles);
            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
