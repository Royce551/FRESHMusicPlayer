using Avalonia.Controls;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Windows.Platform
{
    public class WindowsPlatformWrapper : IPlatformWrapper
    {
        public IAudioBackend GetPlatformAudioBackend(MainViewModel viewModel, Window window) => new NAudioBackend();

        public IPlaybackIntegration GetPlatformPlaybackIntegration(MainViewModel viewModel, Window window) => new SMTCIntegration(viewModel, window);
    }
}
