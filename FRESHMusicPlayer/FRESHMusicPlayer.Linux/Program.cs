using Avalonia;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Linux.Platform;
using Splat;
using System;

namespace FRESHMusicPlayer.Desktop;

class Program
{
    private static bool disableWayland = false;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (args.Contains("--disable-wayland")) disableWayland = true;

        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }
    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        Locator.CurrentMutable.Register<IPlatformWrapper>(() => new LinuxPlatformWrapper());
        
        var appBuilder = AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace();

        if (!disableWayland) appBuilder = appBuilder.UseWayland();

        return appBuilder;
    }
}
