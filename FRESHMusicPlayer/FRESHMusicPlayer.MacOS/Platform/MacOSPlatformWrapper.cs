using Avalonia.Controls;
using FmpBassBackend;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FRESHMusicPlayer.MacOS.Platform
{
    public class MacOSPlatformWrapper : IPlatformWrapper
    {
        public IAudioBackend GetPlatformAudioBackend(MainViewModel viewModel, Window window) => new BassBackend();

        public IPlaybackIntegration GetPlatformPlaybackIntegration(MainViewModel viewModel, Window window) => new MacOSPlaybackIntegration();

        public void SetupFMPCore()
        {
            // TODO: no search search directory inside the bundle but the one in the data folder is probably fine
        }
    }

    public class MacOSPlaybackIntegration : IPlaybackIntegration // not implemented yet, but since it's required this is just a dummy
    {
        public void Close()
        {

        }

        public Task UpdateAsync(IMetadataProvider track, PlaybackStatus status)
        {
            return Task.CompletedTask;
        }
    }
}
