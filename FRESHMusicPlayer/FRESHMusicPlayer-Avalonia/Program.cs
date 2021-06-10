using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.ReactiveUI;
using FRESHMusicPlayer;
using FRESHMusicPlayer.Handlers.Configuration;
using FRESHMusicPlayer.Utilities;
using FRESHMusicPlayer.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FRESHMusicPlayer
{
    public class Program
    {
        public static ConfigurationFile Config;

        // Initialization code. Don't use any Avalonia, third-party APIs or any
        // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
        // yet and stuff might break.
        public static async Task Main(string[] args)
        {
            Config = await ConfigurationHandler.Read();
            if (Config.Language != "automatic") System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(Config.Language);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FRESHMusicPlayer", "Logs", $"{DateTime.Now:M.d.yyyy hh mm tt}.txt");
            File.WriteAllText(path,
            $":(\n" +
            "Sorry, FMP had to close because of a serious problem. If this keeps happening, please report this to the devs at https://github.com/royce551/freshmusicplayer/issues ! Include the following information:\n\n" +
            $"{MainWindowViewModel.ProjectName}\n" +
            $"{System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}\n" +
            $"{Environment.OSVersion.VersionString}\n" +
            $"{(Exception)e.ExceptionObject}");
            InterfaceUtils.OpenURL(path);
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace()
                .UseReactiveUI();
    }
}
