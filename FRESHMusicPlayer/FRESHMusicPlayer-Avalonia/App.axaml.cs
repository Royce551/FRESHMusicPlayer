using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using FRESHMusicPlayer.ViewModels;
using FRESHMusicPlayer.Views;

namespace FRESHMusicPlayer
{
    public class App : Application
    {
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
