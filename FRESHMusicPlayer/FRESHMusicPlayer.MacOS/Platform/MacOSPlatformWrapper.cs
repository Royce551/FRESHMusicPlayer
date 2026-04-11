using Avalonia.Controls;
using FmpBassBackend;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace FRESHMusicPlayer.MacOS.Platform
{
    public class MacOSPlatformWrapper : IPlatformWrapper
    {
        public IAudioBackend GetPlatformAudioBackend(MainViewModel viewModel, Window window) => new BassBackend();

        public IPlaybackIntegration GetPlatformPlaybackIntegration(MainViewModel viewModel, Window window)
        {
            return null;
        }
    }
}
