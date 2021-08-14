using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;
using System;

namespace FRESHMusicPlayer.ViewModels
{
    public class ViewModelBase : ReactiveObject
    {
        public Window GetMainWindow()
        {
            if (Avalonia.Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                return desktop.MainWindow;
            else throw new Exception("fucky wucky");
        }
    }
}
