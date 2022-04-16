using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using FRESHMusicPlayer.Views;

namespace FRESHMusicPlayer
{
    public class App : Application
    {
        public static string DataFolderLocation
        {
            get
            {
                if (Directory.Exists("Data")) return "Data";
                var applicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (applicationData == "") return "Data"; // apparently can happen on macos? just fall back to standalone mode
                else return Path.Combine(applicationData, "FRESHMusicPlayer");
            }
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };

            }
            Name = MainWindowViewModel.ProjectName;
            base.OnFrameworkInitializationCompleted();
        }
    }
}
