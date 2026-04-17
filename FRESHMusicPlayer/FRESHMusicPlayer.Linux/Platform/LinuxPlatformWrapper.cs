using Avalonia.Controls;
using FmpBassBackend;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.Linux.Platform
{
    public class LinuxPlatformWrapper : IPlatformWrapper
    {
        public IAudioBackend GetPlatformAudioBackend(MainViewModel viewModel, Window window) => new BassBackend();

        public IPlaybackIntegration GetPlatformPlaybackIntegration(MainViewModel viewModel, Window window) => new MPRISIntegration(viewModel, window);
    }
}
