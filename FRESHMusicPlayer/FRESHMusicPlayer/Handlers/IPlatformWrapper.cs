using Avalonia.Controls;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Handlers
{
    public interface IPlatformWrapper
    {
        IPlaybackIntegration GetPlatformPlaybackIntegration(MainViewModel viewModel, Window window);
    }
}
