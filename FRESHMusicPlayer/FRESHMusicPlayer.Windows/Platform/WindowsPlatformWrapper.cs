using Avalonia.Controls;
using FRESHMusicPlayer.Backends;
using FRESHMusicPlayer.Handlers;
using FRESHMusicPlayer.Handlers.PlaybackIntegrations;
using FRESHMusicPlayer.ViewModels;
using NAudio.Wave;
using NReplayGain;
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

        public TrackGain CalculateReplayGainDataForTrack(string path)
        {
            using var fileReader = new AudioFileReader(path);
            var sampleRate = fileReader.WaveFormat.SampleRate;
            var trackGain = new TrackGain(sampleRate);

            var buffer = new float[sampleRate * 2];
            while (true)
            {
                var samplesRead = fileReader.Read(buffer, 0, buffer.Length);
                if (samplesRead == 0)
                {
                    break;
                }

                trackGain.AnalyzeSamples(buffer, samplesRead);
            }

            return trackGain;
        }

        public void SetupFMPCore()
        {
            BackendManager.AddDefaultSearchDirectories();
        }
    }
}
